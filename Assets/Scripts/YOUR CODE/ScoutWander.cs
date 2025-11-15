using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoutWander : SteeringBehaviour
{
    private List<Node> currentPath = new List<Node>();
    private Vector3 currentTargetPos = new Vector2();
    private Node currentClosestNode;

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
        if (currentPath.Count == 0 && RoleManager.ScoutManager.nodesToScout.Count > 0)
        {
            //get the closest node
            currentClosestNode = RoleManager.ScoutManager.GetClosestScoutNode(transform.position);
            PathfindToNewNode();
            return;
        }
        else
        {
            IsThereACloserNode();
        }
    
        if(currentPath.Count > 0)
        {
            float distanceToCurrentNode = Vector3.SqrMagnitude(transform.position - currentTargetPos);

            if (distanceToCurrentNode < 1f)
            {
                //get new node
                currentTargetPos = GenerateNewTargetPosWithOffset(currentPath[0]);
                currentPath.RemoveAt(0);

                //reached final node
                if(currentPath.Count == 0)
                {
                    RoleManager.ScoutManager.RemoveScoutedNode(currentClosestNode);
                }
            }
        }
    }


    private void PathfindToNewNode()
    {
        //pathfind to node
        currentPath = PathfindingAlgorithms.AStar(GridData.Instance.GetNodeAt(transform.position), currentClosestNode);

        //if we have a path
        if (currentPath != null && currentPath.Count > 0)
        {
            //remove start node
            currentPath.Remove(GridData.Instance.GetNodeAt(transform.position));

            //set current target 
            currentTargetPos = GenerateNewTargetPosWithOffset(currentPath[0]);
            currentPath.RemoveAt(0);

        }
    }



    private void IsThereACloserNode()
    {
        Node closestNode = RoleManager.ScoutManager.GetClosestScoutNode(transform.position);
        if (currentClosestNode != closestNode)
        {
            currentClosestNode = closestNode;
            PathfindToNewNode();
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