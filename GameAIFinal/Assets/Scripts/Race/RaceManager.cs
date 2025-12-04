using System.Collections;
using System.Collections.Generic;
using Kart.Car;
using Kart.Track;
using UnityEngine;

namespace Kart.Race
{
    public enum ERaceState
    {
        Pregame,
        TimerStarted,
        Racing,
        RaceEnded
    }
    public class RaceManager : MonoBehaviour
    {
        public static RaceManager instance;
        public static CarDriver currentSpectatedRacer;

        [SerializeField] private RaceSpawner spawner;
        [SerializeField] private CheckpointTrack track;
        [SerializeField] private Camera[] alternateCameras;
        [SerializeField] private float pregameTimer = 1.0f;

        [Header("Race SFX")]
        [SerializeField] private SoundStreamSO _countdownSound;
        [SerializeField] private SoundStreamSO _raceStartSound;
        [SerializeField] private SoundStreamSO _spectatorChangeSound;

        public Dictionary<CarDriver, int> RacerCheckpointsPassed { get; private set; } = new();
        public List<CarDriver> OrderedRacers { get; private set; } = new();
        public float TimeElapsed { get; private set; }
        public ERaceState RaceState { get; private set; }

        private List<Camera> allCameras;
        private int currentCameraIndex;
        private ERaceState raceState;

        private void Awake()
        {
            instance = this;

            InitRace();
            InitializeCameras();

            currentSpectatedRacer = null;
            currentCameraIndex = OrderedRacers.Count;
        }

        private void Start()
        {
            ChangeRaceState(ERaceState.Pregame);
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
            // we need to track the furthest checkpoint, along with which lap it was recorded in, and adjust all placements based on this. 

            // itll be based on the most recent checkpoint, itll compare all distances of each racer relative to the most recent checkpoint reached
            // then after grabbing all distances itll compare them to determine placements
            for(int i = 0; i < OrderedRacers.Count; i++)
            {
                // determine all racers and the last checkpoint reached associated with them.

                // if there are multiple racers associated with the same last checkpoint, determine where they are relative to one another and get placement

                // if there is only one racer, that one immediately deserves the spot


            }
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
                AudioManager.instance.PlayAudio(_spectatorChangeSound);
                allCameras[previousIndex].gameObject.SetActive(false);
            }
        }

        private void UpdateCurrentSpectator()
        {
            // we are on an alternate camera
            if(currentCameraIndex >= OrderedRacers.Count)
            {
                currentSpectatedRacer = null;
                Bus<SpectatorChangeCamera>.Raise(new SpectatorChangeCamera() { camera = allCameras[currentCameraIndex], carReference = null });
            }
            // we are spectating a racer
            else
            {
                currentSpectatedRacer = OrderedRacers[currentCameraIndex];
                Bus<SpectatorChangeCamera>.Raise(new SpectatorChangeCamera() { camera = allCameras[currentCameraIndex], carReference = OrderedRacers[currentCameraIndex] });
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
                allCameras.Add(OrderedRacers[i].myCamera);
                OrderedRacers[i].myCamera.gameObject.SetActive(false);
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
        
        private void OnEnable()
        {
            /* Disabling this for now
            foreach (var driver in OrderedRacers)
            {
                driver.CpCollector.onPassedCheckpointDirection.AddListener(OnPassed);
            }*/
        }

        void OnPassed(Checkpoint cp, bool forward)
        {
            
        }

        public void InitRace()
        {
            Debug.Log($"[{name}]: Initializing Race...");
            OrderedRacers = spawner.Spawn();
        }

        public void StartRace()
        {
            StartCoroutine(IStartRace());
        }

        private void ChangeRaceState(ERaceState state)
        {
            ERaceState previous = raceState;
            raceState = state;

            Bus<RaceStateUpdated>.Raise(new RaceStateUpdated() { previousState = previous, currentState = state });
        }

        private IEnumerator IStartRace()
        {
            ChangeRaceState(ERaceState.TimerStarted);
            yield return new WaitForSeconds(pregameTimer);

            // 3
            AudioManager.instance.PlayAudio(_countdownSound);
            Bus<TimerAnnouncement>.Raise(new TimerAnnouncement() { timerAnnouncementType = TimerAnnouncement.ETimerAnnouncement.Three });
            yield return new WaitForSeconds(1.0f);
            // 2
            AudioManager.instance.PlayAudio(_countdownSound);
            Bus<TimerAnnouncement>.Raise(new TimerAnnouncement() { timerAnnouncementType = TimerAnnouncement.ETimerAnnouncement.Two });
            yield return new WaitForSeconds(1.0f);
            // 1
            AudioManager.instance.PlayAudio(_countdownSound);
            Bus<TimerAnnouncement>.Raise(new TimerAnnouncement() { timerAnnouncementType = TimerAnnouncement.ETimerAnnouncement.One });
            yield return new WaitForSeconds(1.0f);

            AudioManager.instance.PlayAudio(_raceStartSound);
            Bus<TimerAnnouncement>.Raise(new TimerAnnouncement() { timerAnnouncementType = TimerAnnouncement.ETimerAnnouncement.Go });
            ChangeRaceState(ERaceState.Racing);
        }
    }
}
