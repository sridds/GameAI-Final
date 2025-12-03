using System.Collections.Generic;
using System.Linq;
using Kart.Car;
using Kart.Track;
using UnityEngine;

namespace Kart.Race
{
    public class RaceManager : MonoBehaviour
    {
        public static RaceManager instance;
        public static CarDriver currentSpectatedRacer;

        [SerializeField] public bool isRacing = false;
        [SerializeField] private RaceSpawner spawner;
        [SerializeField] private float timeElapsed = 0;
        [SerializeField] private CheckpointTrack track;
        [SerializeField] private Camera[] alternateCameras;

        public Dictionary<CarDriver, int> RacerCheckpointsPassed { get; private set; } = new();
        public List<CarDriver> OrderedRacers { get; private set; } = new();
        private List<Camera> allCameras;
        private int currentCameraIndex;

        private void Awake()
        {
            instance = this;

            InitRace();
            InitializeCameras();

            currentSpectatedRacer = null;
            currentCameraIndex = OrderedRacers.Count;
        }

        private void Update()
        {
            UpdateSpectatorInput();
            UpdateCurrentSpectator();
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
            else
            {
                currentSpectatedRacer = OrderedRacers[currentCameraIndex];
                Bus<SpectatorChangeCamera>.Raise(new SpectatorChangeCamera() { camera = allCameras[currentCameraIndex], carReference = OrderedRacers[currentCameraIndex] });
            }

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
    }
}
