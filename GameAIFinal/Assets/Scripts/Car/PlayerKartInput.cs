using UnityEngine;

namespace Kart.Car
{
    public class PlayerInputDriver : MonoBehaviour, IKartInput
    {
        [SerializeField] private KeyCode brakeKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode driftKey = KeyCode.Space;

        private void Update()
        {
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