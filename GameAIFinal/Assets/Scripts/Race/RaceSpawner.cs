using System;
using System.Collections.Generic;
using Kart.Car;
using UnityEngine;

// Responsible for spawning racers on the track in a randomized formation
namespace Kart.Race
{
    public class RaceSpawner : MonoBehaviour
    {
        [SerializeField] private RacerSO[] racerCatalogue;
        [SerializeField] private Transform[] orderedSpawnPoints;

        public List<CarDriver> Spawn()
        {
            List<CarDriver> result = new List<CarDriver>();
            Debug.Log($"[{name}]: Spawning Racers");
            
            foreach (RacerSO racer in racerCatalogue)
                result.Add(SpawnRacer(racer));
            
            return result;
        }

        private CarDriver SpawnRacer(RacerSO racer)
        {
            return Instantiate(racer.carPrefab);
        }
    }
}
