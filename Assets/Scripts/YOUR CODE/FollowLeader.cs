using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UIElements;
using static UnityEngine.Analytics.IAnalytic;
using static UnityEngine.GraphicsBuffer;

public class FollowLeader : SteeringBehaviour
{
    private Vector3 currentTargetPos = new Vector3();
    private Vector3 dodgeRocketForce = new Vector3();
    private List<Node> currentPath = new List<Node>();
    private int currentPathIndex = 0;
    public bool catchingUp { get; private set; } = false;
    public bool atShootPosition = false;
    private AllyAgent leader;



    public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
    {
        Profiler.BeginSample("ScoutFollow UpdateBehaviour");

        float distanceSqr = Vector3.SqrMagnitude(transform.position - leader.transform.position);
        bool isInLos = Algorithms.IsPositionInLineOfSight(transform.position, leader.transform.position);


        if(!catchingUp)
        {
            if (distanceSqr >= 100 && !isInLos)
            {
                CatchUpToLeader();
            }
            else if (distanceSqr >= 225)
            {
                CatchUpToLeader();
            }
        }
        

        if (catchingUp)
        {
            HandleCatchingUpToScoutLeaderPathfinding();
            desiredVelocity = currentTargetPos - transform.position;
        }
        else
        {
            HandleAttackingEnemy();

            if(!atShootPosition)
            {
                // use of arrival https://www.red3d.com/cwr/papers/1999/gdc99steer.pdf
                currentTargetPos = leader.transform.position - (leader.transform.up * 2);

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
                desiredVelocity += (Vector3)Algorithms.CalcualteSeperationForce(gameObject);

                

            }
            else
            {
                desiredVelocity = currentTargetPos - transform.position;
            }
            
        }

        if (dodgeRocketForce != Vector3.zero)
        {
            desiredVelocity += dodgeRocketForce * SteeringAgent.MaxCurrentSpeed;
            dodgeRocketForce = Vector3.zero;
        }

        desiredVelocity.Normalize();
        desiredVelocity *= SteeringAgent.MaxCurrentSpeed;

        if(atShootPosition)
        {
            desiredVelocity /= 10f;
        }

        //calculate steering velocity
        steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;
        Profiler.EndSample();
        return steeringVelocity;
    }



    //pathfind to leader scout if this scout has fallen too far behind
    public void CatchUpToLeader()
    {
        currentPath = Algorithms.AStar(GridData.Instance.GetNodeAt(transform.position), GridData.Instance.GetNodeAt(leader.transform.position));
        catchingUp = true;
        currentPathIndex = 0;
        currentPath.Remove(GridData.Instance.GetNodeAt(transform.position));
        currentTargetPos = Algorithms.GenerateNewTargetPosWithOffset(currentPath[0]);
    }



    private void HandleAttackingEnemy()
    {
        EnemyAgent currentEnemyPosition = Algorithms.GetClosestEnemyInLos(transform.position);
        if (currentEnemyPosition != null)
        {
            if (Vector3.SqrMagnitude(transform.position - currentEnemyPosition.transform.position) <= Attack.AllyGunData.range * Attack.AllyGunData.range)
            {
                currentTargetPos = currentEnemyPosition.transform.position;
                atShootPosition = true;
                return;
            }
        }

        atShootPosition = false;
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



    public void SetLeader(AllyAgent leader)
    {
        this.leader = leader;
    }


    public void AdddodgeRocketForce(Vector3 force)
    {
        dodgeRocketForce += force;
    }

}
