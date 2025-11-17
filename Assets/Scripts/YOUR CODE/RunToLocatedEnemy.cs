using System.Collections.Generic;
using UnityEngine;

public class RunToLocatedEnemy : SteeringBehaviour
{
    private List<Node> currentPath = new List<Node>();
    private int currentPathIndex = 0;
    private Vector3 currentTargetPos = new Vector3();


    public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
    {
        HandleIfAllyNeedsNewNodeToPathfind();

        //get desired velocity to the point
        desiredVelocity = Vector3.Normalize(currentTargetPos - transform.position) * SteeringAgent.MaxCurrentSpeed;

        //calculate steering velocity
        steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;

        return steeringVelocity;
    }



    private void HandleIfAllyNeedsNewNodeToPathfind()
    {

        //if there is not already a current path and there are nodes to scout
        if (currentPath.Count == 0 && AllyManager.Instance.attackEnemies)
        {
            //get the closest node
            PathfindToNewNode();
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


    private void PathfindToNewNode()
    {
        //pathfind to node
        currentPath = Algorithms.AStar(GridData.Instance.GetNodeAt(transform.position), AllyManager.Instance.enemyPosition);

        //if we have a path
        if (currentPath != null && currentPath.Count > 1)
        {
            //remove start node
            currentPath.Remove(GridData.Instance.GetNodeAt(transform.position));

            //set current target 
            currentTargetPos = GenerateNewTargetPosWithOffset(currentPath[0]);
            currentPathIndex = 0;
        }
    }

    private Vector3 GenerateNewTargetPosWithOffset(Node node)
    {
        Vector3 newTargetPos = new Vector3(node.position.x, node.position.y, 0f);
        newTargetPos.x += UnityEngine.Random.Range(0f, 1f);
        newTargetPos.y += UnityEngine.Random.Range(0f, 1f);
        return newTargetPos;
    }
}
