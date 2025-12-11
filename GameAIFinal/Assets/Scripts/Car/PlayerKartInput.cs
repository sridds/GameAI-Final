using Kart.Race;
using Kart.Track;
using UnityEngine;

namespace Kart.Car
{
    public class PlayerKartInput : MonoBehaviour, IKartInput
    {
        [SerializeField] private KeyCode brakeKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode driftKey = KeyCode.Space;

        private void Start()
        {
            var sensor = GetComponent<CheckpointSensor>();

            if (sensor != null)
            {
                sensor.Reset();
                Debug.Log($"[PlayerKartInput] Checkpoint sensor initialized on {gameObject.name}");
            }
            else
            {
                Debug.LogError($"[PlayerKartInput] NO CheckpointSensor found on {gameObject.name}!");
            }
        }

        private void Update()
        {
            // dont accept input until race has started
            if (RaceManager.Instance == null || !RaceManager.Instance.HasRaceStarted())
            {
                Throttle = 0f;
                Steering = 0f;
                IsDrifting = false;
                IsBraking = false;
                return;
            }

            Throttle = Input.GetAxis("Vertical");
            Steering = Input.GetAxis("Horizontal");
            IsDrifting = Input.GetKey(driftKey);
            IsBraking = Input.GetKey(brakeKey);
        }

        public float Throttle { get; private set; }
        public float Steering { get; private set; }
        public bool IsDrifting { get; private set; }
        public bool IsBraking { get; private set; }
    }
}