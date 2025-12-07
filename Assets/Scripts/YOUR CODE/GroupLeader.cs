using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class GroupLeader : SteeringBehaviour
{
    private int currentPathIndex = 0;
    private Vector3 currentTargetPos = new Vector3();
    private List<Node> currentPath = new List<Node>();

    public bool atShootPosition = false;

    public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
    {
        HandleAIPathfinding();

        Vector3 targetOffset = currentTargetPos - transform.position;

        //use of arrival - https://www.red3d.com/cwr/papers/1999/gdc99steer.pdf

        float distance = targetOffset.magnitude;

        float rampedSpeed = SteeringAgent.MaxCurrentSpeed * (distance / 1);

        float clippedSpeed = Mathf.Min(rampedSpeed, SteeringAgent.MaxCurrentSpeed);

        //get desired velocity to the point
        desiredVelocity = (clippedSpeed / distance) * targetOffset;

        //divide by big number so they allys dont move but look the right way
        if(atShootPosition)
        {
            desiredVelocity /= 10f;
        }
        else
        {
            Vector2 avoid = Algorithms.CalcualteObstacleAvoidanceForce(transform.position);
            desiredVelocity += (Vector3)avoid;
        }

        //calculate steering velocity
        steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;

        return steeringVelocity;
    }



    private void HandleAIPathfinding()
    {

        EnemyAgent currentEnemyPosition = Algorithms.GetClosestEnemyInLos(transform.position);
        if (currentEnemyPosition != null)
        {
            if (Vector3.SqrMagnitude(transform.position - currentEnemyPosition.transform.position) < Attack.AllyGunData.range * Attack.AllyGunData.range)
            {
                atShootPosition = true;
                currentTargetPos = currentEnemyPosition.transform.position;
                return;
            }
            else
            {
                atShootPosition = false;
            }
        }

        


        if (currentPath == null)
        {
            return;
        }
        else if (currentPath.Count == 0 && !atShootPosition)
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
        currentPath = path;
        currentTargetPos = currentPath[0].position;
        currentPathIndex = 0;
        atShootPosition = false;
    }

}
