using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class RoleManager : MonoBehaviour
{
    public static RoleManager Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    private const int NUMBER_OF_SCOUTS = 1;

    private void Start()
    {
        int currentRoleIndex = 0;
        for (int i = 0; i < NUMBER_OF_SCOUTS; i++)
        {
            AllyAgent agent = ((AllyAgent)GameData.Instance.allies[i]);
            agent.SetAgentRole(AllyAgentRole.Scout);
            agent.AddComponent<Wander>();
            currentRoleIndex++;
        }

        //now start looping i starting from currentRoleIndex
    }



}
