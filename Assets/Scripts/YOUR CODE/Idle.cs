using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
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
        //check for if there are any enemies in los
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

        Vector3 targetPosition = HandleidleTurning();

        

        desiredVelocity = Vector3.Normalize(targetPosition - transform.position) * SteeringAgent.MaxCurrentSpeed;

        desiredVelocity /= 1000f;

        //calculate steering velocity
        steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;

        return steeringVelocity;
    }



    private Vector3 HandleidleTurning()
    {
        if (!turning)
        {
            pointAhead = transform.position + transform.up;

            //get random left or right direction to trun to
            turningRight = UnityEngine.Random.Range(0, 2) == 0 ? true : false;

            pointToSide = transform.position;

            pointToSide += turningRight ? (Vector2)transform.right : -(Vector2)transform.right;

            //get the difference in x between the two points
            xStep = pointAhead.x - pointToSide.x;

            gradient = (pointAhead.y - pointToSide.y) / xStep;

            //calculate the y-intercept of the line 
            c = pointAhead.y - (gradient * pointAhead.x);

            //divde x step by 1000 to make the turning more gradual
            xStep /= 1000;

            //make sure its positve
            Mathf.Abs(xStep);

            currentX = 0;

            turning = true;

            return pointAhead;
        }
        else
        {
            Vector3 targetPosition = Vector3.zero;

            //randomly stop truning
            if (UnityEngine.Random.Range(0f, 1000f) < 1f)
            {
                turning = false;
            }

            //increment current x position along the line
            currentX += xStep * turnSpeed;

            //target x position on the line
            targetPosition.x = pointAhead.x - currentX;

            //get relative y pos using y = mx + c where m is gradient and c is y-intercept
            targetPosition.y = gradient * targetPosition.x + c;
            return targetPosition;
        }
    }
}