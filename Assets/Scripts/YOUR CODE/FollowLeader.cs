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
 

    private List<Node> currentPath = new List<Node>();
    private int currentPathIndex = 0;
    public bool catchingUp { get; private set; } = false;
    public bool shouldAvoidAllies { get; private set; } = false;
    
    private Vector3 dodgeRocketForce = new Vector3();

    public bool atShootPosition = false;
    public bool canShoot { get; private set; } = false;
    private const float atShootPositionDelay = 0.5f;
    private float atShootPositionDelta = 0;


    private AllyAgent leader;

    private AllyAgent allyAgent;

    public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
    {
        DecideToCatchUp();
        
        if (catchingUp)
        {
            HandleCatchingUpToScoutLeaderPathfinding();
        }
        else
        {
            HandleAttackingEnemy();

            CalculateDesiredVelocity();
        }


        bool dodgeRocket = ShouldWeDodgeRocket();

        if(dodgeRocket)
        {
            desiredVelocity = dodgeRocketForce;
            dodgeRocketForce = Vector3.zero;
        }

        desiredVelocity.Normalize();
        desiredVelocity *= SteeringAgent.MaxCurrentSpeed;


        HandleSlowingDownToShoot(dodgeRocket);

        //calculate steering velocity
        steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;
        return steeringVelocity;
    }


    //deciees if the scout follower needs to catch up to the scout leader
    private void DecideToCatchUp()
    {
        float distanceSqr = Vector3.SqrMagnitude(transform.position - leader.transform.position);
        bool isInLos = Algorithms.IsPositionInLineOfSight(transform.position, leader.transform.position);

        if (!catchingUp)
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


    //finds if there is an enemy to attack then selects the appropriate attack type
    private void HandleAttackingEnemy()
    {
        EnemyAgent currentEnemyPosition = Algorithms.GetClosestEnemyInLos(transform.position);
        if (currentEnemyPosition != null)
        {
            float distance = Vector3.SqrMagnitude(transform.position - currentEnemyPosition.transform.position);

            if (distance <= Attack.MeleeData.range * Attack.MeleeData.range)
            {
                atShootPosition = true;
                currentTargetPos = currentEnemyPosition.transform.position;
                allyAgent.SetAttackType(Attack.AttackType.Melee);
                return;
            }
            else if (distance <= Attack.AllyGunData.range * Attack.AllyGunData.range)
            {
                atShootPosition = true;
                currentTargetPos = currentEnemyPosition.transform.position;
                allyAgent.SetAttackType(Attack.AttackType.AllyGun);
                return;
            }
        }

        //if no enemy to attack we are not at shoot position
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

        desiredVelocity = currentTargetPos - transform.position;
    }


    //calculate the desired velocity taking into acoont seperation and obstacle avoidance
    private void CalculateDesiredVelocity()
    {

        if(atShootPosition)
        {
            desiredVelocity = currentTargetPos - transform.position;
            return;
        }


        // use of arrival https://www.red3d.com/cwr/papers/1999/gdc99steer.pdf
        //this slows the agent as it gets closer to the target position by clipping the speed based on distance
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

        //add obstacle avoidance and seperation forces if we should do that
        if(shouldAvoidAllies)
        {
            desiredVelocity += (Vector3)Algorithms.CalcualteSeperationForce(gameObject);
        }
        desiredVelocity += (Vector3)Algorithms.CalcualteObstacleAvoidanceForce(transform.position);
    }


    //if the dodge rocket force is not zero we need to dodge the rocket
    private bool ShouldWeDodgeRocket()
    {
        if(dodgeRocketForce == Vector3.zero)
        {
            return false;
        }
        return true;
    }


    //if at shoot position slow down to be able to shoot
    private void HandleSlowingDownToShoot(bool dodgeRocket)
    {
        //divide by 10 so the allys slow down but look the right way
        if (atShootPosition && !dodgeRocket)
        {
            atShootPositionDelta += Time.deltaTime;
            if (atShootPositionDelta >= atShootPositionDelay)
            {
                canShoot = true;
            }
            desiredVelocity /= 10f;
        }
        else
        {
            atShootPositionDelta = 0;
            canShoot = false;
        }
    }



    public void SetLeader(AllyAgent leader)
    {
        this.leader = leader;
    }


    public void AddDodgeRocketForce(Vector3 force)
    {
        dodgeRocketForce = -force;
    }

    public void SetAllyAgent(AllyAgent agent)
    {
        allyAgent = agent;
    }


    public void SetAvoidAllies(bool avoid)
    {
        shouldAvoidAllies = avoid;
    }

}
