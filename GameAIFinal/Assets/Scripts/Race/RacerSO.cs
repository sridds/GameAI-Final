using Kart.Car;
using UnityEngine;

namespace Kart.Race
{
    [CreateAssetMenu(menuName = "Super ML Kart/RacerIdentifier")]
    public class RacerSO : ScriptableObject
    {
        public string racerName;
        public Sprite minimapIcon;
        public CarDriver carPrefab;
    }
}
