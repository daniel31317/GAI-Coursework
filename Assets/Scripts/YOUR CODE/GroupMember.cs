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

        Vector3 targetOffset = currentTargetPos - transform.position;

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
        }

        Transform leaderTransform = AllyManager.Instance.groupLeader.transform;

        currentTargetPos = leaderTransform.position - (-leaderTransform.forward * 2);

    }

}
