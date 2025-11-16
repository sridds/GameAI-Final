using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarDriver : MonoBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetInputs(float forwardAmount, float turnAmount)
    {

    }
}
