using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class Idle : SteeringBehaviour
{
  
    public override Vector3 UpdateBehaviour(SteeringAgent steeringAgent)
    {
        Vector3 targetOffset = transform.position - transform.position;

        desiredVelocity = targetOffset;

        //calculate steering velocity
        steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;

        return steeringVelocity;
    }
}
