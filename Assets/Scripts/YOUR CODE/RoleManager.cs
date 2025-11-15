using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class RoleManager : MonoBehaviour
{
    public static RoleManager Instance { get; private set; }
    public GameObject testBlock;
    public static ScoutManager ScoutManager { get; private set; }

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
        ScoutManager.testBlock = testBlock;   
    }

    private const int NUMBER_OF_SCOUTS = 1;


    public void AssignRoles()
    {
        ScoutManager.SetAgentAsScoutLead((AllyAgent)GameData.Instance.allies[0]);
        ScoutManager.SetAgentAsScoutFollower((AllyAgent)GameData.Instance.allies[1]);



        //now start looping i starting from currentRoleIndex
    }

}
