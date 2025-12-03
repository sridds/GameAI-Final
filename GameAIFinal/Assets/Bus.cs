using JetBrains.Annotations;
using Kart.Car;
using Kart.Race;
using UnityEngine;

namespace Kart
{
    public interface IGameEvent { }

    public static class Bus<T> where T : IGameEvent
    {
        public delegate void Event(T evt);

        public static event Event OnEvent;

        public static void Raise([CanBeNull] T evt) => OnEvent?.Invoke(evt);
    }

    public class RacerPlacementUpdated : IGameEvent
    {
        public RacerSO racerReference;
        public int placement;
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

        public string[] timerMapping = new string[] { "3", "2", "1", "GO!" };

        public ETimerAnnouncement timerAnnouncementType;
    }

    public class SpectatorChangeCamera : IGameEvent
    {
        public Camera camera;
        public CarDriver carReference; // may be null
    }
}
