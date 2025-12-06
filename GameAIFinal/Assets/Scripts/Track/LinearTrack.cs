using System.Collections.Generic;
using UnityEngine;

// Linear checkpoint track! (no branches)
// All checkpoints must be ahead of their previous checkpoint
namespace Kart.Track
{
    public class LinearTrack : CheckpointTrack, ICheckpointTrack
    {
        public int TotalCheckpoints => checkpoints.Count;

        public override Checkpoint GetFirstCheckpoint()
        {
            return checkpoints[0];
        }

        public override Checkpoint GetNextCheckpoint(CheckpointSensor sensor)
        {
            var idx = checkpoints.IndexOf(sensor.LastCheckpointPassedForward);
            return checkpoints[(idx + 1) % checkpoints.Count];
        }

        public Checkpoint GetCheckpointAt(int index)
        {
            return checkpoints[(index % checkpoints.Count + checkpoints.Count) % checkpoints.Count];
        }

        public float GetLapProgress(CheckpointSensor sensor)
        {
            return checkpoints.Count == 0
                ? 0f
                : (float)(sensor.CheckpointsPassed % checkpoints.Count) / checkpoints.Count;
        }

        public void PopulateCheckpoints()
        {
            checkpoints.Clear();
            foreach (var cp in GetComponentsInChildren<Checkpoint>(false))
            {
                cp.gameObject.name = "Checkpoint #" + checkpoints.Count;
                checkpoints.Add(cp);
            }

            if (!IsValidLoop())
                Debug.LogError("Each checkpoint must be placed in front of the previous, in a loop. " +
                               "Check recent warnings for more details.");
        }

        // All checkpoints must be ahead of their previous checkpoint
        private bool IsValidLoop()
        {
            if (checkpoints.Count < 3)
            {
                Debug.LogWarning("Not enough checkpoints for a track loop.");
                return false;
            }

            var isValid = true;
            for (var i = 1; i <= checkpoints.Count; i++)
            {
                var cp = checkpoints[i % checkpoints.Count];
                var cpLast = checkpoints[i - 1];

                if (!cp.IsAheadOf(cpLast.Col.bounds.center))
                {
                    Debug.LogWarning($"Checkpoint {cp.gameObject.name} " +
                                     $"is not ahead of checkpoint {cpLast.gameObject.name}!");

                    isValid = false;
                }
            }

            return isValid;
        }
    }
}