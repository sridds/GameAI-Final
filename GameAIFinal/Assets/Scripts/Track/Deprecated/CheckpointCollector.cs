using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Kart.Track.Deprecated
{
    [Obsolete("Use Kart.Track.CheckpointSensor class instead")]
    [RequireComponent(typeof(Collider))]
    public class CheckpointCollector : MonoBehaviour
    {
        [SerializeField] private Checkpoint firstCheckpointBehind;
        
        public Checkpoint cpBehind;

        public int NumCollected {get; private set;}

        public UnityEvent<Checkpoint, bool> onPassedCheckpointDirection; // TODO: subscribe

        // Tracks entry position of the checkpoints colliding with
        // both to validate direction and to ensure they pass the checkpoint at all
        private List<Checkpoint> passingFromBack; // going forward
        private List<Checkpoint> passingFromFront; // going backward

        private Collider col;

        public void Refresh()
        {
            cpBehind = firstCheckpointBehind;
            passingFromBack = new List<Checkpoint>();
            passingFromFront = new List<Checkpoint>();
            NumCollected = 0;
        }

        private void Awake()
        {
            col = GetComponent<Collider>();
            Refresh();
            
            Debug.LogWarning("CheckpointCollector deprecated, Use CheckpointSensor instead");
        }

        bool IsBehindCheckpoint(Checkpoint cp)
        {
            // Based off of collider centers
            return cp.IsAheadOf(col.bounds.center);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.gameObject.TryGetComponent<Checkpoint>(out var cp))
                return;

            if (passingFromBack.Contains(cp) || passingFromFront.Contains(cp))
            {
                Debug.LogError("Doubled cp collision, should never happen!");
                return;
            }

            if (IsBehindCheckpoint(cp))
            {
                // Going forward
                passingFromBack.Add(cp);   
                Debug.Log("Entered going forward");
            }
            else
            {
                // Going backwards
                passingFromFront.Add(cp);
                Debug.Log("Entered going backward");
            }
        }
    
        private void OnTriggerExit(Collider other)
        {
            if (!other.gameObject.TryGetComponent<Checkpoint>(out var cp))
                return;

            if (!passingFromBack.Contains(cp) && !passingFromFront.Contains(cp))
                Debug.LogError("Missing cp tracking, should never happen!");

            if (IsBehindCheckpoint(cp))
            {
                if (passingFromBack.Contains(cp))
                {
                    // Didn't pass forward, still behind
                }
                else if (passingFromFront.Contains(cp))
                {
                    // Passed forward    
                    // TODO: apply reward
                }
            }
            else //  isAhead
            {
                if (passingFromBack.Contains(cp))
                {
                    // Passed backward
                }
                else if (passingFromFront.Contains(cp))
                {
                    // Didn't pass backward, still in front
                    // TODO: apply penalty
                }
            }
           
        }
    }
}
