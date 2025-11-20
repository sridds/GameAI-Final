using System;
using Unity.VisualScripting;
using UnityEngine;

public class TrackCheckpoints : MonoBehaviour
{
    public event Action OnCarCorrectCheckPoint;
    public event Action OnCarWrongCheckPoint;

    protected virtual void OnOnCarCorrectCheckPoint()
    {
        OnCarCorrectCheckPoint?.Invoke();
    }

    protected virtual void OnOnCarWrongCheckPoint()
    {
        OnCarWrongCheckPoint?.Invoke();
    }

    public void ResetCheckpoint(Transform transform1)
    {
        throw new NotImplementedException();
    }

    public object GetNextCheckpoint(Transform transform1)
    {
        throw new NotImplementedException();
    }
}
