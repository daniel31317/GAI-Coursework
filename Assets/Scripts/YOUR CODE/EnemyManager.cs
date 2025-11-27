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
            EnemyGroup currentGroup = new EnemyGroup();



            currentGroup.AddEnemyToGroup(enemiesToGroup[0]);
            enemiesToGroup.RemoveAt(0);

            for (int j = 0; j < currentGroup.Enemies.Count; j++)
            {
                for (int i = 0; i < enemiesToGroup.Count;)
                {
                    if (Vector3.SqrMagnitude(enemiesToGroup[i].transform.position - currentGroup.Enemies[j].transform.position) <= DistanceToBeConideredInGroup * DistanceToBeConideredInGroup)
                    {
                        currentGroup.AddEnemyToGroup(enemiesToGroup[i]);
                        currentGroup.BasePosition += enemiesToGroup[i].transform.position;
                        enemiesToGroup.Remove(enemiesToGroup[i]);
                    }
                    else
                    {
                        i++;
                    }
                }
                
            }

            currentGroup.BasePosition /= currentGroup.Enemies.Count;

            Groups.Add(currentGroup);
        }
    }







}

[System.Serializable]
public struct EnemyGroup
{
    public List<SteeringAgent> Enemies;// { get; private set; }
    public Vector3 BasePosition;// { get; private set; }
    public Node BaseNode;// { get; private set; }

    public void AddEnemyToGroup(SteeringAgent enemyAgent)
    {
        if(Enemies == null)
        {
            Enemies = new List<SteeringAgent>();
        }
        Enemies.Add(enemyAgent);
    }
    public void RemoveEnemyFromGroup(SteeringAgent enemyAgent)
    {
        Enemies.Remove(enemyAgent);
    }

    public void SetBasePosition(Vector2 pos)
    {
        BasePosition = pos;
    }

}