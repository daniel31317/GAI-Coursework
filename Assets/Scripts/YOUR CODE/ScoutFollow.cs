using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;
using static UnityEngine.Analytics.IAnalytic;
using static UnityEngine.GraphicsBuffer;

public class ScoutFollow : SteeringBehaviour
{
    private Vector3 currentTargetPos = new Vector3();
    private List<Node> currentPath = new List<Node>();
    private int currentPathIndex = 0;
    public bool catchingUp { get; private set; } = false;
    private AllyAgent leaderScout;



    public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
    {
        Profiler.BeginSample("ScoutFollow UpdateBehaviour");


        if (catchingUp)
        {
            HandleCatchingUpToScoutLeaderPathfinding();
            desiredVelocity = currentTargetPos - transform.position;
        }
        else
        {
            currentTargetPos = leaderScout.transform.position - (leaderScout.transform.up * 2);

            Vector3 targetOffset = currentTargetPos - transform.position;

            float distance = targetOffset.magnitude;

            float rampedSpeed = SteeringAgent.MaxCurrentSpeed * (distance / 2f);

            float clippedSpeed = Mathf.Min(rampedSpeed, SteeringAgent.MaxCurrentSpeed);

            //get desired velocity to the point
            if (distance == 0)
            {
                desiredVelocity = Vector3.zero;
            }
            else
            {
                desiredVelocity = (clippedSpeed / distance) * targetOffset;
            }

            desiredVelocity += (Vector3)Algorithms.CalcualteObstacleAvoidanceForce(transform.position);
        }

           
        desiredVelocity = currentTargetPos - transform.position;        
        desiredVelocity.Normalize();

        desiredVelocity *= SteeringAgent.MaxCurrentSpeed;
        //calculate steering velocity
        steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;
        Profiler.EndSample();
        return steeringVelocity;
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
        if (currentPathIndex <= currentPath.Count)
        {
            float distanceToCurrentNode = Vector3.SqrMagnitude(transform.position - currentTargetPos);

            if (distanceToCurrentNode < 1f)
            {
                //get new node
                currentPathIndex++;
                if(currentPathIndex >= currentPath.Count)
                {
                    catchingUp = false;
                    return;
                }
                currentTargetPos = Algorithms.GenerateNewTargetPosWithOffset(currentPath[currentPathIndex]);

                //reached final node
                if (currentPathIndex == currentPath.Count - 1)
                {
                    catchingUp = false;
                }
            }
        }
    }



    public void SetLeader(AllyAgent leader)
    {
        leaderScout = leader;
    }

}
