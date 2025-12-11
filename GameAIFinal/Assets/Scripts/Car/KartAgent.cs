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
        public bool IsTrainingMode = false;
        private const float STUCK_THRESHOLD = 5f;
        private const float STUCK_DISTANCE = 0.02f;

        [FormerlySerializedAs("wallBumper")]
        [SerializeField]
        private WallSensor wallSensor;

        [SerializeField] private CheckpointSensor checkpointSensor;

        [Header("Reward Settings")]
        [SerializeField]
        private float checkpointRewardMultiplier = 1f;

        [SerializeField] private float wallPenaltyMultiplier = 1f;
        [SerializeField] private float timePenaltyPerSecond = -0.001f;
        [SerializeField] private float velocityRewardScale = 0.0005f;
        [SerializeField] private float facingCheckpointReward = 0.0002f;

        [Header("Episode Settings")]
        [SerializeField]
        private float maxEpisodeTime = 180f;

        [Header("Debug Settings")]
        [SerializeField]
        private bool debugLogs;

        private int _checkpointsThisEpisode;
        private float _episodeTime;
        private KartController _kart;
        private Vector3 _lastPosition;
        private Checkpoint _nextCheckpoint;
        private float _rewardsThisEpisode;
        private float _stuckTimer;
        private Vector3 spawnPos;
        private Quaternion spawnRot;
        private CheckpointTrack track;

        // gameplay mode state
        private bool _gameplayInitialized = false;

        protected override void Awake()
        {
            base.Awake();
            _kart = GetComponent<KartController>();
            _kart.SetInputSource(this);
            track = FindFirstObjectByType<CheckpointTrack>();

            // only set physics in training mode
            if (IsTrainingMode)
            {
                Time.fixedDeltaTime = 0.02f;
                Physics.simulationMode = SimulationMode.FixedUpdate;
            }
        }

        private void Start()
        {
            spawnPos = transform.position;
            spawnRot = transform.rotation;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (checkpointSensor != null)
                checkpointSensor.OnCheckpointPassed += OnCheckpointPassed;
            if (wallSensor != null)
                wallSensor.OnApplyPenalty += OnWallPenalty;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (checkpointSensor != null)
                checkpointSensor.OnCheckpointPassed -= OnCheckpointPassed;
            if (wallSensor != null)
                wallSensor.OnApplyPenalty -= OnWallPenalty;
        }

        public float Throttle { get; private set; }
        public float Steering { get; private set; }
        public bool IsBraking => false;
        public bool IsDrifting => false;

        private void UpdateNextCheckpoint()
        {
            if (track != null && checkpointSensor != null)
                _nextCheckpoint = track.GetNextCheckpoint(checkpointSensor);
        }

        private new void AddReward(float increment)
        {
            // only track rewards in training mode
            if (IsTrainingMode)
            {
                _rewardsThisEpisode += increment;
                base.AddReward(increment);
            }
        }

        private new void EndEpisode()
        {
            if (IsTrainingMode)
            {
                Debug.Log($"Episode reward: {_rewardsThisEpisode:F2}");
                base.EndEpisode();
            }
        }

        private void OnCheckpointPassed(CheckpointPassedEvent evt)
        {
            if (evt.IsForward)
            {
                var reward = evt.RewardMultiplier * checkpointRewardMultiplier;
                AddReward(reward);
                _checkpointsThisEpisode++;

                if (IsTrainingMode && _checkpointsThisEpisode >= track.CheckpointCount)
                {
                    Debug.Log("finished track!");
                    AddReward(2f);
                    EndEpisode();
                }
            }
            else
            {
                var penalty = evt.RewardMultiplier * checkpointRewardMultiplier;
                AddReward(penalty);
                if (IsTrainingMode)
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
            // reset main transform
            transform.position = spawnPos;
            transform.rotation = spawnRot;

            // reset rigidbody position and rotation
            _kart.Rb.position = spawnPos;
            _kart.Rb.rotation = spawnRot;
            _kart.Rb.linearVelocity = Vector3.zero;
            _kart.Rb.angularVelocity = Vector3.zero;

            // reset kart state (this will reset child transforms)
            _kart.ResetState();
        }

        public override void OnEpisodeBegin()
        {
            _episodeTime = 0f;
            _checkpointsThisEpisode = 0;
            _rewardsThisEpisode = 0;

            ResetPosition();

            // reset checkpoint sensor
            if (checkpointSensor != null)
                checkpointSensor.Reset();

            UpdateNextCheckpoint();

            _lastPosition = transform.position;
            _stuckTimer = 0f;
        }

        // 6 total observations about the kart's state and environment
        public override void CollectObservations(VectorSensor sensor)
        {
            if (_nextCheckpoint != null)
            {
                // checkpoint directions
                var toCheckpoint = _nextCheckpoint.Col.bounds.center - transform.position;
                var directionToCheckpoint = toCheckpoint.normalized;

                // how much are we facing the checkpoint? (1.0 = directly facing, -1.0 = facing away)
                var facingAlignment = Vector3.Dot(transform.forward, directionToCheckpoint);
                sensor.AddObservation(facingAlignment);

                // which way should we turn? (positive = turn right, negative = turn left)
                var turnDirection = Vector3.SignedAngle(transform.forward, directionToCheckpoint, Vector3.up);
                sensor.AddObservation(turnDirection / 180f); // normalize to [-1, 1]

                // how far away is the checkpoint? (normalized)
                var distanceToCheckpoint = toCheckpoint.magnitude;
                sensor.AddObservation(Mathf.Clamp01(distanceToCheckpoint / 50f));
            }
            else
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }

            // velocity
            var localVelocity = transform.InverseTransformDirection(_kart.Rb.linearVelocity);

            sensor.AddObservation(localVelocity.z / 30f); // forward/backward speed
            sensor.AddObservation(localVelocity.x / 15f); // left/right sliding
            sensor.AddObservation(_kart.Rb.linearVelocity.magnitude / 30f); // total speed
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            // action 0 is throttle
            Throttle = actions.DiscreteActions[0] switch
            {
                0 => 0f, // none
                1 => 1f, // forward
                2 => -1f, // reverse
                _ => 0f
            };

            // action 1 is steering
            Steering = actions.DiscreteActions[1] switch
            {
                0 => 0f, // none
                1 => 1f, // right 
                2 => -1f, // left
                _ => 0f
            };

            // training mode rewards
            if (IsTrainingMode)
            {
                // time penalty to encourage speed
                AddReward(timePenaltyPerSecond);

                // reward for moving toward checkpoint
                if (_nextCheckpoint != null)
                {
                    var toCheckpoint = (_nextCheckpoint.Col.bounds.center - transform.position).normalized;
                    var velocityToward = Vector3.Dot(_kart.Rb.linearVelocity, toCheckpoint);

                    if (velocityToward > 0)
                        AddReward(velocityToward * velocityRewardScale);

                    // small reward for facing checkpoint
                    var facingDot = Vector3.Dot(transform.forward, toCheckpoint);
                    if (facingDot > 0)
                        AddReward(facingDot * facingCheckpointReward);
                }

                _episodeTime += Time.fixedDeltaTime;

                if (_episodeTime >= maxEpisodeTime)
                {
                    AddReward(-0.5f); // timeout
                    Debug.Log("episode end due to timeout");
                    EndEpisode();
                }
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var discreteActions = actionsOut.DiscreteActions;

            // throttle
            var forwardAction = 0;
            if (Input.GetKey(KeyCode.W)) forwardAction = 1;
            if (Input.GetKey(KeyCode.S)) forwardAction = 2;
            discreteActions[0] = forwardAction;

            // steering
            var turnAction = 0;
            if (Input.GetKey(KeyCode.D)) turnAction = 1;
            if (Input.GetKey(KeyCode.A)) turnAction = 2;
            discreteActions[1] = turnAction;
        }

        // called by race manager when race starts
        public void InitializeForGameplay()
        {
            if (_gameplayInitialized) return;

            _gameplayInitialized = true;
            UpdateNextCheckpoint();

            // still need initial setup
            if (checkpointSensor != null)
                checkpointSensor.Reset();
        }
    }
}