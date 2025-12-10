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

        [Header("Spawn Settings")] [SerializeField]
        private Transform spawnPoint;
        [SerializeField] private bool randomizeSpawnPosition = false;
        [SerializeField] private float spawnRandomRadius = 2f;

        private KartController _kart;

        private Rigidbody _rb;

        protected override void Awake()
        {
            base.Awake();
            _kart = GetComponent<KartController>();
            _kart.SetInputSource(this);
            _rb = GetComponent<Rigidbody>();
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

        private void OnCheckpointPassed(CheckpointPassedEvent checkpointPassedEvent)
        {
            AddReward(checkpointPassedEvent.RewardMultiplier);
        }

        public override void OnEpisodeBegin()
        {
            // Reset position
            var spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
            var spawnRot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

            if (randomizeSpawnPosition && spawnPoint != null)
            {
                var randomOffset = Random.insideUnitCircle * spawnRandomRadius;
                spawnPos += new Vector3(randomOffset.x, 0f, randomOffset.y);
            }

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
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // TODO: Make agent aware of direction to next checkpoint (dot product)
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