using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class GroupMember : SteeringBehaviour
{
    private Vector3 currentTargetPos = new Vector3();

    public bool atShootPosition = false;

    public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
    {
        HandleAIMovement();

        // use of arrival https://www.red3d.com/cwr/papers/1999/gdc99steer.pdf

        Vector3 targetOffset = (Vector2)currentTargetPos - (Vector2)transform.position;


        float distance = targetOffset.magnitude;

        float rampedSpeed = SteeringAgent.MaxCurrentSpeed * (distance / 2f);

        float clippedSpeed = Mathf.Min(rampedSpeed, SteeringAgent.MaxCurrentSpeed);

        //get desired velocity to the point
        desiredVelocity = (clippedSpeed / distance) * targetOffset;

        
        //divide by big number so they allys dont move but look the right way
        if (atShootPosition)
        {
            desiredVelocity /= 10f;
        }
        else
        {
            desiredVelocity += (Vector3)CalcualteSeperationForce();
            Vector2 avoid = Algorithms.CalcualteObstacleAvoidanceForce(transform.position);
            desiredVelocity += (Vector3)avoid;
        }


        //calculate steering velocity
        steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;

        return steeringVelocity;
    }


    private void HandleAIMovement()
    {

        EnemyAgent currentEnemyPosition = Algorithms.GetClosestEnemyInLos(transform.position);
        if (currentEnemyPosition != null)
        {
            if (Vector3.SqrMagnitude(transform.position - currentEnemyPosition.transform.position) < Attack.AllyGunData.range * Attack.AllyGunData.range)
            {
                currentTargetPos = currentEnemyPosition.transform.position;
                atShootPosition = true;
                return;
            }
            else
            {
                atShootPosition = false;
            }
        }

        Transform leaderTransform = AllyManager.Instance.groupLeader.transform;

        currentTargetPos = leaderTransform.position - (-leaderTransform.forward * 2);

    }


    



    private Vector2 CalcualteSeperationForce()
    {
        Vector2 totalForce = Vector2.zero;
        int amountOfAlliesNearby = 0;

        for (int i = 0; i < AllyManager.Instance.m_agents.Count; i++)
        {
            if (AllyManager.Instance.m_agents[i].gameObject != gameObject)
            {
                float distSqr = Vector2.SqrMagnitude(transform.position - AllyManager.Instance.m_agents[i].transform.position);

                if(distSqr <= 4 && distSqr > 0)
                {
                    Vector2 pushForce = transform.position - AllyManager.Instance.m_agents[i].transform.position;

                    totalForce += pushForce;
                    amountOfAlliesNearby++;
                }

            }
        }

        //so we dont get divide by zero errors on line after if statement
        if(amountOfAlliesNearby == 0)
        {
            return totalForce;
        }

        totalForce /= amountOfAlliesNearby;

        return totalForce * SteeringAgent.MaxCurrentSpeed;
    }




}
