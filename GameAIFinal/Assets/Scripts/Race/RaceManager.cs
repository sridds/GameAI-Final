using System.Collections.Generic;
using Kart.Car;
using Kart.Track;
using UnityEngine;

namespace Kart.Race
{
    public class RaceManager : MonoBehaviour
    {
        public static RaceManager instance;
        public static RacerSO currentSpectatedRacer;

        [SerializeField] public bool isRacing = false;
        [SerializeField] private RaceSpawner spawner;
        [SerializeField] private float timeElapsed = 0;
        [SerializeField] private CheckpointTrack track;

        public Dictionary<CarDriver, int> RacerCheckpointsPassed { get; private set; } = new();
        public List<CarDriver> OrderedRacers { get; private set; } = new();

        private void Awake()
        {
            InitRace();

            currentSpectatedRacer = null;
            instance = this;
        }
        
        private void OnEnable()
        {
            foreach (var driver in OrderedRacers)
            {
                driver.CpCollector.onPassedCheckpointDirection.AddListener(OnPassed);
            }
        }

        void OnPassed(Checkpoint cp, bool forward)
        {
            
        }

        public void InitRace()
        {
            Debug.Log($"[{name}]: Initializing Race...");
            OrderedRacers = spawner.Spawn();
        }
    }
}
