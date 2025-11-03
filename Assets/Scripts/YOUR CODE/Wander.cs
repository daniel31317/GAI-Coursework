using UnityEngine;

public class Wander : SteeringBehaviour
{
    private float maxNewSteeringAngleDelta = 5f;

    public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
    {
        Quaternion newRotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, transform.rotation.z + GetOffsetForSteering());

        Vector3 desiredDirection = newRotation * transform.up;

        Vector3 targetPoint = transform.position + (Vector3.Normalize(desiredDirection) * 5f);

        //get desired velocity to the point
        desiredVelocity = Vector3.Normalize(targetPoint - transform.position) * SteeringAgent.MaxCurrentSpeed;

        // Calculate steering velocity
        steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;

        return steeringVelocity;
    }


    private float GetOffsetForSteering()
    {
        Vector3 posInFront = transform.position + transform.up;

        if (posInFront.x >= 100 || posInFront.x < 0 || posInFront.y >= 100 || posInFront.y < 0)
        {
            return 180f;
        }

        Node currentPosNode = GridData.Instance.GetNodeAt((int)transform.position.x, (int)transform.position.y);
        Node posInFrontNode = GridData.Instance.GetNodeAt((int)posInFront.x, (int)posInFront.y);

        if(posInFrontNode.terrain == Map.Terrain.Tree)
        {

            //currently not work because it doesnt know which way the player is facing
            //probably do something with the neighbours of current position
            Node posInToRight = GridData.Instance.GetNodeAt((int)posInFront.x + 1, (int)posInFront.y);
            Node posInToLeft = GridData.Instance.GetNodeAt((int)posInFront.x - 1, (int)posInFront.y);

            bool canGoRight = true;
            bool canGoLeft = true;

            if (posInToRight.terrain == Map.Terrain.Tree)
            {
                canGoRight = false;
            }
            if (posInToLeft.terrain == Map.Terrain.Tree)
            {
                canGoLeft = false;
            }

            if (!canGoLeft && !canGoRight)
            {
                return 180f;
            }
            else if (canGoRight && !canGoLeft)
            {
                return 90f;
            }
            else if (canGoLeft)
            {
                return -90f;
            }
        }
        return Random.Range(-maxNewSteeringAngleDelta, maxNewSteeringAngleDelta);
    }

}