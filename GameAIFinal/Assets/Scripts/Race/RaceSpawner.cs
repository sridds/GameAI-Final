using System.Collections.Generic;
using Kart.Car;
using UnityEngine;

// spawns racers on track in various configurations
namespace Kart.Race
{
    public enum ESpawnMode
    {
        Training,
        Gameplay
    }

    public class RaceSpawner : MonoBehaviour
    {
        [Header("Mode")]
        [SerializeField] private ESpawnMode spawnMode = ESpawnMode.Gameplay;

        [Header("Racer Setup")]
        [SerializeField] private List<RacerSO> racerCatalogue = new();
        [SerializeField] private RacerSO playerRacer;
        [SerializeField] private bool spawnRandomly;

        [Header("Spawn Points")]
        [SerializeField] private Transform[] orderedSpawnPoints;

        [Header("Training Mode Settings")]
        [SerializeField] private Transform trainingSpawnPoint;
        [SerializeField] private int trainingRacerDuplicates = 3;
        [SerializeField] private float trainingSpawnRadius = 1f;

        public List<KartController> Spawn()
        {
            if (spawnMode == ESpawnMode.Training)
                return SpawnTrainingMode();
            else
                return SpawnGameplayMode();
        }

        private List<KartController> SpawnTrainingMode()
        {
            // shuffle if needed
            if (spawnRandomly)
                ShuffleRacers();

            var result = new List<KartController>();

            // spawn duplicates in circle for training
            for (var r = 0; r < trainingRacerDuplicates; r++)
            {
                foreach (var t in racerCatalogue)
                {
                    var prefab = t.kartPrefab;
                    if (prefab == null) continue;

                    // randomize position in circle
                    var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    var offset = new Vector3(
                        Mathf.Cos(angle),
                        0f,
                        Mathf.Sin(angle)
                    );
                    var spawnPos = trainingSpawnPoint.position + offset * Random.Range(0f, trainingSpawnRadius);
                    var instance = Instantiate(prefab, spawnPos, trainingSpawnPoint.rotation);

                    var agent = instance.GetComponent<KartAgent>();
                    if (agent != null)
                        agent.IsTrainingMode = true;

                    instance.name = t.racerName + $" {r}";
                    result.Add(instance);
                }
            }

            return result;
        }

        private List<KartController> SpawnGameplayMode()
        {
            var result = new List<KartController>();

            // need at least 12 spawn points for full grid
            if (orderedSpawnPoints.Length < 12)
            {
                Debug.LogWarning($"only {orderedSpawnPoints.Length} spawn points available, need 12 for full grid");
            }

            // spawn player at first position
            if (playerRacer != null && playerRacer.kartPrefab != null)
            {
                var playerInstance = Instantiate(
                    playerRacer.kartPrefab,
                    orderedSpawnPoints[0].position,
                    orderedSpawnPoints[0].rotation
                );

                playerInstance.name = playerRacer.racerName + " (Player)";

                // remove ml agent if exists, add player input
                var agent = playerInstance.GetComponent<KartAgent>();
                if (agent != null)
                    Destroy(agent);

                var playerInput = playerInstance.GetComponent<PlayerKartInput>();
                if (playerInput == null)
                    playerInput = playerInstance.gameObject.AddComponent<PlayerKartInput>();

                playerInstance.SetInputSource(playerInput);

                result.Add(playerInstance);
            }
            else
            {
                Debug.LogError("player racer not set or missing prefab!");
            }

            // prepare ai racers
            var aiRacers = new List<RacerSO>(racerCatalogue);

            // shuffle ai if needed
            if (spawnRandomly)
            {
                for (var n = aiRacers.Count - 1; n > 0; n--)
                {
                    var k = Random.Range(0, n + 1);
                    (aiRacers[k], aiRacers[n]) = (aiRacers[n], aiRacers[k]);
                }
            }

            // spawn 11 ai opponents starting at position 2 (index 1)
            var spawnedAi = 0;
            var catalogueIndex = 0;

            while (spawnedAi < 11 && (spawnedAi + 1) < orderedSpawnPoints.Length)
            {
                // loop through catalogue if we need more racers than available
                var racerSO = aiRacers[catalogueIndex % aiRacers.Count];

                if (racerSO.kartPrefab == null)
                {
                    catalogueIndex++;
                    continue;
                }

                var spawnIndex = spawnedAi + 1;
                var aiInstance = Instantiate(
                    racerSO.kartPrefab,
                    orderedSpawnPoints[spawnIndex].position,
                    orderedSpawnPoints[spawnIndex].rotation
                );

                // configure as ai opponent
                var agent = aiInstance.GetComponent<KartAgent>();
                if (agent != null)
                {
                    agent.IsTrainingMode = false;
                }

                // name it clearly
                var duplicateNumber = catalogueIndex / aiRacers.Count;
                if (duplicateNumber > 0)
                    aiInstance.name = racerSO.racerName + $" {duplicateNumber + 1}";
                else
                    aiInstance.name = racerSO.racerName;

                result.Add(aiInstance);
                spawnedAi++;
                catalogueIndex++;
            }

            Debug.Log($"spawned {result.Count} racers: 1 player + {spawnedAi} ai");
            return result;
        }

        private void ShuffleRacers()
        {
            for (var n = racerCatalogue.Count - 1; n > 0; n--)
            {
                var k = Random.Range(0, n + 1);
                (racerCatalogue[k], racerCatalogue[n]) = (racerCatalogue[n], racerCatalogue[k]);
            }
        }
    }
}