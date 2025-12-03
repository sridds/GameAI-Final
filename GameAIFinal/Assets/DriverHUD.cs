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

        private Coroutine currentPlacementCoroutine;

        private void Start()
        {
            // set bindings
            Bus<RacerPlacementUpdated>.OnEvent += UpdatePlacement;
            Bus<SpectatorChangeCamera>.OnEvent += UpdateSpectatorUI;
            Bus<TimerAnnouncement>.OnEvent += DisplayTimerAnnouncement;

            _placementText.text = "";
        }

        private void Update()
        {
            _timeText.text = "Time: " + RaceManager.instance.TimeElapsed.ToString("F2");

            if (Input.GetKeyDown(KeyCode.F))
            {
                Bus<RacerPlacementUpdated>.Raise(new RacerPlacementUpdated() { placement = Random.Range(1, 12) });
            }
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
