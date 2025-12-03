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
        [SerializeField] private bool spawnRandomly;

        public List<CarDriver> Spawn()
        {
            List<CarDriver> result = new List<CarDriver>();
            Debug.Log($"[{name}]: Spawning Racers");
            
            foreach (RacerSO racer in racerCatalogue)
                result.Add(SpawnRacer(racer));

            if (spawnRandomly)
            {
                int n = result.Count;
                while (n > 1)
                {
                    n--;
                    int k = Random.Range(0, n + 1); // Get a random index from 0 to n (inclusive)
                    CarDriver value = result[k];
                    result[k] = result[n];
                    result[n] = value;
                }
            }

            // set positions
            for(int i = 0; i < result.Count; i++)
            {
                result[i].transform.position = orderedSpawnPoints[i].transform.position;
                Debug.Log($"{orderedSpawnPoints[i].transform.position}, {result[i].transform.position}");
            }
            
            return result;
        }

        private CarDriver SpawnRacer(RacerSO racer)
        {
            return Instantiate(racer.carPrefab);
        }
    }
}
