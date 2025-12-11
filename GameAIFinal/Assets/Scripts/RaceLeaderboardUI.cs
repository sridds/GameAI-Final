using System.Collections.Generic;
using System.Linq;
using Kart.Car;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kart.Race
{
    public class RaceLeaderboardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject leaderboardPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform leaderboardEntriesParent;
        [SerializeField] private GameObject leaderboardEntryPrefab;
        [SerializeField] private TextMeshProUGUI restartPromptText;

        [Header("Settings")]
        [SerializeField] private string restartSceneName;

        private bool isShowing = false;
        private List<RaceFinishData> finishData = new List<RaceFinishData>();
        private int lastFinishedCount = 0;
        private bool playerHasFinished = false;

        private void Awake()
        {
            if (leaderboardPanel != null)
                leaderboardPanel.SetActive(false);
        }

        private void OnEnable()
        {
            Bus<RaceStateUpdated>.OnEvent += OnRaceStateChanged;
        }

        private void OnDisable()
        {
            Bus<RaceStateUpdated>.OnEvent -= OnRaceStateChanged;
        }

        private void Update()
        {
            if (RaceManager.Instance != null && RaceManager.Instance.HasRaceStarted())
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RestartRace();
                    return;
                }
            }

            // check if player just finished
            if (!playerHasFinished && RaceManager.Instance != null)
            {
                if (HasPlayerFinished())
                {
                    playerHasFinished = true;
                    ShowLeaderboard();
                }
            }

            // if leaderboard is showing, keep updating it
            if (isShowing)
            {
                int currentFinishedCount = GetFinishedRacersCount();
                if (currentFinishedCount != lastFinishedCount)
                {
                    lastFinishedCount = currentFinishedCount;
                    UpdateLeaderboard();
                }
            }
        }

        private void OnRaceStateChanged(RaceStateUpdated evt)
        {
            if (evt.CurrentState == ERaceState.Racing)
            {
                playerHasFinished = false;
            }
        }

        private bool HasPlayerFinished()
        {
            if (RaceManager.Instance == null)
            {
                Debug.LogWarning("[RaceLeaderboardUI] RaceManager.Instance is null");
                return false;
            }

            foreach (var kvp in RaceManager.Instance.Racers)
            {
                var kart = kvp.Key;
                var participant = kvp.Value;

                bool isPlayer = IsPlayerKart(kart);

                if (isPlayer)
                {
                    if (participant.HasFinished)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private int GetFinishedRacersCount()
        {
            int count = 0;
            foreach (var kvp in RaceManager.Instance.Racers)
            {
                if (kvp.Value.HasFinished)
                    count++;
            }
            return count;
        }

        public void ShowLeaderboard()
        {
            if (RaceManager.Instance == null) return;

            isShowing = true;
            lastFinishedCount = GetFinishedRacersCount();
            UpdateLeaderboard();

            if (leaderboardPanel != null)
                leaderboardPanel.SetActive(true);
        }

        private void UpdateLeaderboard()
        {
            CollectFinishData();
            BuildLeaderboardUI();
        }

        private void CollectFinishData()
        {
            finishData.Clear();

            if (RaceManager.Instance == null) return;

            Debug.Log("[RaceLeaderboardUI] Collecting finish data...");

            // get all racers sorted by their final positions
            var orderedRacers = RaceManager.Instance.OrderedRacers;

            foreach (var kart in orderedRacers)
            {
                var data = new RaceFinishData(kart);

                // check if they finished
                var participant = RaceManager.Instance.Racers[kart];

                if (participant.HasFinished)
                {
                    data.HasFinished = true;
                    data.FinishTime = participant.FinishTime;
                }

                finishData.Add(data);
            }

            finishData = finishData.OrderBy(d => !d.HasFinished)
                                   .ThenBy(d => d.FinishTime)
                                   .ToList();

            int position = 1;
            foreach (var data in finishData)
            {
                if (data.HasFinished)
                {
                    data.Position = position;
                    position++;
                }
            }
        }

        private void BuildLeaderboardUI()
        {
            // clear existing entries
            foreach (Transform child in leaderboardEntriesParent)
            {
                Destroy(child.gameObject);
            }

            // count how many have finished
            int finishedCount = finishData.Count(d => d.HasFinished);
            int totalRacers = finishData.Count;

            // set title
            if (titleText != null)
            {
                if (finishedCount == totalRacers)
                {
                    titleText.text = "RACE COMPLETE!";
                }
                else
                {
                    titleText.text = $"RACE RESULTS ({finishedCount}/{totalRacers} Finished)";
                }
            }

            // create entries
            foreach (var data in finishData)
            {
                if (leaderboardEntryPrefab != null && leaderboardEntriesParent != null)
                {
                    var entry = Instantiate(leaderboardEntryPrefab, leaderboardEntriesParent);
                    var entryUI = entry.GetComponent<LeaderboardEntry>();

                    if (entryUI != null)
                    {
                        bool isPlayer = IsPlayerKart(data.Kart);
                        entryUI.Setup(data, isPlayer);
                    }
                }
            }

            // set restart prompt
            if (restartPromptText != null)
            {
                restartPromptText.text = "Press R to Restart";
            }
        }

        private bool IsPlayerKart(KartController kart)
        {
            if (kart == null || RaceManager.Instance == null) return false;

            return kart == RaceManager.Instance.PlayerKart;
        }

        private void RestartRace()
        {
            // determine which scene to load
            string sceneToLoad = string.IsNullOrEmpty(restartSceneName)
                ? SceneManager.GetActiveScene().name
                : restartSceneName;

            Time.timeScale = 1f;

            isShowing = false;
            playerHasFinished = false;

            SceneManager.LoadScene(sceneToLoad);
        }

        public void HideLeaderboard()
        {
            isShowing = false;
            if (leaderboardPanel != null)
                leaderboardPanel.SetActive(false);
        }
    }
}