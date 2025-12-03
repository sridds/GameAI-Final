using DG.Tweening;
using Kart.Race;
using System.Collections;
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
        [SerializeField]
        private GameObject _pregameHolder;
        [SerializeField]
        private GameObject _driverUIHolder;
        [SerializeField]
        private GameObject _spectatorUIHolder;

        private Coroutine currentPlacementCoroutine;

        private void Start()
        {
            // set bindings
            Bus<RacerPlacementUpdated>.OnEvent += UpdatePlacement;
            Bus<SpectatorChangeCamera>.OnEvent += UpdateSpectatorUI;
            Bus<TimerAnnouncement>.OnEvent += DisplayTimerAnnouncement;
            Bus<RaceStateUpdated>.OnEvent += UpdateActiveMenus;

            _placementText.text = "";

            // set menus active state at the start to reflect pregame menus
            _pregameHolder.SetActive(true);
            _driverUIHolder.SetActive(false);
            _spectatorUIHolder.SetActive(false);
        }

        private void UpdateActiveMenus(RaceStateUpdated evt)
        {
            // Display driver / spectator UI if the timer has started
            if(evt.currentState == ERaceState.TimerStarted)
            {
                _pregameHolder.SetActive(false);
                _driverUIHolder.SetActive(true);
                _spectatorUIHolder.SetActive(false);
            }

            else if (evt.currentState == ERaceState.Racing)
            {
                _pregameHolder.SetActive(false);
                _driverUIHolder.SetActive(true);
                _spectatorUIHolder.SetActive(true);
            }
        }

        private void Update()
        {
            _timeText.text = "Time: " + RaceManager.instance.TimeElapsed.ToString("F2");
        }

        public void DisplayTimerAnnouncement(TimerAnnouncement evt)
        {
            // Reset values for countdown text
            _countdownText.gameObject.SetActive(true);
            _countdownText.transform.localScale = Vector3.one;
            _countdownText.color = Color.white;

            // Start animation
            _countdownText.DOKill(false);
            _countdownText.text = evt.timerMapping[(int)evt.timerAnnouncementType];

            // number animation
            if(evt.timerAnnouncementType != TimerAnnouncement.ETimerAnnouncement.Go)
            {
                _countdownText.DOFade(0.0f, 0.5f).SetEase(Ease.InQuad);
                _countdownText.transform.DOScale(1.2f, 0.5f).SetEase(Ease.OutQuad);
            }
            // GO! animation
            else
            {
                _countdownText.transform.DOScale(1.4f, 0.5f).SetEase(Ease.OutQuad);
                _countdownText.DOFade(0.0f, 0.5f).SetEase(Ease.InQuad).SetDelay(0.3f);
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

        /// <summary>
        /// Updates the placement of the racer (if the racer is on screen)
        /// </summary>
        public void UpdatePlacement(RacerPlacementUpdated evt)
        {
            if (evt.racerReference == RaceManager.currentSpectatedRacer) return;

            string placeStr = evt.placement.ToString();

            // Create placement string
            if (evt.placement == 1) placeStr += "st";
            else if (evt.placement == 2) placeStr += "nd";
            else if (evt.placement == 3) placeStr += "rd";
            else placeStr += "th";

            // stop current animation before playing new one
            if(currentPlacementCoroutine != null) StopCoroutine(currentPlacementCoroutine);
            currentPlacementCoroutine = StartCoroutine(IUpdatePlacement(placeStr));
        }

        /// <summary>
        /// Helper function that does some nice animation on the placement text using Demigant's DOTween libray
        /// </summary>
        private IEnumerator IUpdatePlacement(string newPlacementString)
        {
            _placementText.DOKill(false);

            _placementText.rectTransform.DOAnchorPosY(28f, 0.25f).SetEase(Ease.OutQuad);
            yield return _placementText.DOFade(0.0f, 0.25f).SetEase(Ease.OutQuad).WaitForCompletion();
            _placementText.text = newPlacementString;

            _placementText.rectTransform.DOAnchorPosY(100f, 0.25f).SetEase(Ease.OutQuad);
            yield return _placementText.DOFade(1.0f, 0.25f).SetEase(Ease.OutQuad).WaitForCompletion();

            currentPlacementCoroutine = null;
        }
    }
}
