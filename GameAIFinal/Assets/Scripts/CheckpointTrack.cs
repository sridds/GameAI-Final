using System.Collections.Generic;
using UnityEngine;

public abstract class CheckpointTrack : MonoBehaviour
{
    public abstract Checkpoint GetNextCheckpoint(CheckpointCollector collector);
}