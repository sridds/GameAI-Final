using System;
using Kart.Track;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Kart.Car
{
    [RequireComponent(typeof(KartController))]
    public class KartAgent : Agent, IKartInput
    {
        [FormerlySerializedAs("wallBumper")] [SerializeField] private WallSensor wallSensor;
        [SerializeField] private CheckpointSensor checkpointSensor;
        // private Transform spawnPosition;

        private KartController _kart;

        protected override void Awake()
        {
            base.Awake();
            _kart = GetComponent<KartController>();
            _kart.SetInputSource(this);
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

        private void OnCheckpointPassed(CheckpointPassedEvent checkpointPassedEvent)
        {
            AddReward(checkpointPassedEvent.RewardMultiplier);
        }

        public float Throttle { get; private set; }
        public float Steering { get; private set; }
        public bool IsBraking => false;
        public bool IsDrifting => false;

        public override void OnEpisodeBegin()
        {
            // Vector3 pos = spawnPosition.position + new Vector3(
            //     Random.Range(-5f, 5f),
            //     0f,
            //     Random.Range(-5f, 5f)
            // );
            // transform.position = pos;
            // transform.forward = spawnPosition.forward;
            _kart.ResetState();
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
            int forwardAction = 0;
            if (Input.GetKey(KeyCode.W)) forwardAction = 1;
            if (Input.GetKey(KeyCode.S)) forwardAction = 2;
            discreteActions[0] = forwardAction;

            // Steering
            int turnAction = 0;
            if (Input.GetKey(KeyCode.D)) turnAction = 1;
            if (Input.GetKey(KeyCode.A)) turnAction = 2;
            discreteActions[1] = turnAction;
        }
    }
}