using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class ScoutFollow : SteeringBehaviour
{
    private const float slowingDistance = 2f;
    private Vector3 currentTargetPos = new Vector3();

    public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
    {
        desiredVelocity = Vector3.Normalize(currentTargetPos - transform.position) * SteeringAgent.MaxCurrentSpeed;

        //calculate steering velocity
        steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;

        return steeringVelocity;
    }


    public void SetTargetPos(Vector3 targetPos)
    { 
        currentTargetPos = targetPos;
    }




}
