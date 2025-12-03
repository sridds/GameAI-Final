using DG.Tweening;
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
            Bus<TimerAnnouncement>.OnEvent += DisplayTimerAnnouncement;
        }

        private void Update()
        {
            _timeText.text = "Time: " + RaceManager.instance.TimeElapsed.ToString("F2");
        }

        public void DisplayTimerAnnouncement(TimerAnnouncement evt)
        {
            _countdownText.gameObject.SetActive(true);
            _countdownText.transform.localScale = Vector3.one;
            _countdownText.color = Color.white;

            _countdownText.DOKill(false);
            _countdownText.DOFade(0.0f, 0.5f).SetEase(Ease.InQuad);
            _countdownText.transform.DOScale(1.2f, 0.5f).SetEase(Ease.OutQuad);

            switch (evt.timerAnnouncementType)
            {
                case TimerAnnouncement.ETimerAnnouncement.Three:
                    _countdownText.text = "3";
                    break;
                case TimerAnnouncement.ETimerAnnouncement.Two:
                    _countdownText.text = "2";
                    break;
                case TimerAnnouncement.ETimerAnnouncement.One:
                    _countdownText.text = "1";
                    break;
                case TimerAnnouncement.ETimerAnnouncement.Go:
                    _countdownText.text = "GO!";
                    break;
            }
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
