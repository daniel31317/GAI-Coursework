using UnityEngine;

public class Wander : SteeringBehaviour
{
    private float maxNewSteeringAngleDelta = 5f;
    private float currentSteeringAngle = 0f;

    public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
    {
        //get random angle between the max and min delta
        float offset = UnityEngine.Random.Range(-maxNewSteeringAngleDelta, maxNewSteeringAngleDelta);

        currentSteeringAngle += offset;

        Quaternion newRotation = Quaternion.Euler(0f, currentSteeringAngle, 0f);

        Vector3 desiredDirection = newRotation * transform.forward;

        Vector3 targetPoint = transform.position + (Vector3.Normalize(desiredDirection) * 5f);

        //get desired velocity to the point
        desiredVelocity = Vector3.Normalize(targetPoint - transform.position) * SteeringAgent.MaxCurrentSpeed;

        // Calculate steering velocity
        steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;

        steeringVelocity.y = steeringVelocity.z;
        steeringVelocity.z = 0f;

        return steeringVelocity;
    }
}
