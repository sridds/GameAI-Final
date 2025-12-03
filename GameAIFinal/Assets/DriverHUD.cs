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
            Bus<SpectatingRacerChanged>.OnEvent += UpdateSpectatorUI;
        }

        public void UpdateSpectatorUI(SpectatingRacerChanged evt)
        {
            // if the spectator was updated and there is no racer referenced, disable non-relevant UI
            if(evt.racerReference == null)
            {
                _placementHolder.gameObject.SetActive(false);
                _lapHolder.gameObject.SetActive(false);
            }
            // reenable UI if its relevant to the race
            else
            {
                _placementHolder.gameObject.SetActive(true);
                _lapHolder.gameObject.SetActive(true);
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
