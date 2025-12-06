using System.Collections.Generic;
using UnityEngine;

namespace Kart.Track
{
    public abstract class CheckpointTrack : MonoBehaviour
    {
        [SerializeField] protected List<Checkpoint> checkpoints = new List<Checkpoint>();
        public abstract Checkpoint GetFirstCheckpoint();
        public bool HasCheckpoint(Checkpoint checkpoint) => checkpoints.Contains(checkpoint);
        public int CheckpointCount => checkpoints.Count;
    
        public abstract Checkpoint GetNextCheckpoint(CheckpointSensor collector);
    }
}