using UnityEngine;

/// <summary>
/// Upon touching, disappear and add an item to the racer that collided with it
/// </summary>
public class MysteryCube : MonoBehaviour
{
    [SerializeField, Tooltip("If true, gives the racer two items instead of one")]
    private bool _isDoubled;

    public void OnTriggerEnter(Collider other)
    {
        // must be a driver
        if (!other.gameObject.TryGetComponent<CarDriver>(out CarDriver driver)) return;

        Debug.Log("Driver collided with mystery cube!");
    }
}