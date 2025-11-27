using Kart.Car;
using UnityEngine;

namespace Kart.Race
{
    [CreateAssetMenu(menuName = "Super ML Kart/RacerIdentifier")]
    public class RacerIdentifier : ScriptableObject
    {
        public CarDriver CarPrefab;
        public Sprite Icon;
        public string Name;
    }
}
