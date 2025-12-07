using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class Idle : SteeringBehaviour
{
  
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

        Vector3 targetOffset = transform.position - transform.position;

        desiredVelocity = targetOffset;

        //calculate steering velocity
        steeringVelocity = desiredVelocity - steeringAgent.CurrentVelocity;

        return steeringVelocity;
    }
}
