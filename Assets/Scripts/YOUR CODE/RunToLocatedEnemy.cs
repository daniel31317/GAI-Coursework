using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class RunToLocatedEnemy : SteeringBehaviour
{
    private int currentPathIndex = 0;
    private Vector3 currentTargetPos = new Vector3();
    private List<Node> currentPath = new List<Node>();
    bool arrivedToShootPlace = false;

    public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
    {
        HandleAIPathfinding();

        if(!arrivedToShootPlace)
        {
            //use of arrival - https://www.red3d.com/cwr/steer/gdc99/#:~:text=Arrival%20behavior%20is%20identical%20to,as%20shown%20in%20Figure%206.
            Vector3 targetOffset = currentTargetPos - transform.position;

            float distance = targetOffset.magnitude;

            float rampedSpeed = SteeringAgent.MaxCurrentSpeed * (distance / 1);

            float clippedSpeed = Mathf.Min(rampedSpeed, SteeringAgent.MaxCurrentSpeed);

            //get desired velocity to the point
            desiredVelocity = (clippedSpeed / distance) * targetOffset;
        }
        else
        {
            desiredVelocity = Vector3.zero;
        }


        //calculate steering velocity
        steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;

        return steeringVelocity;

    }



    private void HandleAIPathfinding()
    {
        if(currentPath == null)
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
                currentTargetPos = GenerateNewTargetPosWithOffset(currentPath[currentPathIndex]);
            }
        }
        else
        {
            currentTargetPos = AllyManager.Instance.enemyPosition.position;
            arrivedToShootPlace = true;
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
        arrivedToShootPlace = false;
    }
}
