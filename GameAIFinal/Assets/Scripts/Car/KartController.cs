using Kart.Race;
using UnityEngine;

namespace Kart.Car
{
    public class KartController : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] private RacerSO racerID;
        public RacerSO RacerID => racerID;
        public Rigidbody Rb => rb;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Camera cam;

        [SerializeField] private Transform carHolder;
        [SerializeField] private Transform carOrientation;

        [Header("Movement")] 
        [SerializeField] private float steering = 100f;

        [SerializeField] private float acceleration = 15f;
        [SerializeField] private float brakeForce = 15f;
        [SerializeField] private float gravity = 20f;

        [Header("Physics")] 
        [SerializeField] private float downforceMultiplier = 2f;
        [SerializeField] private float counterSteerStrength = 0.3f;
        [SerializeField] private float angularDrag = 0.8f;
        [SerializeField] private LayerMask groundLayer;
        
        private float _currentRotation;
        private float _currentSpeed;

        private IKartInput _input;
        private float _targetRotation;
        private float _targetSpeed;
        public Camera Cam => cam;

        public bool IsGrounded { get; private set; }

        private void Start()
        {
            rb.angularDamping = angularDrag;
            if (_input == null)
                _input = GetComponent<IKartInput>();
        }

        private void Update()
        {
            if (_input != null)
            {
                // Throttle
                if (_input.Throttle > 0f)
                    _targetSpeed = acceleration;
                else if (_input.Throttle < 0f)
                    _targetSpeed = -brakeForce;
                else
                    _targetSpeed = 0f;

                // Steering
                if (Mathf.Abs(_input.Steering) > 0.01f)
                {
                    var dir = _input.Steering > 0 ? 1 : -1;
                    var amount = Mathf.Abs(_input.Steering);
                    _targetRotation = steering * dir * amount;
                }
                else
                {
                    _targetRotation = 0f;
                }
            }

            _currentSpeed = Mathf.SmoothStep(_currentSpeed, _targetSpeed, Time.deltaTime * 12f);
            _currentRotation = Mathf.Lerp(_currentRotation, _targetRotation, Time.deltaTime * 4f);

            carHolder.position = rb.position - new Vector3(0, 0.2f, 0);
        }

        private void FixedUpdate()
        {
            // Throttle
            rb.AddForce(-carOrientation.forward * _currentSpeed, ForceMode.Acceleration);

            // Gravity
            rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

            // Downforce with sped
            var speedSqr = rb.linearVelocity.sqrMagnitude;
            rb.AddForce(Vector3.down * speedSqr * downforceMultiplier, ForceMode.Force);

            // Countersteer gives less sliding
            var localVel = carOrientation.InverseTransformDirection(rb.linearVelocity);
            var counterForce = -localVel.x * counterSteerStrength;
            rb.AddForce(carOrientation.right * counterForce, ForceMode.VelocityChange);

            // Rotation
            carHolder.eulerAngles = Vector3.Lerp(
                carHolder.eulerAngles,
                new Vector3(0, carHolder.eulerAngles.y + _currentRotation, 0),
                Time.deltaTime * 5f
            );

            // Ground alignment
            if (Physics.Raycast(carHolder.position, Vector3.down, out var hitNear, 2f, groundLayer))
            {
                carOrientation.parent.up = Vector3.Lerp(carOrientation.parent.up, hitNear.normal, Time.deltaTime * 8f);
                carOrientation.parent.Rotate(0, carHolder.eulerAngles.y, 0);
            }

            // Grounded check
            IsGrounded = Physics.Raycast(carHolder.position, Vector3.down, 1.1f, groundLayer);
        }

        public void SetInputSource(IKartInput input)
        {
            _input = input;
        }

        public void SetInputsDirect(float throttle, float steer)
        {
            if (throttle > 0f) _targetSpeed = acceleration;
            else if (throttle < 0f) _targetSpeed = -brakeForce;
            else _targetSpeed = 0f;

            if (Mathf.Abs(steer) > 0.01f)
            {
                var dir = steer > 0 ? 1 : -1;
                _targetRotation = steering * dir * Mathf.Abs(steer);
            }
            else
            {
                _targetRotation = 0f;
            }
        }

        public void ResetState()
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            _currentSpeed = 0f;
            _currentRotation = 0f;
            _targetSpeed = 0f;
            _targetRotation = 0f;
        }

        public void Initialize(RacerSO racerSO, Vector3 position, Quaternion rotation)
        {
            this.racerID = racerSO;
            rb.position = position;
            rb.rotation = rotation;
            carHolder.position = position - new Vector3(0, 0.2f, 0);
            carHolder.rotation = rotation;
            carOrientation.parent.rotation = rotation;
            ResetState();
        }
    }
}