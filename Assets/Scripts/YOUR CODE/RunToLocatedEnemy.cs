using System.Collections.Generic;
using UnityEngine;

public class RunToLocatedEnemy : SteeringBehaviour
{
    private int currentPathIndex = 0;
    private Vector3 currentTargetPos = new Vector3();
    private List<Node> currentPath = new List<Node>();

    public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
    {
        HandleAIPathfinding();

        //get desired velocity to the point
        desiredVelocity = Vector3.Normalize(currentTargetPos - transform.position) * SteeringAgent.MaxCurrentSpeed;

        //calculate steering velocity
        steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;

        return steeringVelocity;
    }



    private void HandleAIPathfinding()
    {
        if(currentPath == null)
        {
            return;
        }
        
        if (currentPathIndex < currentPath.Count - 1)
        {
            float distanceToCurrentNode = Vector3.SqrMagnitude(transform.position - currentTargetPos);

            if (distanceToCurrentNode < 1f)
            {
                //get new node
                currentPathIndex++;
                currentTargetPos = GenerateNewTargetPosWithOffset(currentPath[currentPathIndex]);
            }
        }
    }


    private Vector3 GenerateNewTargetPosWithOffset(Node node)
    {
        Vector3 newTargetPos = new Vector3(node.position.x, node.position.y, 0f);
        newTargetPos.x += Random.Range(0f, 1f);
        newTargetPos.y += Random.Range(0f, 1f);
        return newTargetPos;
    }


    public void SetCurrentPath(List<Node> path)
    {
        currentPath = path;
        currentTargetPos = currentPath[0].position;
        currentPathIndex = 0;
    }
}
