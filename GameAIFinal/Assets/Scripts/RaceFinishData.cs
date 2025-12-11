using Kart.Car;
using UnityEngine;

namespace Kart.Race
{
    public class RaceFinishData
    {
        public KartController Kart { get; set; }
        public int Position { get; set; }
        public float FinishTime { get; set; }
        public bool HasFinished { get; set; }

        public RaceFinishData(KartController kart)
        {
            Kart = kart;
            Position = -1;
            FinishTime = 0f;
            HasFinished = false;
        }

        public string GetFormattedTime()
        {
            int minutes = Mathf.FloorToInt(FinishTime / 60f);
            int seconds = Mathf.FloorToInt(FinishTime % 60f);
            int milliseconds = Mathf.FloorToInt((FinishTime * 1000f) % 1000f);
            return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
        }
    }
}