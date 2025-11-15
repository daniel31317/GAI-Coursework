using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class AllyManager : MonoBehaviour
{
    public static AllyManager Instance { get; private set; }
    public GameObject scoutBlock;
    public GameObject enemyBlock;
    public static ScoutManager ScoutManager { get; private set; }

    public Vector3 currentBasePosition { get; private set; }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        ScoutManager = (ScoutManager)ScriptableObject.CreateInstance("ScoutManager");
        ScoutManager.InitialiseOnStart();
        ScoutManager.scoutBlock = scoutBlock;   
        ScoutManager.enemyBlock = enemyBlock;   
    }


    public void AssignRoles()
    {
        ScoutManager.SetAgentAsScoutLead((AllyAgent)GameData.Instance.allies[0]);
        ScoutManager.SetAgentAsScoutFollower((AllyAgent)GameData.Instance.allies[1]);

        Vector3 positionTotal = Vector3.zero;
        int amount = 0;
        for (int i = 2; i < GameData.Instance.allies.Count; i++)
        {
            ((AllyAgent)GameData.Instance.allies[i]).SetAgentRole(AllyAgentRole.Default);
            positionTotal += GameData.Instance.allies[i].transform.position;
            amount++;
        }


        currentBasePosition = positionTotal / amount;

        //now start looping i starting from currentRoleIndex
    }

}
