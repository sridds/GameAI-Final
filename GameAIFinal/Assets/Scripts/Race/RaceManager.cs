using UnityEngine;

namespace Kart.Race
{
    public class RaceManager : MonoBehaviour
    {
        [SerializeField]
        private RaceSpawner _spawner;
        public void InitializeRace()
        {
            Debug.Log($"[{name}]: Initializing Race...");
            _spawner.Spawn();
        }

        public void StartRace()
        {

        }

        private void FixedUpdate()
        {
        
        }
    }
}
