using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class TrackCheckpoints : MonoBehaviour
{
    [SerializeField] private Checkpoint firstForward;
    [SerializeField] private Checkpoint middleCheckpoint;
    [SerializeField] private Checkpoint firstBackward;

    Dictionary<CarDriver, Checkpoint> lastCheckpoints = new();
    Dictionary<CarDriver, int> carDirections = new(); 

    public void HandleCheckpoint(CarDriver driver, Checkpoint checkpoint)
    {
        bool hasExitedStart = lastCheckpoints.ContainsKey(driver);
        bool passedMiddle = hasExitedStart 
            && Mathf.Abs(carDirections[driver]) == 2;
        float currentDirection = hasExitedStart ? Mathf.Sign(carDirections[driver]) : 0;

        if (!hasExitedStart)
        {
            // handle starting
            if (checkpoint == firstForward) carDirections.Add(driver, 1);
            else if (checkpoint == firstBackward) carDirections.Add(driver, -1);
            else Debug.LogError("Starting skipped a checkpoint");

            lastCheckpoints.Add(driver, checkpoint);
            return;
        }

    }

}
