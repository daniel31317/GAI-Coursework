using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [SerializeField] public List<EnemyGroup> Groups = new List<EnemyGroup>();

    private const float DistanceToBeConideredInGroup = 8f;
   
    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }       
    }


    public void Start()
    {
        SortEnemiesIntoGroups();
    }



    public void SortEnemiesIntoGroups()
    {
        List<SteeringAgent> enemiesToGroup = GameData.Instance.enemies;


        while (enemiesToGroup.Count > 0)
        {
            EnemyGroup currentGroup = new EnemyGroup(true);
            currentGroup.AddEnemyToGroup(enemiesToGroup[0]);
            enemiesToGroup.RemoveAt(0);

            for (int j = 0; j < currentGroup.Enemies.Count; j++)
            {
                for (int i = 0; i < enemiesToGroup.Count;)
                {
                    if (Vector3.SqrMagnitude(enemiesToGroup[i].transform.position - currentGroup.Enemies[j].transform.position) <= DistanceToBeConideredInGroup * DistanceToBeConideredInGroup)
                    {
                        currentGroup.AddEnemyToGroup(enemiesToGroup[i]);
                        enemiesToGroup.Remove(enemiesToGroup[i]);
                    }
                    else
                    {
                        i++;
                    }
                }
                
            }

            Groups.Add(currentGroup);
        }
    }



    public EnemyGroup GetGroupIncludingThisEnemy(EnemyAgent enemy)
    {
        for (int i = 0; i < Groups.Count; i++)
        {
            for (int j = 0; j < Groups[i].Enemies.Count; j++)
            {
                if(Groups[i].Enemies[j] == enemy)
                {
                    return Groups[i];
                }
            }
        }

        return new EnemyGroup(false);
    }





}

[System.Serializable]
public struct EnemyGroup
{
    public List<SteeringAgent> Enemies;// { get; private set; }
    public bool isRealGroup;

    public EnemyGroup(bool isReal)
    {
        Enemies = new List<SteeringAgent>();    
        isRealGroup = isReal;

    }

    public void AddEnemyToGroup(SteeringAgent enemyAgent)
    {
        if(Enemies == null)
        {
            isRealGroup = true;
            Enemies = new List<SteeringAgent>();
        }
        Enemies.Add(enemyAgent);
    }
    public void RemoveEnemyFromGroup(SteeringAgent enemyAgent)
    {
        Enemies.Remove(enemyAgent);
    }

}