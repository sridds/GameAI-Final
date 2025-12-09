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

        public RaceParticipant(KartController kart)
        {
            this.kart = kart;
            Laps = 0;
            CheckpointsPassedInLap = 0;
            CheckpointsPassed = 0;
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
        [FormerlySerializedAs("_countdownSound")] [SerializeField] private SoundStreamSO countdownSound;
        [FormerlySerializedAs("_raceStartSound")] [SerializeField] private SoundStreamSO raceStartSound;
        [FormerlySerializedAs("_spectatorChangeSound")] [SerializeField] private SoundStreamSO spectatorChangeSound;

        public Dictionary<KartController, RaceParticipant> Racers = new Dictionary<KartController, RaceParticipant>();
        
        public List<KartController> OrderedRacers { get; private set; } = new();
        
        public float TimeElapsed { get; private set; }
        public ERaceState RaceState { get; private set; }

        private List<Camera> allCameras;
        private int currentCameraIndex;
        private ERaceState raceState;

        private void Awake()
        {
            Instance = this;

            InitRace();
            InitializeCameras();

            CurrentSpectatedRacer = null;
            currentCameraIndex = OrderedRacers.Count;
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

                    if (participant.Laps >= laps)
                    {
                        ChangeRaceState(ERaceState.RaceEnded);
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
            /* Disabling this for now
            foreach (var driver in OrderedRacers)
            {
                driver.CpCollector.onPassedCheckpointDirection.AddListener(OnPassed);
            }*/
            Bus<CheckpointPassedEvent>.OnEvent += OnCheckpointPassed;
        }

        private void OnDisable()
        {
            Bus<CheckpointPassedEvent>.OnEvent -= OnCheckpointPassed;
        }

        private void Update()
        {
            // start race by pressing mouse down
            if (Input.GetMouseButtonDown(0) && raceState == ERaceState.Pregame)
            {
                StartRace();
            }

            if (raceState == ERaceState.Racing) UpdateRace();
        }

        private void UpdateRace()
        {
            TimeElapsed += Time.deltaTime;

            // we can spectate and change cameras during race
            UpdateSpectatorInput();
            UpdateCurrentSpectator();
            UpdateRaceLeaderboard();
        }

        private void UpdateRaceLeaderboard()
        {
            //sort racers by: laps, checkpoints in lap, distance to next checkpoint
            OrderedRacers.Sort((a, b) =>
            {
                var participantA = Racers[a];
                var participantB = Racers[b];

                // laps sort
                if (participantA.Laps != participantB.Laps)
                {
                    return participantB.Laps.CompareTo(participantA.Laps);
                }

                // checkpoints in current lap sort
                if (participantA.CheckpointsPassedInLap != participantB.CheckpointsPassedInLap)
                {
                    return participantB.CheckpointsPassedInLap.CompareTo(participantA.CheckpointsPassedInLap);
                }

                // distane to next checkpoint sort
                var sensorA = a.GetComponent<CheckpointSensor>();
                var sensorB = b.GetComponent<CheckpointSensor>();

                if (sensorA == null || sensorB == null) return 0;

                var nextCheckpointA = track.GetNextCheckpoint(sensorA);
                var nextCheckpointB = track.GetNextCheckpoint(sensorB);

                if (nextCheckpointA == null || nextCheckpointB == null) return 0;

                // racers on different checkpoints
                if (nextCheckpointA != nextCheckpointB) return 0;

                // calculate distance to next checkpoint
                var distA = Vector3.Distance(a.transform.position, nextCheckpointA.Col.bounds.center);
                var distB = Vector3.Distance(b.transform.position, nextCheckpointB.Col.bounds.center);

                return distA.CompareTo(distB);
            });

            // raise placement events for changed pos
            for (int i = 0; i < OrderedRacers.Count; i++)
            {
                Bus<RacerPlacementUpdated>.Raise(new RacerPlacementUpdated()
                {
                    Placement = i + 1,
                    RacerReference = OrderedRacers[i].RacerID
                });
            }

            DebugRacePositions();
        }

        private void DebugRacePositions()
        {
            Debug.Log("=== RACE POSITIONS ===");

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
            int previousIndex = currentCameraIndex;
            // go back
            if (Input.GetMouseButtonDown(0)) currentCameraIndex--;
            // go forward
            if (Input.GetMouseButtonDown(1)) currentCameraIndex++;

            // adjust index
            currentCameraIndex %= allCameras.Count;
            if(currentCameraIndex < 0) currentCameraIndex = allCameras.Count - 1;

            if (currentCameraIndex != previousIndex)
            {
                // disable current to prepare for next
                AudioManager.instance.PlayAudio(spectatorChangeSound);
                allCameras[previousIndex].gameObject.SetActive(false);
            }
        }

        private void UpdateCurrentSpectator()
        {
            // we are on an alternate camera
            if(currentCameraIndex >= OrderedRacers.Count)
            {
                CurrentSpectatedRacer = null;
                Bus<SpectatorChangeCamera>.Raise(
                    new SpectatorChangeCamera()
                    {
                        Race = this,
                        Camera = allCameras[currentCameraIndex], CarReference = null
                    });
            }
            // we are spectating a racer
            else
            {
                CurrentSpectatedRacer = OrderedRacers[currentCameraIndex];
                Bus<SpectatorChangeCamera>.Raise(
                    new SpectatorChangeCamera()
                    {
                        Race = this,
                        Camera = allCameras[currentCameraIndex], CarReference = OrderedRacers[currentCameraIndex]
                    });
            }

            // set the current camera active
            allCameras[currentCameraIndex].gameObject.SetActive(true);

        }

        private void InitializeCameras()
        {
            allCameras = new List<Camera>();

            // add all cameras from racers, disable all
            for (int i = 0; i < OrderedRacers.Count; i++)
            {
                allCameras.Add(OrderedRacers[i].Cam);
                OrderedRacers[i].Cam.gameObject.SetActive(false);
            }

            // add alternate cameras, disable all
            for (int i = 0; i < alternateCameras.Length; i++)
            {
                allCameras.Add(alternateCameras[i]);
                alternateCameras[i].gameObject.SetActive(false);
            }

            // activate only one overhead
            alternateCameras[0].gameObject.SetActive(true);
        }
        
        public void InitRace()
        {
            Debug.Log($"[{name}]: Initializing Race...");
            var racers = spawner.Spawn();
            
            OrderedRacers.Clear();
            Racers = new Dictionary<KartController, RaceParticipant>();
            foreach (var racer in racers)
            {
                var kart = racer.GetComponent<KartController>();
                if (kart != null)
                {
                    OrderedRacers.Add(kart);
                    Racers.Add(kart, new RaceParticipant(kart));
                }
                else
                {
                    Debug.LogWarning("No KartController component found on racer: " + racer.name);
                }
            }
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
            AudioManager.instance.PlayAudio(countdownSound);
            Bus<TimerAnnouncement>.Raise(new TimerAnnouncement() { TimerAnnouncementType = TimerAnnouncement.ETimerAnnouncement.Three });
            yield return new WaitForSeconds(1.0f);
            // 2
            AudioManager.instance.PlayAudio(countdownSound);
            Bus<TimerAnnouncement>.Raise(new TimerAnnouncement() { TimerAnnouncementType = TimerAnnouncement.ETimerAnnouncement.Two });
            yield return new WaitForSeconds(1.0f);
            // 1
            AudioManager.instance.PlayAudio(countdownSound);
            Bus<TimerAnnouncement>.Raise(new TimerAnnouncement() { TimerAnnouncementType = TimerAnnouncement.ETimerAnnouncement.One });
            yield return new WaitForSeconds(1.0f);

            AudioManager.instance.PlayAudio(raceStartSound);
            Bus<TimerAnnouncement>.Raise(new TimerAnnouncement() { TimerAnnouncementType = TimerAnnouncement.ETimerAnnouncement.Go });
            ChangeRaceState(ERaceState.Racing);
        }

        public int GetCurrentLap(KartController kart)
        {
            return Racers[kart].Laps;
        }
    }
}
