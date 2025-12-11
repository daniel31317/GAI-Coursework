using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class ScoutLeader : SteeringBehaviour
{
    private List<Node> currentPath = new List<Node>();
    private int currentPathIndex = 0;

    private Vector3 currentTargetPos = new Vector3();
    private bool returningToBase = false;

    private Node currentClosestNode;

    private EnemyAgent closestEnemy;

    private FollowLeader scoutFollower;

    private Vector3 lastPosition = Vector3.zero;

    public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
    {
        HandleIfAllyNeedsNewNodeToPathfind();

        //get desired velocity to the point
        desiredVelocity = Vector3.Normalize(currentTargetPos - transform.position) * SteeringAgent.MaxCurrentSpeed;      

        //if scout has fallen behind wait for it to catch up by it pathfinding
        if (scoutFollower.catchingUp)
        {
            desiredVelocity /= 10f;
        }

        if (lastPosition == transform.position)
        {
            desiredVelocity *= -1;
        }

        lastPosition = transform.position;





        //calculate steering velocity
        steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;

        return steeringVelocity;
    }

    private void HandleIfAllyNeedsNewNodeToPathfind()
    {

        //if we are not already returning to base
        if (!returningToBase)
        {
            //if there is an enemy in line of sight then we pathfind back to base
            closestEnemy = Algorithms.GetClosestEnemyInLos(transform.position);

            if (closestEnemy != null)
            {
                returningToBase = true;
                currentClosestNode = GridData.Instance.GetNodeAt(AllyManager.Instance.currentBasePosition);
                PathfindToNewNode();
                return;
            }
        }


        //if we dont have a path return unless we are meant to be returning in which we pathfind back to node
        //more of a safety check just to make sure there are no errors
        if(currentPath == null)
        {

            if(returningToBase)
            {
                currentClosestNode = GridData.Instance.GetNodeAt(AllyManager.Instance.currentBasePosition);
                PathfindToNewNode();
            }
            return;
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
            //check for a closer scout node and go to that now
            IsThereACloserNode();
        }

        //while on the path
        if (currentPathIndex <= currentPath.Count)
        {
           
            float distanceToCurrentNode = Vector3.SqrMagnitude(transform.position - currentTargetPos);


            //if within distance of current node skip to the next one and if at the end of the path then we remove that scouted node
            if (distanceToCurrentNode < 1f)
            {
                //get new node
                currentPathIndex++;
                if(currentPathIndex >= currentPath.Count)
                {
                    return;
                }
                currentTargetPos = Algorithms.GenerateNewTargetPosWithOffset(currentPath[currentPathIndex]);
               //reached final node
                if (currentPathIndex == currentPath.Count - 1 && !returningToBase)
                {
                    AllyManager.ScoutManager.RemoveScoutedNode(currentClosestNode);
                }
            }
        }
        //if we have reached then end of the path (first if statement ahs run out of path) and we were returning to base we must be at base so tell group to go attack
        else if (returningToBase)
        {
            AllyManager.Instance.FoundEnemyToAttack(closestEnemy);
        }
        else //if we arent returning we must ahve reached a scout node so remove it
        {
            AllyManager.ScoutManager.RemoveScoutedNode(currentClosestNode);
        }
    }


    private void PathfindToNewNode()
    {
        //get path to nexrt node
        currentPath = Algorithms.AStar(GridData.Instance.GetNodeAt(transform.position), currentClosestNode);
        //if we have a path
        if (currentPath != null && currentPath.Count > 1)
        {
            //remove start node
            currentPath.Remove(GridData.Instance.GetNodeAt(transform.position));

            //set current target 
            currentTargetPos = Algorithms.GenerateNewTargetPosWithOffset(currentPath[0]);
            currentPathIndex = 0;
        }
    }


    //checks for a closer node and if so pathfinds to it
    private void IsThereACloserNode()
    {
        Node closestNode = AllyManager.ScoutManager.GetClosestScoutNode(transform.position);
        if (currentClosestNode != closestNode)
        {
            currentClosestNode = closestNode;
            PathfindToNewNode();
        }
    }

    //resets the scout so they dont attempt to go abck to their old path
    public void ResetScout()
    {
        currentPath = new List<Node>();
        currentPathIndex = 0;

        currentTargetPos = new Vector3();

        returningToBase = false;

        currentClosestNode = null;

        closestEnemy = null;
    }


    public void SetScoutFollower(FollowLeader followLeader)
    {
        scoutFollower = followLeader;
    }

}