using NUnit.Framework;
using System.Collections.Generic;
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
        Vector3 posInFront = transform.position + (transform.up * 2);

        DebugDrawCircle("Yippee", posInFront, 0.5f, Color.yellow);

        bool skipTerrainCheck = false;

        if (posInFront.x >= 100 || posInFront.x < 0 || posInFront.y >= 100 || posInFront.y < 0)
        {
            skipTerrainCheck = true;
        }

        Node currentPosNode = GridData.Instance.GetNodeAt((int)transform.position.x, (int)transform.position.y);
        Node posInFrontNode = new Node();

        if (!skipTerrainCheck)
        {
            posInFrontNode = GridData.Instance.GetNodeAt((int)posInFront.x, (int)posInFront.y);
        }
        

        if(skipTerrainCheck || posInFrontNode.terrain == Map.Terrain.Tree)
        {

            List<Node> possibleMoveTiles = new List<Node>();
            foreach (Node node in currentPosNode.neighbours)
            {
                if(node.terrain != Map.Terrain.Tree)
                {
                    possibleMoveTiles.Add(node);
                }
            }

            //just to be sure there is a valid node
            if(possibleMoveTiles.Count == 0)
            {
                return 180f;
            }

            int randomIndex = Random.Range(0, possibleMoveTiles.Count - 1);

            Node pickedNode = possibleMoveTiles[randomIndex];
            Vector3 dir = transform.position - new Vector3(pickedNode.position.x, pickedNode.position.y, 0f); 
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
            return rot.eulerAngles.y;
        }
        return Random.Range(-maxNewSteeringAngleDelta, maxNewSteeringAngleDelta);
    }

}