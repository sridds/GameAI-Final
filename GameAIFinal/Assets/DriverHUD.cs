using Kart.Race;
using TMPro;
using UnityEngine;

namespace Kart
{
    public class DriverHUD : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private RectTransform _placementHolder;
        [SerializeField]
        private RectTransform _lapHolder;
        [SerializeField] 
        private TextMeshProUGUI _placementText;
        [SerializeField]
        private TextMeshProUGUI _timeText;
        [SerializeField]
        private TextMeshProUGUI _lapText;
        [SerializeField]
        private TextMeshProUGUI _countdownText;
        [SerializeField]
        private TextMeshProUGUI _spectatorTargetText;

        private void Start()
        {
            // set bindings
            Bus<RacerPlacementUpdated>.OnEvent += UpdatePlacement;
            Bus<SpectatorChangeCamera>.OnEvent += UpdateSpectatorUI;
        }

        private void Update()
        {
            _timeText.text = "Time: " + RaceManager.instance.TimeElapsed.ToString("F2");
        }

        public void UpdateSpectatorUI(SpectatorChangeCamera evt)
        {
            // if the spectator was updated and there is no racer referenced, disable non-relevant UI
            if(evt.carReference == null)
            {
                _placementHolder.gameObject.SetActive(false);
                _lapHolder.gameObject.SetActive(false);
            }
            // reenable UI if its relevant to the race
            else
            {
                _placementHolder.gameObject.SetActive(true);
                _lapHolder.gameObject.SetActive(true);

                // set lap text
                _lapText.text = $"Lap: {evt.carReference.currentLap}/3";
            }

            _spectatorTargetText.text = "Spectating: ";
            // set text to racer name
            if (evt.carReference != null)
            {
                _spectatorTargetText.text += evt.carReference.racerID.racerName;
            }
            // set text to camera name
            else
            {
                _spectatorTargetText.text += evt.camera.gameObject.name;
            }
        }

        public void UpdatePlacement(RacerPlacementUpdated evt)
        {
            if (evt.racerReference == RaceManager.currentSpectatedRacer) return;

            string placeStr = evt.placement.ToString();

            // Create placement string
            if (evt.placement == 1) placeStr += "st";
            else if (evt.placement == 2) placeStr += "nd";
            else if (evt.placement == 3) placeStr += "rd";
            else placeStr += "th";

            _placementText.text = placeStr;
        }
    }
}
