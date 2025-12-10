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
    public bool canShoot { get; private set; } = false;
    private const float atShootPositionDelay = 0.5f;
    private float atShootPositionDelta = 0;

    public bool shootRocket = false;

    private AllyAgent allyAgent;


    public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
    {
        Profiler.BeginSample("GroupLeader UpdateBehaviour");
        HandleAIPathfinding();

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


        bool dodgeRocket = false;

        if (dodgeRocketForce != Vector3.zero)
        {
            dodgeRocket = true;
            desiredVelocity = dodgeRocketForce;
            dodgeRocketForce = Vector3.zero;
        }

        desiredVelocity.Normalize();
        desiredVelocity *= SteeringAgent.MaxCurrentSpeed;

        //divide by big number so they allys dont move but look the right way
        if (atShootPosition && !dodgeRocket)
        {
            atShootPositionDelta += Time.deltaTime;
            if(atShootPositionDelta >= atShootPositionDelay)
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

            //calculate steering velocity
            steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;

        Profiler.EndSample();
        return steeringVelocity;
    }



    private void HandleAIPathfinding()
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
            else if (shootRocket && distance <= Attack.RocketData.range * Attack.RocketData.range)
            {
                atShootPosition = true;
                currentTargetPos = currentEnemyPosition.transform.position;
                return;
            }
            else if (currentPath != null && atShootPosition)
            {
                Node enemyNode = GridData.Instance.GetNodeAt(currentEnemyPosition.transform.position);
                currentPath = Algorithms.AStar(GridData.Instance.GetNodeAt(transform.position), enemyNode);
                currentPathIndex = 0;
                atShootPosition = false;
            }
        }
  
        atShootPosition = false;


        if (currentPath == null)
        {
            return;
        }
        else if ((currentPath.Count == 0 || currentPathIndex == currentPath.Count - 1) && !atShootPosition)
        {
            AllyManager.Instance.AssignRoles();
        }

        if (currentPathIndex < currentPath.Count - 1)
        {
            float distanceToCurrentNode = Vector3.SqrMagnitude(transform.position - currentTargetPos);

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
                    currentTargetPos = GenerateNewTargetPosWithOffset(currentPath[currentPathIndex]);
                }
            }
        }
        else
        {
            float distanceToCurrentNode = Vector3.SqrMagnitude(transform.position - currentTargetPos);

            if (distanceToCurrentNode < 0.001f)
            {
                atShootPosition = true;
            }
        }
    }


    private Vector3 GenerateNewTargetPosWithOffset(Node node)
    {
        Vector3 newTargetPos = new Vector3(node.position.x, node.position.y, 0f);
        newTargetPos.x += UnityEngine.Random.Range(0f, 1f);
        newTargetPos.y += UnityEngine.Random.Range(0f, 1f);
        return newTargetPos;
    }


    public void SetCurrentPath(List<Node> path)
    {
        if (path == null || path.Count == 0)
        {
            return;
        }
        currentPath = path;      
        currentTargetPos = currentPath[0].position;
        currentPathIndex = 0;
        atShootPosition = false;
        canShoot = false;
        shootRocket = true;
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
}
