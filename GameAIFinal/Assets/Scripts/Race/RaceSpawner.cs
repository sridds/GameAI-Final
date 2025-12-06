using System.Collections.Generic;
using Kart.Car;
using UnityEngine;

// Responsible for spawning racers on the track in a randomized formation
namespace Kart.Race
{
    public class RaceSpawner : MonoBehaviour
    {
        [SerializeField] private List<RacerSO> racerCatalogue = new();
        [SerializeField] private Transform[] orderedSpawnPoints;
        [SerializeField] private bool spawnRandomly;

        public List<KartController> Spawn()
        {
            var result = new List<KartController>();

            if (spawnRandomly)
                for (var n = racerCatalogue.Count - 1; n > 0; n--)
                {
                    var k = Random.Range(0, n + 1);
                    (racerCatalogue[k], racerCatalogue[n]) = (racerCatalogue[n], racerCatalogue[k]);
                }

            for (var i = 0; i < racerCatalogue.Count && i < orderedSpawnPoints.Length; i++)
            {
                var prefab = racerCatalogue[i].kartPrefab;
                if (prefab == null) continue;

                var instance = Instantiate(prefab, orderedSpawnPoints[i].position, orderedSpawnPoints[i].rotation);
                instance.name = racerCatalogue[i].racerName;
                result.Add(instance);
            }

            return result;
        }
    }
}