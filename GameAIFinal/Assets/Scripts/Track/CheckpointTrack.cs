using UnityEngine;

namespace Kart.Track
{
    public abstract class CheckpointTrack : MonoBehaviour
    {
        public abstract Checkpoint GetNextCheckpoint(CheckpointCollector collector);
    }
}