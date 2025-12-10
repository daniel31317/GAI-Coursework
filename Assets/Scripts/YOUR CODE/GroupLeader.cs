using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;


public class GroupLeader : SteeringBehaviour
{
    private int currentPathIndex = 0;
    private Vector3 currentTargetPos = new Vector3();
    private Vector3 dodgeRocketForce = new Vector3();
    private List<Node> currentPath = new List<Node>();

    public bool atShootPosition = false;
    public bool lastAtShootPosition = false;
    public bool canShoot { get; private set; } = false;
    private const float atShootPositionDelay = 0.5f;
    private float atShootPositionDelta = 0;

    public bool shootRocket = false;
    private const float shootRocketDelay = 7.5f;
    private float shootRocketDelta = 0f;

    private AllyAgent allyAgent;


    public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
    {
        if(!FoundEnemyToAttack())
        {
            HandleAIPathfinding();
        }
        

        CalculateDesiredVelocity();


        bool dodgeRocket = ShouldWeDodgeRocket();

        if (dodgeRocket)
        {
            desiredVelocity = dodgeRocketForce;
            dodgeRocketForce = Vector3.zero;
        }

        desiredVelocity.Normalize();
        desiredVelocity *= SteeringAgent.MaxCurrentSpeed;


        HandleIfCanShootRocket();

        HandleSlowingDownToShoot(dodgeRocket);

        //calculate steering velocity
        steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;

        return steeringVelocity;
    }



    //checks for any enemy in line of sight then selects the right weapon based on distance 
    private bool FoundEnemyToAttack()
    {
        EnemyAgent currentEnemyPosition = Algorithms.GetClosestEnemyInLos(transform.position);
        if (currentEnemyPosition != null)
        {
            //distance to that enemy
            float distance = Vector3.SqrMagnitude(transform.position - currentEnemyPosition.transform.position);

            //check distance for various weapons
            if (distance <= Attack.MeleeData.range * Attack.MeleeData.range)
            {
                atShootPosition = true;
                lastAtShootPosition = true;
                currentTargetPos = currentEnemyPosition.transform.position;
                allyAgent.SetAttackType(Attack.AttackType.Melee);
                return true;
            }
            else if (distance <= Attack.AllyGunData.range * Attack.AllyGunData.range)
            {
                atShootPosition = true;
                lastAtShootPosition = true;
                currentTargetPos = currentEnemyPosition.transform.position;
                allyAgent.SetAttackType(Attack.AttackType.AllyGun);
                return true;
            }
            else if (shootRocket && distance <= Attack.RocketData.range * Attack.RocketData.range && GameData.Instance.AllyRocketsAvailable > 0)
            {
                atShootPosition = true;
                lastAtShootPosition = true;
                currentTargetPos = currentEnemyPosition.transform.position;
                return true;
            }
            //if the enemy is not in range and we were at our shootposition then we pathfind to that new found enemy
            else if (atShootPosition)
            {
                Node enemyNode = GridData.Instance.GetNodeAt(currentEnemyPosition.transform.position);
                currentPath = Algorithms.AStar(GridData.Instance.GetNodeAt(transform.position), enemyNode);
                currentPathIndex = 0;
                atShootPosition = false;
                currentTargetPos = Algorithms.GenerateNewTargetPosWithOffset(currentPath[currentPathIndex]);
            }
        }

        return false;
    }




    //handles ai pathfinding
    private void HandleAIPathfinding()
    {
        //if we are this point then we are not at the shootposition
        atShootPosition = false;

        //if any of these conditions are met a group leader is not needed so re-assign roles 
        if (currentPath == null || currentPath.Count == 0 || currentPathIndex == currentPath.Count - 1 || atShootPosition != lastAtShootPosition)
        {
            AllyManager.Instance.AssignRoles();
        }

        //if we are on the path
        if (currentPathIndex < currentPath.Count - 1)
        {
            float distanceToCurrentNode = Vector3.SqrMagnitude(transform.position - currentTargetPos);

            //if we are at that node then
            if (distanceToCurrentNode < 1f)
            {
                //get new node
                currentPathIndex++;
                if (currentPathIndex == currentPath.Count - 1)
                {
                    currentTargetPos = (Vector3)currentPath[currentPathIndex].position + new Vector3(0.5f, 0.5f, 0f);
                }
                else
                {
                    currentTargetPos = Algorithms.GenerateNewTargetPosWithOffset(currentPath[currentPathIndex]);
                }
            }
        }
        else
        {
            // if we are not on the path anymore and we are inside the shootpositon then set it to true
            float distanceToCurrentNode = Vector3.SqrMagnitude(transform.position - currentTargetPos);

            if (distanceToCurrentNode < 0.001f)
            {
                atShootPosition = true;
            }
        }
    }


    //calcualtes the desired velocity using arrival
    private void CalculateDesiredVelocity()
    {
        Vector3 targetOffset = currentTargetPos - transform.position;

        //use of arrival - https://www.red3d.com/cwr/papers/1999/gdc99steer.pdf

        float distance = targetOffset.magnitude;

        float rampedSpeed = SteeringAgent.MaxCurrentSpeed * (distance / 1);

        float clippedSpeed = Mathf.Min(rampedSpeed, SteeringAgent.MaxCurrentSpeed);

        //get desired velocity to the point
        if (distance == 0)
        {
            desiredVelocity = Vector3.zero;
        }
        else
        {
            desiredVelocity = (clippedSpeed / distance) * targetOffset;
            desiredVelocity.Normalize();
            desiredVelocity *= SteeringAgent.MaxCurrentSpeed;
        }
    }


    //if the dodge rocket force is not zero we need to dodge the rocket
    private bool ShouldWeDodgeRocket()
    {
        if (dodgeRocketForce == Vector3.zero)
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

    public void SetCurrentPath(List<Node> path)
    {
        //quick check to make sure its a valid path
        if (path == null || path.Count == 0)
        {
            return;
        }
        currentPath = path;      
        currentTargetPos = currentPath[0].position;
        currentPathIndex = 0;
        atShootPosition = false;
        canShoot = false;
        allyAgent.SetAttackType(Attack.AttackType.Rocket);
    }

    public void AddDodgeRocketForce(Vector3 force)
    {
        dodgeRocketForce = -force;
    }


    public void SetAllyAgent(AllyAgent agent)
    {
        allyAgent = agent;
    }


    private void HandleIfCanShootRocket()
    {
        if(shootRocket)
        {
            return;
        }

        shootRocketDelta += Time.deltaTime;
        if (shootRocketDelta >= shootRocketDelay)
        {
            shootRocket = true;
            shootRocketDelta = 0f;
        }
    }
}
