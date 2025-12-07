using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Profiling;

public class AllyManager : MonoBehaviour
{
    public static AllyManager Instance { get; private set; }

    public List<AllyAgent> m_agents { get; private set; } = new List<AllyAgent>();

    public static ScoutManager ScoutManager { get; private set; }

    public const int viewDistance = 31;
    public const int viewDistanceSqr = 961;


    public Vector3 currentBasePosition { get; private set; }
    public AllyAgent groupLeader { get; private set; }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            ScoutManager = (ScoutManager)ScriptableObject.CreateInstance("ScoutManager");
        }
    }

    private void Start()
    {
        for (int i = 0; i < GameData.Instance.allies.Count; i++)
        {
            m_agents.Add((AllyAgent)GameData.Instance.allies[i]);
            m_agents[i].AddAllComponents();
        }
    }


    public void AssignRoles()
    {
        Vector3 positionTotal = Vector3.zero;
        for (int i = 0; i < m_agents.Count; i++)
        {
            if(i == 0)
            {
                ScoutManager.SetAgentAsScoutLead(m_agents[i]);
            }
            else if(i == 1)
            {
                ScoutManager.SetAgentAsScoutFollower(m_agents[i]);
            }
            else
            {
                m_agents[i].SwitchAgentRole(AllyAgentRole.OnBreak);
            }

            positionTotal += m_agents[i].transform.position;
        }


        currentBasePosition = positionTotal / m_agents.Count;
    }


    public void Update()
    {
        for(int i = 0; i < m_agents.Count; i++)
        {
            if (!m_agents[i].gameObject.activeSelf)
            {
                m_agents.Remove(m_agents[i]);
            }
        }

        if (groupLeader != null && !groupLeader.gameObject.activeSelf)
        {
            AssignNewGroupLeader();
        }


        if (!AnyVisibleEnemies())
        {
            AssignRoles();
        }
    }

    private bool AnyVisibleEnemies()
    {
        for(int i = 0; i < m_agents.Count; i++)
        {
            for(int j = 0; j < GameData.Instance.enemies.Count; j++)
            {
                if(!GameData.Instance.enemies[j].gameObject.activeSelf)
                {
                    continue;
                } 
                if (Algorithms.IsPositionInLineOfSight((Vector2)m_agents[i].transform.position, (Vector2)GameData.Instance.enemies[j].transform.position))
                {
                    return true;
                }
            }         
        }
        return false;
    }



    public void FoundEnemyToAttack(EnemyAgent enemyToAttack)
    {
        Vector3 enemyPosition = enemyToAttack.transform.position;

        Node enemyNode = GridData.Instance.GetNodeAt(enemyPosition);

        List<Node> currentPath = Algorithms.AStar(GridData.Instance.GetNodeAt(currentBasePosition), enemyNode);

        if(currentPath == null)
        {
            AssignRoles();
        }

        for (int i = 0; i < m_agents.Count; i++)
        {
            if(i == 0)
            {
                m_agents[i].SwitchAgentRole(AllyAgentRole.GroupLeader);
                m_agents[i].groupLeader.SetCurrentPath(currentPath);
                groupLeader = m_agents[i];
            }
            else
            {
                m_agents[i].SwitchAgentRole(AllyAgentRole.GroupMember);
            }

                
        }
    }







    public void AssignNewGroupLeader()
    {
        if (m_agents.Count > 0)
        {
            m_agents[0].SwitchAgentRole(AllyAgentRole.GroupLeader);
            groupLeader = m_agents[0];
        }
    }
}

