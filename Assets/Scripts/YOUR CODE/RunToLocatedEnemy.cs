using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class RunToLocatedEnemy : SteeringBehaviour
{
    private int currentPathIndex = 0;
    private Vector3 currentTargetPos = new Vector3();
    private List<Node> currentPath = new List<Node>();

    public bool atShootPosition = false;

    public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
    {
        HandleAIPathfinding();

        Vector3 targetOffset = currentTargetPos - transform.position;

        //use of arrival - https://www.red3d.com/cwr/steer/gdc99/#:~:text=Arrival%20behavior%20is%20identical%20to,as%20shown%20in%20Figure%206.

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

        //calculate steering velocity
        steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;

        return steeringVelocity;
    }



    private void HandleAIPathfinding()
    {

        Vector3 currentEnemyPosition = Algorithms.GetClosestEnemyInLos(transform.position);
        if (Vector3.SqrMagnitude(transform.position - currentEnemyPosition) < Attack.AllyGunData.range * Attack.AllyGunData.range)
        {
            atShootPosition = true;
            currentTargetPos = currentEnemyPosition;
            return;
        }


        if (currentPath == null)
        {
            return;
        }
        
        if (currentPathIndex < currentPath.Count - 1)
        {
            float distanceToCurrentNode = Vector3.SqrMagnitude(transform.position - currentTargetPos);

            if (distanceToCurrentNode < 1f)
            {
                //get new node
                currentPathIndex++;
                if(currentPathIndex == currentPath.Count - 1)
                {
                    currentTargetPos = (Vector3)currentPath[currentPathIndex].position + new Vector3(0.5f, 0.5f,0f);
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
