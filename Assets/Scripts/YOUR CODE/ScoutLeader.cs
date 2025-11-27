using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoutLeader : SteeringBehaviour
{
    private List<Node> currentPath = new List<Node>();
    private int currentPathIndex = 0;

    private Vector3 currentTargetPos = new Vector3();
    private List<Vector3> previousTargetPositions = new List<Vector3>();

    private bool returningToBase = false;
    private bool scoutCatchingUp = false;

    private Node currentClosestNode;
    private AllyAgent followerScout;

    private EnemyAgent closestEnemy;

    public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
    {
        HandleIfAllyNeedsNewNodeToPathfind();

        //get desired velocity to the point
        desiredVelocity = Vector3.Normalize(currentTargetPos - transform.position) * SteeringAgent.MaxCurrentSpeed;

        scoutCatchingUp = Vector3.SqrMagnitude(transform.position - followerScout.transform.position) >= (AllyManager.viewDistance / 2) * (AllyManager.viewDistance / 2);

        //if scout has fallen behind wait for it to catch up by it pathfinding
        if (scoutCatchingUp)
        {
            desiredVelocity /= 10000f;
            ScoutFollow scoutFollowScript = followerScout.gameObject.GetComponent<ScoutFollow>();
            if (scoutFollowScript != null && !scoutFollowScript.catchingUp)
            {
                followerScout.gameObject.GetComponent<ScoutFollow>().CatchUpToScout(this);
            }
        }

        //calculate steering velocity
        steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;

        return steeringVelocity;
    }

    private void HandleIfAllyNeedsNewNodeToPathfind()
    {
        if (!returningToBase)
        {
            closestEnemy = Algorithms.GetClosestEnemyInLos(transform.position);

            if (closestEnemy != null)
            {
                returningToBase = true;
                currentClosestNode = GridData.Instance.GetNodeAt(AllyManager.Instance.currentBasePosition);
                PathfindToNewNode();
                return;
            }
        }

        //if there is not already a current path and there are nodes to scout
        if (currentPath.Count == 0 && AllyManager.ScoutManager.nodesToScout.Count > 0)
        {
            //get the closest node
            currentClosestNode = AllyManager.ScoutManager.GetClosestScoutNode(transform.position);
            PathfindToNewNode();
            return;
        }
        else if (!returningToBase)
        {
            IsThereACloserNode();
        }

        if (currentPathIndex < currentPath.Count - 1)
        {
            float distanceToCurrentNode = Vector3.SqrMagnitude(transform.position - currentTargetPos);

            if (distanceToCurrentNode < 1f)
            {
                //get new node
                currentPathIndex++;
                currentTargetPos = Algorithms.GenerateNewTargetPosWithOffset(currentPath[currentPathIndex]);
                previousTargetPositions.Add(currentTargetPos);
                if (currentPathIndex >= 2)
                {
                    followerScout.GetComponent<ScoutFollow>().SetTargetPos(previousTargetPositions[currentPathIndex - 2]);
                }


                //reached final node
                if (currentPathIndex == currentPath.Count - 1 && !returningToBase)
                {
                    AllyManager.ScoutManager.RemoveScoutedNode(currentClosestNode);
                }
            }
        }
        else if (returningToBase)
        {
            AllyManager.Instance.FoundEnemyToAttack(closestEnemy);
        }
    }


    private void PathfindToNewNode()
    {

        currentPath = Algorithms.AStar(GridData.Instance.GetNodeAt(transform.position), currentClosestNode);
        //if we have a path
        if (currentPath != null && currentPath.Count > 1)
        {
            //remove start node
            currentPath.Remove(GridData.Instance.GetNodeAt(transform.position));

            //set current target 
            currentTargetPos = Algorithms.GenerateNewTargetPosWithOffset(currentPath[0]);
            previousTargetPositions.Clear();
            previousTargetPositions.Add(currentTargetPos);
            currentPathIndex = 0;
        }
    }



    private void IsThereACloserNode()
    {
        Node closestNode = AllyManager.ScoutManager.GetClosestScoutNode(transform.position);
        if (currentClosestNode != closestNode)
        {
            currentClosestNode = closestNode;
            PathfindToNewNode();
        }
    }




    public void SetFollowerScout(AllyAgent lead)
    {
        followerScout = lead;
    }
}