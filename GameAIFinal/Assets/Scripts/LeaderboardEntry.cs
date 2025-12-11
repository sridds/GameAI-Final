using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kart.Race
{
    public class LeaderboardEntry : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI positionText;
        [SerializeField] private TextMeshProUGUI racerNameText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private Image backgroundImage;

        [Header("Colors")]
        [SerializeField] private Color playerColor = new Color(1f, 0.84f, 0f, 0.3f);
        [SerializeField] private Color finisherColor = new Color(1f, 1f, 1f, 0.1f);
        [SerializeField] private Color dnfColor = new Color(0.5f, 0.5f, 0.5f, 0.1f);
        [SerializeField] private Color racingColor = new Color(0.3f, 0.6f, 1f, 0.2f);

        public void Setup(RaceFinishData data, bool isPlayer)
        {
            if (data == null || data.Kart == null) return;

            // position
            if (positionText != null)
            {
                if (data.HasFinished)
                {
                    positionText.text = GetPositionSuffix(data.Position);
                }
                else
                {
                    positionText.text = "---";
                }
            }

            // racer name
            if (racerNameText != null)
            {
                string racerName = data.Kart.RacerID != null
                    ? data.Kart.RacerID.racerName
                    : data.Kart.name;

                if (isPlayer)
                {
                    racerNameText.text = $"{racerName} (You)";
                }
                else
                {
                    racerNameText.text = racerName;
                }
            }

            // time
            if (timeText != null)
            {
                if (data.HasFinished)
                {
                    timeText.text = data.GetFormattedTime();
                }
                else
                {
                    timeText.text = "Racing...";
                }
            }

            // background color
            if (backgroundImage != null)
            {
                if (isPlayer && data.HasFinished)
                {
                    backgroundImage.color = playerColor;
                }
                else if (isPlayer && !data.HasFinished)
                {
                    // player still racing
                    backgroundImage.color = Color.Lerp(playerColor, racingColor, 0.5f);
                }
                else if (data.HasFinished)
                {
                    backgroundImage.color = finisherColor;
                }
                else
                {
                    // still racing
                    backgroundImage.color = racingColor;
                }
            }
        }

        private string GetPositionSuffix(int position)
        {
            string suffix = "th";

            if (position % 100 >= 11 && position % 100 <= 13)
            {
                return position + "th";
            }

            switch (position % 10)
            {
                case 1:
                    suffix = "st";
                    break;
                case 2:
                    suffix = "nd";
                    break;
                case 3:
                    suffix = "rd";
                    break;
            }

            return position + suffix;
        }
    }
}