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
        Vector3 desiredDirection = transform.rotation * transform.up;
        Vector3 posInFront = transform.position + (Vector3.Normalize(desiredDirection));

        if (posInFront.x >= 100 || posInFront.x < 0 || posInFront.y >= 100 || posInFront.y < 0)
        {
            return 180f;
        }


        if (GameData.Instance.Map.GetTerrainAt((int)posInFront.x, (int)posInFront.y) != Map.Terrain.Grass 
            && GameData.Instance.Map.GetTerrainAt((int)transform.position.x, (int)transform.position.y) == Map.Terrain.Grass)
        {
            desiredDirection = transform.rotation * -transform.right;
            Vector3 posToLeft = transform.position + (Vector3.Normalize(desiredDirection));
            
            desiredDirection = transform.rotation * transform.right;
            Vector3 posToRight = transform.position + (Vector3.Normalize(desiredDirection));

            bool isWallLeft = GameData.Instance.Map.GetTerrainAt((int)posToLeft.x, (int)posToLeft.y) != Map.Terrain.Grass;
            bool isWallRight = GameData.Instance.Map.GetTerrainAt((int)posToRight.x, (int)posToRight.y) != Map.Terrain.Grass;

            if (isWallLeft && !isWallRight)
            {
                return 135f;
            }
            else
            {
                return -135f;
            }
        }


        return Random.Range(-maxNewSteeringAngleDelta, maxNewSteeringAngleDelta);
    }

}