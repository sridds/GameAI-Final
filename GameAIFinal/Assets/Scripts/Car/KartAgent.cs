using Kart.Track;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Kart.Car
{
    [RequireComponent(typeof(KartController))]
    public class KartAgent : Agent, IKartInput
    {
        [FormerlySerializedAs("wallBumper")] [SerializeField]
        private WallSensor wallSensor;

        [SerializeField] private CheckpointSensor checkpointSensor;
        // private Transform spawnPosition;

        private Vector3 spawnPos;
        Quaternion spawnRot;

        [Header("Sensors")] [SerializeField] private CheckpointTrack track;

        private KartController _kart;

        private Checkpoint _nextCheckpoint;

        private Rigidbody _rb;

        protected override void Awake()
        {
            base.Awake();
            _kart = GetComponent<KartController>();
            _kart.SetInputSource(this);
            _rb = GetComponent<Rigidbody>();
            spawnPos = transform.position;
            spawnRot = transform.rotation;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            checkpointSensor.OnCheckpointPassed += OnCheckpointPassed;
            wallSensor.OnApplyPenalty += AddReward;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            checkpointSensor.OnCheckpointPassed -= OnCheckpointPassed;
            wallSensor.OnApplyPenalty -= AddReward;
        }

        public float Throttle { get; private set; }
        public float Steering { get; private set; }
        public bool IsBraking => false;
        public bool IsDrifting => false;

        private void UpdateNextCheckpoint()
        {
            if (track != null && checkpointSensor != null) _nextCheckpoint = track.GetNextCheckpoint(checkpointSensor);
        }

        private void OnCheckpointPassed(CheckpointPassedEvent checkpointPassedEvent)
        {
            AddReward(checkpointPassedEvent.RewardMultiplier);
        }

        public override void OnEpisodeBegin()
        {
            // Reset position
            transform.position = spawnPos;

            // Reset rigidbody
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.position = spawnPos;
            _rb.rotation = spawnRot;

            transform.position = spawnPos;
            transform.rotation = spawnRot;

            // Reset kart state
            _kart.ResetState();

            // Reset checkpoint sensor
            if (checkpointSensor != null)
                checkpointSensor.Reset();

            UpdateNextCheckpoint();
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
                Debug.Log($"ForwardDot: {forwardDot}");

                var signedAngle = Vector3.SignedAngle(transform.forward, dirToCheckpoint, Vector3.up);
                sensor.AddObservation(signedAngle / 180f); // turn direction
                Debug.Log($"SignedAngle: {signedAngle}");

                var distance = toCheckpoint.magnitude;
                sensor.AddObservation(Mathf.Clamp01(distance / 50f)); // distance
                Debug.Log($"Distance: {distance}");
            }
            else
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }

            var localVelocity = transform.InverseTransformDirection(_rb.linearVelocity);
            sensor.AddObservation(localVelocity.z / 30f); // forward speed
            Debug.Log($"LocalVelocity: {localVelocity}");
            sensor.AddObservation(localVelocity.x / 15f); // lateral slide
            Debug.Log($"Velocity: {_rb.linearVelocity}");
            sensor.AddObservation(_rb.linearVelocity.magnitude / 30f); // total speed
            Debug.Log($"Velocity: {_rb.linearVelocity.magnitude}");
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