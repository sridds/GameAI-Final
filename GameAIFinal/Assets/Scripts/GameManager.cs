using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Awake()
    {
        
#if UNITY_EDITOR
        Application.targetFrameRate = 239; // Cap in Editor to save my laptop
#else
    Application.targetFrameRate = -1;
#endif
        
    }
}