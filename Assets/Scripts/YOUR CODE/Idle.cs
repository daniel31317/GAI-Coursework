using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class Idle : SteeringBehaviour
{
    private bool turning = false;
    private bool turningRight = false;

    private Vector2 pointAhead = new Vector2(); 
    private Vector2 pointToSide = new Vector2(); 
    private float gradient = 0f;
    private float c = 0f;
    private float currentX = 1f;
    private float xStep = 0f;
    private float turnSpeed = 10f;


    public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
    {

        for (int j = 0; j < GameData.Instance.enemies.Count; j++)
        {
            if (!GameData.Instance.enemies[j].gameObject.activeSelf)
            {
                continue;
            }
            if (Algorithms.IsPositionInLineOfSight((Vector2)transform.position, (Vector2)GameData.Instance.enemies[j].transform.position))
            {
                AllyManager.Instance.FoundEnemyToAttack((EnemyAgent)GameData.Instance.enemies[j]);
            }
        }

        Vector3 targetPosition = Vector3.zero;

        if (!turning)
        {
            pointAhead = transform.position + transform.up;

            turningRight = UnityEngine.Random.Range(0, 2) == 0 ? true : false;

            pointToSide = transform.position;

            pointToSide += turningRight ? (Vector2)transform.right : -(Vector2)transform.right;

            xStep = pointAhead.x - pointToSide.x;

            gradient = (pointAhead.y - pointToSide.y) / xStep;

            c = pointAhead.y + (gradient * -pointAhead.x);

            xStep /= 1000;

            Mathf.Abs(xStep);

            currentX = 0;

            turning = true;

            targetPosition = pointAhead;
        }
        else
        {

            if(UnityEngine.Random.Range(0f, 1000f) < 1f)
            {
                turning = false;
            }

            currentX += xStep * turnSpeed;

            targetPosition.x = pointAhead.x - currentX;


            targetPosition.y = gradient * targetPosition.x + c;

        }

        desiredVelocity = Vector3.Normalize(targetPosition - transform.position) * SteeringAgent.MaxCurrentSpeed;

        desiredVelocity /= 1000f;

        //calculate steering velocity
        steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;

        return steeringVelocity;
    }
}
