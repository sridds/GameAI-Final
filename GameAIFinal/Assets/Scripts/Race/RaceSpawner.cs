using UnityEngine;

// Responsible for spawning racers on the track in a randomized formation
namespace Kart.Race
{
    public class RaceSpawner : MonoBehaviour
    {
        [SerializeField]
        private RacerIdentifier[] _racerCatalogue;

        public void Spawn()
        {
            Debug.Log($"[{name}]: Spawning Racers");
        }
    }
}
