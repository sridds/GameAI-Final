using Kart.Track;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kart.Car
{
    [RequireComponent(typeof(KartController))]
    public class KartAgent : Agent, IKartInput
    {
        private const float STUCK_THRESHOLD = 5f;
        private const float STUCK_DISTANCE = 0.02f;

        [FormerlySerializedAs("wallBumper")] [SerializeField]
        private WallSensor wallSensor;

        [SerializeField] private CheckpointSensor checkpointSensor;

        [Header("Reward Settings")] [SerializeField]
        private float checkpointRewardMultiplier = 1f;

        [SerializeField] private float wallPenaltyMultiplier = 1f;
        [SerializeField] private float timePenaltyPerSecond = -0.001f;
        [SerializeField] private float velocityRewardScale = 0.0005f;
        [SerializeField] private float facingCheckpointReward = 0.0002f;

        [Header("Episode Settings")] [SerializeField]
        private float maxEpisodeTime = 180f;

        [Header("Debug Settings")] [SerializeField]
        private bool debugLogs;

        private int _checkpointsThisEpisode;

        private float _episodeTime;

        private KartController _kart;

        private Vector3 _lastPosition;

        private Checkpoint _nextCheckpoint;
        private float _stuckTimer;

        // private Transform spawnPosition;

        private Vector3 spawnPos;
        private Quaternion spawnRot;

        [Header("Sensors")] private CheckpointTrack track;

        private float _rewardsThisEpisode;

        protected override void Awake()
        {
            base.Awake();
            _kart = GetComponent<KartController>();
            _kart.SetInputSource(this);
            spawnPos = transform.position;
            spawnRot = transform.rotation;
            track = FindFirstObjectByType<CheckpointTrack>();

            Time.fixedDeltaTime = 0.02f;
            Physics.simulationMode = SimulationMode.FixedUpdate;
        }

        private void FixedUpdate()
        {
            // if (Vector3.Distance(transform.position, _lastPosition) < STUCK_DISTANCE)
            // {
            //     _stuckTimer += Time.fixedDeltaTime;
            //     if (_stuckTimer >= STUCK_THRESHOLD)
            //     {
            //         Debug.Log("Stuck, ending episode");
            //         AddReward(-0.5f);
            //         EndEpisode();
            //     }
            // }
            // else
            // {
            //     _stuckTimer = 0f;
            //     _lastPosition = transform.position;
            // }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            checkpointSensor.OnCheckpointPassed += OnCheckpointPassed;
            wallSensor.OnApplyPenalty += OnWallPenalty;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            checkpointSensor.OnCheckpointPassed -= OnCheckpointPassed;
            wallSensor.OnApplyPenalty -= OnWallPenalty;
        }

        public float Throttle { get; private set; }
        public float Steering { get; private set; }
        public bool IsBraking => false;
        public bool IsDrifting => false;

        private void UpdateNextCheckpoint()
        {
            if (track != null && checkpointSensor != null) _nextCheckpoint = track.GetNextCheckpoint(checkpointSensor);
        }

        private new void AddReward(float increment)
        {
            _rewardsThisEpisode += increment;
            base.AddReward(increment);
        }

        private new void EndEpisode()
        {
            Debug.Log($"Episode reward: {_rewardsThisEpisode:F2}");
            base.EndEpisode();
        }

        private void OnCheckpointPassed(CheckpointPassedEvent evt)
        {
            if (evt.IsForward)
            {
                var reward = evt.RewardMultiplier * checkpointRewardMultiplier;
                AddReward(reward);
                _checkpointsThisEpisode++;
                if (_checkpointsThisEpisode >= track.CheckpointCount)
                {
                    Debug.Log("Finished track!");
                    AddReward(2f);
                    EndEpisode();
                }
            }
            else
            {
                var penalty = evt.RewardMultiplier * checkpointRewardMultiplier;
                AddReward(penalty);
                EndEpisode();
            }
            

            UpdateNextCheckpoint();
        }

        private void OnWallPenalty(float penalty)
        {
            AddReward(-Mathf.Abs(penalty) * wallPenaltyMultiplier);
        }

        public void ResetPosition(bool randomize = false)
        {
            // Reset position
            transform.position = spawnPos;

            // Reset rigidbody
            _kart.Rb.linearVelocity = Vector3.zero;
            _kart.Rb.angularVelocity = Vector3.zero;
            _kart.Rb.position = spawnPos;
            _kart.Rb.rotation = spawnRot;

            transform.position = spawnPos;
            transform.rotation = spawnRot;
        }

        public override void OnEpisodeBegin()
        {
            _episodeTime = 0f;
            _checkpointsThisEpisode = 0;
            _rewardsThisEpisode = 0;

            ResetPosition();

            // Reset kart state
            _kart.ResetState();

            // Reset checkpoint sensor
            if (checkpointSensor != null)
                checkpointSensor.Reset();

            UpdateNextCheckpoint();

            _lastPosition = transform.position;
            _stuckTimer = 0f;
        }

        // 6 Observations
        public override void CollectObservations(VectorSensor sensor)
        {
            if (_nextCheckpoint != null)
            {
                var toCheckpoint = _nextCheckpoint.Col.bounds.center - transform.position;
                var dirToCheckpoint = toCheckpoint.normalized;

                var forwardDot = Vector3.Dot(transform.forward, dirToCheckpoint);
                sensor.AddObservation(forwardDot); // facing the checkpoint
                if (debugLogs) Debug.Log($"ForwardDot: {forwardDot}");

                var signedAngle = Vector3.SignedAngle(transform.forward, dirToCheckpoint, Vector3.up);
                sensor.AddObservation(signedAngle / 180f); // turn direction
                if (debugLogs) Debug.Log($"SignedAngle: {signedAngle}");

                var distance = toCheckpoint.magnitude;
                sensor.AddObservation(Mathf.Clamp01(distance / 50f)); // distance
                if (debugLogs) Debug.Log($"Distance: {distance}");
            }
            else
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }

            var localVelocity = transform.InverseTransformDirection(_kart.Rb.linearVelocity);
            sensor.AddObservation(localVelocity.z / 30f); // forward speed
            if (debugLogs) Debug.Log($"LocalVelocity: {localVelocity}");
            sensor.AddObservation(localVelocity.x / 15f); // lateral slide
            if (debugLogs) Debug.Log($"Velocity: {_kart.Rb.linearVelocity}");
            sensor.AddObservation(_kart.Rb.linearVelocity.magnitude / 30f); // total speed
            if (debugLogs) Debug.Log($"Velocity: {_kart.Rb.linearVelocity.magnitude}");
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            // Action 0 is throttle
            Throttle = actions.DiscreteActions[0] switch
            {
                0 => 0f, // none
                1 => 1f, // forward
                2 => -1f, // reverse
                _ => 0f
            };

            // Action 1 is steering
            Steering = actions.DiscreteActions[1] switch
            {
                0 => 0f, // none
                1 => 1f, // right 
                2 => -1f, // left
                _ => 0f
            };

            // Time penalty to encourage speed
            AddReward(timePenaltyPerSecond);

            // Reward for moving toward checkpoint
            if (_nextCheckpoint != null)
            {
                var toCheckpoint = (_nextCheckpoint.Col.bounds.center - transform.position).normalized;
                var velocityToward = Vector3.Dot(_kart.Rb.linearVelocity, toCheckpoint);

                if (velocityToward > 0)
                    AddReward(velocityToward * velocityRewardScale);

                // Small reward for facing checkpoint
                var facingDot = Vector3.Dot(transform.forward, toCheckpoint);
                if (facingDot > 0)
                    AddReward(facingDot * facingCheckpointReward);
            }

            _episodeTime += Time.fixedDeltaTime;

            if (_episodeTime >= maxEpisodeTime)
            {
                AddReward(-0.5f); // timeout
                Debug.Log("Episode End due to timeout");
                EndEpisode();
            }
        }
        
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var discreteActions = actionsOut.DiscreteActions;

            // Throttle
            var forwardAction = 0;
            if (Input.GetKey(KeyCode.W)) forwardAction = 1;
            if (Input.GetKey(KeyCode.S)) forwardAction = 2;
            discreteActions[0] = forwardAction;

            // Steering
            var turnAction = 0;
            if (Input.GetKey(KeyCode.D)) turnAction = 1;
            if (Input.GetKey(KeyCode.A)) turnAction = 2;
            discreteActions[1] = turnAction;
        }
    }
}