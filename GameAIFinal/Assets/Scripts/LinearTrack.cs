using System.Collections.Generic;
using UnityEngine;

// Linear checkpoint track! (no branches)
// All checkpoints must be ahead of their previous checkpoint
public class LinearTrack : CheckpointTrack
{
    [SerializeField] private List<Checkpoint> checkpoints = new();

    public override Checkpoint GetNextCheckpoint(CheckpointCollector collector)
    {
        return checkpoints[checkpoints.IndexOf(collector.cpBehind) % checkpoints.Count];
    }
    
    public void PopulateCheckpoints()
    {
        checkpoints.Clear();
        foreach (Checkpoint cp in GetComponentsInChildren<Checkpoint>(false))
        {
            cp.gameObject.name = "Checkpoint #" + checkpoints.Count.ToString();
            checkpoints.Add(cp);
        }

        if (!IsValidLoop())
        {
            Debug.LogError("Each checkpoint must be placed in front of the previous, in a loop. " +
                           "Check recent warnings for more details.");
        }
    }

    // All checkpoints must be ahead of their previous checkpoint
    private bool IsValidLoop()
    {
        if (checkpoints.Count < 3)
        {
            Debug.LogWarning("Not enough checkpoints for a track loop.");
            return false;
        }
            
        bool isValid = true;
        for (int i = 1; i <= checkpoints.Count; i++)
        {
            Checkpoint cp = checkpoints[i % checkpoints.Count];
            Checkpoint cpLast = checkpoints[i - 1];
            
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
