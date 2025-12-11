using System;
using System.Collections;
using System.Collections.Generic;
using Kart.Car;
using Kart.Track;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kart.Race
{
    public enum ERaceState
    {
        Pregame,
        TimerStarted,
        Racing,
        RaceEnded
    }

    public class RaceParticipant
    {
        private KartController kart;
        public int Laps;
        public int CheckpointsPassedInLap;
        public int CheckpointsPassed;
        public float FinishTime;
        public bool HasFinished;

        public RaceParticipant(KartController kart)
        {
            this.kart = kart;
            Laps = 1;
            CheckpointsPassedInLap = 0;
            CheckpointsPassed = 0;
            FinishTime = 0f;
            HasFinished = false;
        }
    }

    public class RaceManager : MonoBehaviour
    {
        public static RaceManager Instance;
        public static KartController CurrentSpectatedRacer;

        [Header("Race settings")]
        [SerializeField] private int laps = 3;
        public int Laps => laps;

        [Header("Tweaks")]
        [SerializeField] private float pregameTimer = 1.0f;

        [Header("References")]
        [SerializeField] private RaceSpawner spawner;
        [SerializeField] private CheckpointTrack track;
        [SerializeField] private Camera[] alternateCameras;

        [Header("Race SFX")]
        [FormerlySerializedAs("_countdownSound")][SerializeField] private SoundStreamSO countdownSound;
        [FormerlySerializedAs("_raceStartSound")][SerializeField] private SoundStreamSO raceStartSound;
        [FormerlySerializedAs("_spectatorChangeSound")][SerializeField] private SoundStreamSO spectatorChangeSound;

        public Dictionary<KartController, RaceParticipant> Racers = new Dictionary<KartController, RaceParticipant>();

        public List<KartController> OrderedRacers { get; private set; } = new();
        private List<KartController> racersFixedOrder = new List<KartController>();

        public KartController PlayerKart { get; private set; }

        public float TimeElapsed { get; private set; }
        public ERaceState RaceState { get; private set; }

        private List<Camera> allCameras;
        private int currentCameraIndex;
        private ERaceState raceState;
        private KartController playerKart;
        private bool raceStarted = false;
        private int finishedRacersCount = 0;

        private void Awake()
        {
            Instance = this;

            InitRace();
            InitializeCameras();

            CurrentSpectatedRacer = null;
            currentCameraIndex = racersFixedOrder.Count;
        }

        private void OnCheckpointPassed(CheckpointPassedEvent evt)
        {
            // get kart from event
            var passingKart = evt.Kart;
            if (passingKart == null || !Racers.ContainsKey(passingKart)) return;

            var participant = Racers[passingKart];

            if (evt.IsForward)
            {
                participant.CheckpointsPassed++;
                participant.CheckpointsPassedInLap++;

                // lap check
                if (participant.CheckpointsPassedInLap >= track.CheckpointCount)
                {
                    participant.Laps++;
                    participant.CheckpointsPassedInLap = 0;

                    // lap update event
                    Bus<RacerLapUpdated>.Raise(new RacerLapUpdated()
                    {
                        Racer = passingKart,
                        CurrentLap = participant.Laps,
                        TotalLaps = laps
                    });

                    // racer finished the race
                    if (participant.Laps > laps && !participant.HasFinished)
                    {
                        participant.HasFinished = true;
                        participant.FinishTime = TimeElapsed;
                        finishedRacersCount++;

                        // check if this is the first finisher
                        if (finishedRacersCount == 1)
                        {
                            ChangeRaceState(ERaceState.RaceEnded);
                        }
                    }
                }
            }
            // going backwards
            else
            {
                participant.CheckpointsPassed--;
                participant.CheckpointsPassedInLap--;

                // if they went backward past the start line of their current lap
                if (participant.CheckpointsPassedInLap < 0)
                {
                    // only decrement lap if they have completed at least one lap
                    if (participant.Laps > 1)
                    {
                        participant.Laps--;
                        participant.CheckpointsPassedInLap = track.CheckpointCount - 1;

                        // lap update event
                        Bus<RacerLapUpdated>.Raise(new RacerLapUpdated()
                        {
                            Racer = passingKart,
                            CurrentLap = participant.Laps,
                            TotalLaps = laps
                        });
                    }
                    else
                    {
                        // clamp at 0 if theyre on lap 1
                        participant.CheckpointsPassedInLap = 0;
                    }
                }
            }
        }

        private void Start()
        {
            ChangeRaceState(ERaceState.Pregame);
        }

        private void OnEnable()
        {
            Bus<CheckpointPassedEvent>.OnEvent += OnCheckpointPassed;
        }

        private void OnDisable()
        {
            Bus<CheckpointPassedEvent>.OnEvent -= OnCheckpointPassed;
        }

        private void Update()
        {
            // manual start race by pressing mouse down
            if (Input.GetMouseButtonDown(0) && raceState == ERaceState.Pregame && !raceStarted)
            {
                StartRace();
                return;
            }

            if (raceState == ERaceState.Racing || raceState == ERaceState.RaceEnded)
            {
                UpdateRace();
            }
        }

        private void UpdateRace()
        {
            TimeElapsed += Time.deltaTime;

            // spectator controls during race
            UpdateSpectatorInput();
            UpdateCurrentSpectator();
            UpdateRaceLeaderboard();
        }

        private void UpdateRaceLeaderboard()
        {
            // sort by total progress: higher lap = better, higher checkpoints in lap = better, closer to next checkpoint = better
            OrderedRacers.Sort((a, b) =>
            {
                var participantA = Racers[a];
                var participantB = Racers[b];

                // compare laps first
                int lapCompare = participantB.Laps.CompareTo(participantA.Laps);
                if (lapCompare != 0) return lapCompare;

                // same lap, compare checkpoints passed in this lap
                int cpCompare = participantB.CheckpointsPassedInLap.CompareTo(participantA.CheckpointsPassedInLap);
                if (cpCompare != 0) return cpCompare;

                // same checkpoint progress compare distance to next checkpoint
                var sensorA = a.GetComponent<CheckpointSensor>();
                var sensorB = b.GetComponent<CheckpointSensor>();
                if (sensorA == null || sensorB == null) return 0;

                var nextCheckpointA = track.GetNextCheckpoint(sensorA);
                var nextCheckpointB = track.GetNextCheckpoint(sensorB);
                if (nextCheckpointA == null || nextCheckpointB == null) return 0;
                if (nextCheckpointA != nextCheckpointB) return 0;

                float distA = Vector3.Distance(a.transform.position, nextCheckpointA.Col.bounds.center);
                float distB = Vector3.Distance(b.transform.position, nextCheckpointB.Col.bounds.center);
                return distA.CompareTo(distB);
            });

            for (int i = 0; i < OrderedRacers.Count; i++)
            {
                Bus<RacerPlacementUpdated>.Raise(new RacerPlacementUpdated()
                {
                    Placement = i + 1,
                    RacerReference = OrderedRacers[i].RacerID
                });
            }

            //DebugRacePositions();
        }

        private void DebugRacePositions()
        {
            Debug.Log("=== race positions ===");

            for (int i = 0; i < OrderedRacers.Count; i++)
            {
                var kart = OrderedRacers[i];
                var participant = Racers[kart];
                Debug.Log($"p{i + 1}: {kart.RacerID.racerName} | lap {participant.Laps}/{laps} | cp {participant.CheckpointsPassedInLap}/{track.CheckpointCount} | total cp: {participant.CheckpointsPassed}");
            }

            Debug.Log("===================");
        }

        private void UpdateSpectatorInput()
        {
            // dont allow camera switching until race has started
            if (!raceStarted) return;

            int previousIndex = currentCameraIndex;
            // go back
            if (Input.GetMouseButtonDown(0)) currentCameraIndex--;
            // go forward
            if (Input.GetMouseButtonDown(1)) currentCameraIndex++;

            // adjust index
            currentCameraIndex %= allCameras.Count;
            if (currentCameraIndex < 0) currentCameraIndex = allCameras.Count - 1;

            if (currentCameraIndex != previousIndex)
            {
                // disable current to prepare for next
                if (spectatorChangeSound != null)
                    AudioManager.instance.PlayAudio(spectatorChangeSound);
                allCameras[previousIndex].gameObject.SetActive(false);
            }
        }

        private void InitializeCameras()
        {
            allCameras = new List<Camera>();

            // find player kart
            PlayerKart = null;
            foreach (var kart in racersFixedOrder)
            {
                if (kart.GetComponent<KartAgent>() == null)
                {
                    PlayerKart = kart;
                    break;
                }
            }

            //fallback
            if (PlayerKart == null && racersFixedOrder.Count > 0)
            {
                PlayerKart = racersFixedOrder[0];
            }

            Debug.Log($"[RaceManager] Player kart identified: {(PlayerKart != null ? PlayerKart.name : "NULL")}");

            // add player camera first
            if (PlayerKart != null && PlayerKart.Cam != null)
            {
                allCameras.Add(PlayerKart.Cam);
                PlayerKart.Cam.gameObject.SetActive(false);
            }

            for (int i = 0; i < racersFixedOrder.Count; i++)
            {
                if (racersFixedOrder[i] == PlayerKart) continue;

                if (racersFixedOrder[i].Cam != null)
                {
                    allCameras.Add(racersFixedOrder[i].Cam);
                    racersFixedOrder[i].Cam.gameObject.SetActive(false);
                }
            }

            for (int i = 0; i < alternateCameras.Length; i++)
            {
                allCameras.Add(alternateCameras[i]);
                alternateCameras[i].gameObject.SetActive(false);
            }

            if (PlayerKart != null && PlayerKart.Cam != null)
            {
                currentCameraIndex = 0;
                PlayerKart.Cam.gameObject.SetActive(true);
            }
            else if (alternateCameras.Length > 0)
            {
                currentCameraIndex = allCameras.Count - alternateCameras.Length;
                alternateCameras[0].gameObject.SetActive(true);
            }
        }

        private void UpdateCurrentSpectator()
        {
            // determine which camera/racer we spectating
            int racerCameraCount = allCameras.Count - alternateCameras.Length;

            // we are on an alternate camera
            if (currentCameraIndex >= racerCameraCount)
            {
                CurrentSpectatedRacer = null;
                Bus<SpectatorChangeCamera>.Raise(
                    new SpectatorChangeCamera()
                    {
                        Race = this,
                        Camera = allCameras[currentCameraIndex],
                        CarReference = null
                    });
            }
            // we are spectating a racer
            else
            {
                // find which racer this camera belongs to
                KartController spectatedKart = null;

                if (currentCameraIndex == 0 && PlayerKart != null)
                {
                    spectatedKart = PlayerKart;
                }
                else
                {
                    // search for matching camera
                    for (int i = 0; i < racersFixedOrder.Count; i++)
                    {
                        if (racersFixedOrder[i].Cam == allCameras[currentCameraIndex])
                        {
                            spectatedKart = racersFixedOrder[i];
                            break;
                        }
                    }
                }

                CurrentSpectatedRacer = spectatedKart;
                Bus<SpectatorChangeCamera>.Raise(
                    new SpectatorChangeCamera()
                    {
                        Race = this,
                        Camera = allCameras[currentCameraIndex],
                        CarReference = spectatedKart
                    });
            }

            // set the current camera active
            allCameras[currentCameraIndex].gameObject.SetActive(true);
        }

        public void InitRace()
        {
            Debug.Log($"[{name}]: initializing race...");
            var racers = spawner.Spawn();

            OrderedRacers.Clear();
            racersFixedOrder.Clear();
            Racers = new Dictionary<KartController, RaceParticipant>();
            finishedRacersCount = 0;

            foreach (var racer in racers)
            {
                var kart = racer.GetComponent<KartController>();
                if (kart != null)
                {
                    OrderedRacers.Add(kart);
                    racersFixedOrder.Add(kart);
                    Racers.Add(kart, new RaceParticipant(kart));

                    // initialize ai agents for gameplay
                    var agent = kart.GetComponent<KartAgent>();
                    if (agent != null && !agent.IsTrainingMode)
                    {
                        agent.InitializeForGameplay();
                    }
                }
                else
                {
                    Debug.LogWarning("no kartcontroller component found on racer: " + racer.name);
                }
            }

            Debug.Log($"initialized {Racers.Count} racers");
        }

        public void StartRace()
        {
            StartCoroutine(IStartRace());
        }

        private void ChangeRaceState(ERaceState state)
        {
            ERaceState previous = raceState;
            raceState = state;

            Bus<RaceStateUpdated>.Raise(new RaceStateUpdated() { PreviousState = previous, CurrentState = state });
        }

        private IEnumerator IStartRace()
        {
            ChangeRaceState(ERaceState.TimerStarted);
            yield return new WaitForSeconds(pregameTimer);

            // 3
            if (countdownSound != null)
                AudioManager.instance.PlayAudio(countdownSound);
            Bus<TimerAnnouncement>.Raise(new TimerAnnouncement() { TimerAnnouncementType = TimerAnnouncement.ETimerAnnouncement.Three });
            yield return new WaitForSeconds(1.0f);
            // 2
            if (countdownSound != null)
                AudioManager.instance.PlayAudio(countdownSound);
            Bus<TimerAnnouncement>.Raise(new TimerAnnouncement() { TimerAnnouncementType = TimerAnnouncement.ETimerAnnouncement.Two });
            yield return new WaitForSeconds(1.0f);
            // 1
            if (countdownSound != null)
                AudioManager.instance.PlayAudio(countdownSound);
            Bus<TimerAnnouncement>.Raise(new TimerAnnouncement() { TimerAnnouncementType = TimerAnnouncement.ETimerAnnouncement.One });
            yield return new WaitForSeconds(1.0f);

            if (raceStartSound != null)
                AudioManager.instance.PlayAudio(raceStartSound);
            Bus<TimerAnnouncement>.Raise(new TimerAnnouncement() { TimerAnnouncementType = TimerAnnouncement.ETimerAnnouncement.Go });

            // mark race as started so karts can move
            raceStarted = true;
            currentCameraIndex = 0;

            yield return null;

            ChangeRaceState(ERaceState.Racing);

            currentCameraIndex = 0;
        }

        public int GetCurrentLap(KartController kart)
        {
            if (Racers.ContainsKey(kart))
                return Racers[kart].Laps;
            return 1;
        }

        public int GetPlayerPlacement()
        {
            if (PlayerKart == null) return -1;

            for (int i = 0; i < OrderedRacers.Count; i++)
            {
                if (OrderedRacers[i] == PlayerKart)
                    return i + 1;
            }

            return -1;
        }

        public bool HasRaceStarted()
        {
            return raceStarted;
        }
    }
}