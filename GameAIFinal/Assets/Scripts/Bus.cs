using JetBrains.Annotations;
using Kart.Car;
using Kart.Race;
using Kart.Track;
using UnityEngine;

namespace Kart
{
    public interface IGameEvent
    {
    }

    public static class Bus<T> where T : IGameEvent
    {
        public delegate void Event(T evt);

        public static event Event OnEvent;

        public static void Raise([CanBeNull] T evt)
        {
            OnEvent?.Invoke(evt);
        }
    }

    public class RaceStateUpdated : IGameEvent
    {
        public ERaceState currentState;
        public ERaceState previousState;
    }

    public class RacerPlacementUpdated : IGameEvent
    {
        public int placement;
        public RacerSO racerReference;
    }

    public class TimerAnnouncement : IGameEvent
    {
        public enum ETimerAnnouncement
        {
            Three,
            Two,
            One,
            Go
        }

        public ETimerAnnouncement timerAnnouncementType;

        public string[] timerMapping = { "3", "2", "1", "GO!" };
    }

    public class CheckpointPassedEvent : IGameEvent
    {
        public Checkpoint Checkpoint;
        public bool IsForward;
        public float RewardMultiplier;
        public int TotalPassed;
    }

    public class SpectatorChangeCamera : IGameEvent
    {
        public Camera camera;
        public CarDriver carReference; // may be null
    }
}