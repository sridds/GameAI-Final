using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kart.Track
{
    [RequireComponent(typeof(Collider))]
    public class CheckpointSensor : MonoBehaviour
    {
        [SerializeField] private Checkpoint initialCheckpointBehind;
        private readonly Dictionary<Checkpoint, bool> _entryFromBehind = new();

        private Collider _collider;
        private CheckpointTrack _track;
        
        public event Action<CheckpointPassedEvent> OnCheckpointPassed;
        public Checkpoint LastCheckpointPassedForward { get; private set; }
        public int CheckpointsPassed { get; private set; }

        private void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        public void Reset()
        {
            LastCheckpointPassedForward = initialCheckpointBehind;
            CheckpointsPassed = 0;
            _entryFromBehind.Clear();
        }

        private void Start()
        {
            Reset();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent<Checkpoint>(out var cp)) return;
            _entryFromBehind[cp] = cp.IsAheadOf(_collider.bounds.center);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent<Checkpoint>(out var cp)) return;
            if (!_entryFromBehind.TryGetValue(cp, out var enteredFromBehind)) return;

            var exitedAhead = !cp.IsAheadOf(_collider.bounds.center);
            var passedForward = enteredFromBehind && exitedAhead;
            var passedBackward = !enteredFromBehind && !exitedAhead;

            if (passedForward)
            {
                CheckpointsPassed++;
                LastCheckpointPassedForward = cp;
                var pEvent = new CheckpointPassedEvent
                {
                    Checkpoint = cp,
                    IsForward = true, TotalPassed = CheckpointsPassed,
                    RewardMultiplier = cp.GetRewardMultiplier()
                };
                OnCheckpointPassed?.Invoke(pEvent);
                Bus<CheckpointPassedEvent>.Raise(pEvent);
            }
            else if (passedBackward)
            {
                CheckpointsPassed = Mathf.Max(0, CheckpointsPassed - 1);
                var pEvent = new CheckpointPassedEvent
                {
                    Checkpoint = cp,
                    IsForward = false,
                    TotalPassed = CheckpointsPassed,
                    RewardMultiplier = -cp.GetRewardMultiplier()
                };
                OnCheckpointPassed?.Invoke(pEvent);
                Bus<CheckpointPassedEvent>.Raise(pEvent);
                
                OnWrongWay?.Invoke();
            }

            _entryFromBehind.Remove(cp);
        }

        public void SetTrack(CheckpointTrack track)
        {
            _track = track;
        }

        public event Action OnWrongWay;
    }
}