using System.Collections.Generic;
using UnityEngine;

public class ScoutWander : SteeringBehaviour
{
    private List<Node> currentPath = new List<Node>();
    private Vector3 currentTargetPos = new Vector2();

    public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
    {
        HandleIfAllyNeedsNewNodeToPathFind();

        //get desired velocity to the point
        desiredVelocity = Vector3.Normalize(currentTargetPos - transform.position) * SteeringAgent.MaxCurrentSpeed;

        //calculate steering velocity
        steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;

        return steeringVelocity;
    }

    private void HandleIfAllyNeedsNewNodeToPathFind()
    {
        if (currentPath.Count == 0 && RoleManager.ScoutManager.nodesToScout.Count > 0)
        {
            Node closestNodeToScout = RoleManager.ScoutManager.GetClosestScoutNode(transform.position);
            currentPath = PathfindingAlgorithms.AStar(GridData.Instance.GetNodeAt(transform.position), closestNodeToScout);
            if(currentPath != null && currentPath.Count > 0)
            {
                //remove start node
                currentPath.Remove(GridData.Instance.GetNodeAt(transform.position));

                currentTargetPos = GenerateNewTargetPosWithOffset(currentPath[0]);
                currentPath.RemoveAt(0);
                
            }
            return;
        }


        if(currentPath.Count > 0)
        {
            float distanceToCurrentNode = Vector3.SqrMagnitude(transform.position - currentTargetPos);

            if (distanceToCurrentNode < 1f)
            {
                currentTargetPos = GenerateNewTargetPosWithOffset(currentPath[0]);
                currentPath.RemoveAt(0);
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
}