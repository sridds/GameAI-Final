using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CarDriver))]
public class CarDriverAgent : Agent
{
    [SerializeField] private CheckpointTrack track;
    [SerializeField] private Transform spawnPosition;
    
    private CarDriver _carDriver;

    protected override void Awake()
    {
        _carDriver = GetComponent<CarDriver>();
    }

    public override void OnEpisodeBegin()
    {
        transform.position = spawnPosition.position + new Vector3(Random.Range(-5f, +5f), 0.0f, Random.Range(-5f, 5f));
        transform.forward = spawnPosition.forward;
        // TODO: reset checkpoints
        _carDriver.StopCompletely();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        
        //Vector3 checkpointForward = trackCheckpoints.GetNextCheckpoint(transform).transform.forward;
        // float directionDot = Vector3.Dot(transform.forward, checkpointForward);
        // sensor.AddObservation(directionDot);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float forwardAmount = 0f;
        float turnAmount = 0f;

        switch (actions.DiscreteActions[0])
        {
            case 0: forwardAmount = 0f; break;
            case 1: forwardAmount = 1f; break;
            case 2: forwardAmount = -1f; break;
        }
        switch (actions.DiscreteActions[2])
        {
            case 0: turnAmount = 0f; break;
            case 1: turnAmount = 1f; break;
            case 2: turnAmount = -1f; break;
        }
        
        _carDriver.SetInputs(forwardAmount, turnAmount);
        
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int forwardAction = 0;
        if (Input.GetKey(KeyCode.W)) forwardAction = 1;
        if (Input.GetKey(KeyCode.S)) forwardAction = 2;

        int turnAction = 0;
        if (Input.GetKey(KeyCode.D)) forwardAction = 1;
        if (Input.GetKey(KeyCode.A)) forwardAction = 2;
        
        ActionSegment<int> discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = forwardAction;
        discreteActionsOut[1] = turnAction;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.TryGetComponent<Wall>(out Wall wall))
        {
            AddReward(-0.5f);
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.TryGetComponent<Wall>(out Wall wall))
        {
            AddReward(-0.1f);
        }
    }
}
