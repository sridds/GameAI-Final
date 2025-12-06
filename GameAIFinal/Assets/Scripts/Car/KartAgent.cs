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
        [SerializeField] private CheckpointTrack track;
        [FormerlySerializedAs("wallBumper")] [SerializeField] private WallSensor wallSensor;
        [SerializeField] private Transform spawnPosition;

        private KartController _kart;

        protected override void Awake()
        {
            base.Awake();
            _kart = GetComponent<KartController>();
            _kart.SetInputSource(this);
        }

        public float Throttle { get; private set; }
        public float Steering { get; private set; }
        public bool IsBraking => false;
        public bool IsDrifting => false;

        public override void OnEpisodeBegin()
        {
            Vector3 pos = spawnPosition.position + new Vector3(
                Random.Range(-5f, 5f),
                0f,
                Random.Range(-5f, 5f)
            );
            transform.position = pos;
            transform.forward = spawnPosition.forward;
            _kart.ResetState();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // TODO: Make agent aware of direction to next checkpoint (dot product)
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            float forwardAmount = actions.DiscreteActions[0] switch
            {
                0 => 0f,
                1 => 1f,
                2 => -1f,
                _ => 0f
            };

            float turnAmount = actions.DiscreteActions[2] switch
            {
                0 => 0f,
                1 => 1f,
                2 => -1f,
                _ => 0f
            };

            Throttle = forwardAmount;
            Steering = turnAmount;
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            int forwardAction = 0;
            if (Input.GetKey(KeyCode.W)) forwardAction = 1;
            if (Input.GetKey(KeyCode.S)) forwardAction = 2;

            int turnAction = 0;
            if (Input.GetKey(KeyCode.D)) turnAction = 1;
            if (Input.GetKey(KeyCode.A)) turnAction = 2;

            var discreteActions = actionsOut.DiscreteActions;
            discreteActions[0] = forwardAction;
            discreteActions[2] = turnAction;
        }
    }
}