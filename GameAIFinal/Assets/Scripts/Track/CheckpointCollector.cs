using System.Collections.Generic;
using UnityEngine;

namespace Kart.Track
{
    [RequireComponent(typeof(Collider))]
    public class CheckpointCollector : MonoBehaviour
    {
        public Checkpoint cpBehind;
    
        public delegate void CollectHandler(); 
        public event CollectHandler OnCollectForward;
        public event CollectHandler OnCollectBackward;

        // Tracks entry position of the checkpoints colliding with
        // both to validate direction and to ensure they pass the checkpoint at all
        private List<Checkpoint> passingFromBack; // going forward
        private List<Checkpoint> passingFromFront; // going backward

        private Collider col;

        [SerializeField] private Checkpoint debugCp;

        private void Awake()
        {
            col = GetComponent<Collider>();
        }

        bool IsBehindCheckpoint(Checkpoint cp)
        {
            // Based off of collider centers
            return cp.IsAheadOf(col.bounds.center);
        }

        void Update()
        {
            Debug.Log(IsBehindCheckpoint(debugCp));
        }
    
        private void OnCollisionEnter(Collision other)
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
    
        private void OnCollisionExit(Collision other)
        {
            if (!other.gameObject.TryGetComponent<Checkpoint>(out var cp))
                return;

            if (!passingFromBack.Contains(cp) && !passingFromFront.Contains(cp))
                Debug.LogError("Missing cp tracking, should never happen!");

            if (IsBehindCheckpoint(cp))
            {
                if (passingFromBack.Contains(cp))
                {
                    // On the same side we started
                
                }
                else if (passingFromFront.Contains(cp))
                {
                    // Passed forward    
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
                    // On the same side we started
                }
            }
           
        }
    }
}
