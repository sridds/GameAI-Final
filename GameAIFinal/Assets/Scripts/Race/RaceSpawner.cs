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

        [SerializeField] private bool trainingMode;
        [SerializeField] private Transform trainingSpawnPoint;
        [SerializeField] private int trainingRacerDuplicates = 3;
        [SerializeField] private float trainingSpawnRadius = 1f;

        public List<KartController> Spawn()
        {
            if (spawnRandomly)
                for (var n = racerCatalogue.Count - 1; n > 0; n--)
                {
                    var k = Random.Range(0, n + 1);
                    (racerCatalogue[k], racerCatalogue[n]) = (racerCatalogue[n], racerCatalogue[k]);
                }

            var result = new List<KartController>();
            if (trainingMode)
            {
                for (var r = 0; r < trainingRacerDuplicates; r++)
                    foreach (var t in racerCatalogue)
                    {
                        var prefab = t.kartPrefab;
                        if (prefab == null) continue;

                        // Randomize position in a circle
                        var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                        var offset = new Vector3(
                            Mathf.Cos(angle),
                            0f,
                            Mathf.Sin(angle)
                        );
                        var spawnPos = trainingSpawnPoint.position + offset * Random.Range(0f, trainingSpawnRadius);

                        var instance = Instantiate(prefab, spawnPos, trainingSpawnPoint.rotation);
                        instance.name = t.racerName + $" {r}";
                        result.Add(instance);
                    }

                return result;
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