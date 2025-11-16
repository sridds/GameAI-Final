using UnityEngine;

public class GameManager : MonoBehaviour
{
}

public class RaceManager : MonoBehaviour
{
    [SerializeField] private float _countdown = 3.0f;

    public void StartCountdown()
    {

    }
    public void StartRace()
    {

    }
}

public class RacerIdentifier : ScriptableObject
{
    public Sprite Icon;
    public string Name;
}
