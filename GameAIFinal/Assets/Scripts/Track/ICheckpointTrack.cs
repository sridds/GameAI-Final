namespace Kart.Track
{
    public interface ICheckpointTrack
    {
        int TotalCheckpoints { get; }
        Checkpoint GetNextCheckpoint(CheckpointSensor sensor);
        Checkpoint GetCheckpointAt(int index);
        float GetLapProgress(CheckpointSensor sensor);
    }
}