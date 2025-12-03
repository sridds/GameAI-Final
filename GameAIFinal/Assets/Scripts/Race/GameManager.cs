using UnityEngine;

namespace Kart.Race
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        void Awake()
        {
        
#if UNITY_EDITOR
            Application.targetFrameRate = 239; // Cap in Editor to save my laptop
#else
    Application.targetFrameRate = -1;
#endif

            if (instance == null)
            {
                instance = this;
            }
        }
    }
}