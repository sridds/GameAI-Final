using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class MoveToGoalAgent : Agent
{
    [SerializeField]
    private Transform targetTransform;
    [SerializeField]
    private float moveSpeed = 2.0f;
    [SerializeField]
    private MeshRenderer planeRenderer;

    public override void OnEpisodeBegin()
    {
        // reset the state for training again
        transform.localPosition = new Vector3(
            Random.Range(-3f, 3f),
            0.5f,
            Random.Range(-3f, 3f));
        targetTransform.localPosition = new Vector3(
            Random.Range(-3f, 3f),
            0.5f,
            Random.Range(-3f, 3f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // how it observes the environment
        // inputs for the AI

        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Goal")
        {
            SetReward(1f);
            planeRenderer.material.color = Color.green;
            EndEpisode();
        }

        if(other.gameObject.tag == "Wall")
        {
            SetReward(-1f);
            planeRenderer.material.color = Color.red;
            EndEpisode();
        }

    }

}
