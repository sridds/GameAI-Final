using JetBrains.Annotations;
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
}
