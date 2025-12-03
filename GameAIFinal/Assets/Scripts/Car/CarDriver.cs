using System;
using Kart.Race;
using Kart.Track;
using UnityEngine;

namespace Kart.Car
{
    public class CarDriver : MonoBehaviour
    {
        public RacerSO racerID;
        public int currentLap = 1;

        [Header("References")]
        [SerializeField] private Transform _carHolder;
        [SerializeField] private Transform _carOrientation;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Camera _camera;
        [SerializeField] private CheckpointCollector _checkpointCollector;

        [Header("Car Settings")]
        [SerializeField] private float _steering;
        [SerializeField] private float _acceleration;
        [SerializeField] private float _gravity;
        [SerializeField] private LayerMask _groundLayer;

        //private bool isDrifting = false;
        private float currentRotation;
        private float targetRotation;
        private float currentSpeed;
        private float targetSpeed;
        //private float forward;

        public Camera myCamera { get { return _camera; } }
        public CheckpointCollector CpCollector { get { return _checkpointCollector; } }

        private void Update()
        {
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            float turnAmount = 0.0f;

            _carHolder.position = _rigidbody.transform.position - new Vector3(0, 0.2f, 0);

            if(input.y > 0.0f)
            {
                targetSpeed = _acceleration;
            }

            if (input.x != 0.0f)
            {
                int dir = input.x > 0 ? 1 : -1;
                turnAmount = Mathf.Abs(input.x);
                ApplySteer(dir, turnAmount);
            }

            currentSpeed = Mathf.SmoothStep(currentSpeed, targetSpeed, Time.deltaTime * 12.0f);
            currentRotation = Mathf.Lerp(currentRotation, targetRotation, Time.deltaTime * 4.0f);

            targetSpeed = 0.0f;
            targetRotation = 0.0f;
        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(-_carOrientation.transform.forward * currentSpeed, ForceMode.Acceleration);
            _rigidbody.AddForce(Vector3.down * _gravity, ForceMode.Acceleration);
            _carHolder.eulerAngles = Vector3.Lerp(_carHolder.eulerAngles, new Vector3(0, _carHolder.eulerAngles.y + currentRotation, 0), Time.deltaTime * 5.0f);

            RaycastHit hitOn;
            RaycastHit hitNear;

            Physics.Raycast(_carHolder.position, Vector3.down, out hitOn, 1.1f, _groundLayer);
            Physics.Raycast(_carHolder.position, Vector3.down, out hitNear, 2.0f, _groundLayer);

            _carOrientation.parent.up = Vector3.Lerp(_carOrientation.parent.up, hitNear.normal, Time.deltaTime * 8.0f);
            _carOrientation.parent.Rotate(0, _carHolder.eulerAngles.y, 0);
        }

        private void ApplySteer(float direction, float amount)
        {
            targetRotation = (_steering * direction) * amount;
        }

        public void SetInputs(float forwardAmount, float turnAmount)
        {
            targetSpeed = forwardAmount * _acceleration;
            targetRotation = _steering * turnAmount;
        }

        public void StopCompletely()
        {
            throw new System.NotImplementedException();
        }
    }
}
