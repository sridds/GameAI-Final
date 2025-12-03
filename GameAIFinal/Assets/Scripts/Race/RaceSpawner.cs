using System.Collections.Generic;
using Kart.Car;
using UnityEngine;

// Responsible for spawning racers on the track in a randomized formation
namespace Kart.Race
{
    public class RaceSpawner : MonoBehaviour
    {
        [SerializeField] private List<RacerSO> racerCatalogue = new List<RacerSO>();
        [SerializeField] private Transform[] orderedSpawnPoints;
        [SerializeField] private bool spawnRandomly;

        public List<CarDriver> Spawn()
        {
            List<CarDriver> result = new List<CarDriver>();
            Debug.Log($"[{name}]: Spawning Racers");

            if (spawnRandomly)
            {
                int n = racerCatalogue.Count;
                while (n > 1)
                {
                    n--;
                    int k = Random.Range(0, n + 1); // Get a random index from 0 to n (inclusive)
                    RacerSO value = racerCatalogue[k];
                    racerCatalogue[k] = racerCatalogue[n];
                    racerCatalogue[n] = value;
                }
            }

            // set positions
            for(int i = 0; i < racerCatalogue.Count; i++)
            {
                result.Add(SpawnRacer(racerCatalogue[i], orderedSpawnPoints[i].transform.position));
            }
            
            return result;
        }

        private CarDriver SpawnRacer(RacerSO racer, Vector3 position)
        {
            return Instantiate(racer.carPrefab, position, Quaternion.identity);
        }
    }
}
