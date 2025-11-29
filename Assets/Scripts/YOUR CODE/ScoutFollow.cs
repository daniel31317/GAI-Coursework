using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Analytics.IAnalytic;
using static UnityEngine.GraphicsBuffer;

public class ScoutFollow : SteeringBehaviour
{
    private Vector3 currentTargetPos = new Vector3();
    private List<Node> currentPath = new List<Node>();
    private int currentPathIndex = 0;
    public bool catchingUp { get; private set; } = false;




    public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
    {
        if(catchingUp)
        {
            HandleCatchingUpToScoutLeaderPathfinding();
        }

        desiredVelocity = Vector3.Normalize(currentTargetPos - transform.position) * SteeringAgent.MaxCurrentSpeed;

        //calculate steering velocity
        steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;

        return steeringVelocity;
    }




    public void SetTargetPos(Vector3 targetPos)
    { 
        currentTargetPos = targetPos;
    }


    //pathfind to leader scout if this scout has fallen too far behind
    public void CatchUpToScout(ScoutLeader leader)
    {
        catchingUp = true;
        currentPathIndex = 0;
        currentPath = Algorithms.AStar(GridData.Instance.GetNodeAt(transform.position), GridData.Instance.GetNodeAt(leader.transform.position));
        currentPath.Remove(GridData.Instance.GetNodeAt(transform.position));
        currentTargetPos = Algorithms.GenerateNewTargetPosWithOffset(currentPath[0]);
    }



    //handles setting the current target position on the path for the scout follower
    private void HandleCatchingUpToScoutLeaderPathfinding()
    {
        if (currentPathIndex < currentPath.Count - 1)
        {
            float distanceToCurrentNode = Vector3.SqrMagnitude(transform.position - currentTargetPos);

            if (distanceToCurrentNode < 1f)
            {
                //get new node
                currentPathIndex++;
                currentTargetPos = Algorithms.GenerateNewTargetPosWithOffset(currentPath[currentPathIndex]);

                //reached final node
                if (currentPathIndex == currentPath.Count - 1)
                {
                    catchingUp = false;
                }
            }
        }
    }


}
