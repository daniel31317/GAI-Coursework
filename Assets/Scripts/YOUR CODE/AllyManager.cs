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

    private bool rolesAssigned = false;

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
            if (i == 0)
            {
                ScoutManager.SetAgentAsScoutLead(m_agents[i]);
            }
            else if (i == 1)
            {
                ScoutManager.SetAgentAsScoutFollower(m_agents[i]);
            }
            else
            {
                m_agents[i].SwitchAgentRole(AllyAgentRole.Idle, null);
            }

            positionTotal += m_agents[i].transform.position;
        }


        currentBasePosition = positionTotal / m_agents.Count;

        rolesAssigned = true;
    }


    public void Update()
    {
        for (int i = 0; i < m_agents.Count; i++)
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


        HandleDodgingRockets();


        if (!AnyVisibleEnemies() && !rolesAssigned && groupLeader.groupLeader.atShootPosition)
        {
            AssignRoles();
        }
    }

    private bool AnyVisibleEnemies()
    {
        for (int i = 0; i < m_agents.Count; i++)
        {
            for (int j = 0; j < GameData.Instance.enemies.Count; j++)
            {
                if (!GameData.Instance.enemies[j].gameObject.activeSelf)
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
        rolesAssigned = false;

        Vector3 enemyPosition = enemyToAttack.transform.position;

        Node enemyNode = GridData.Instance.GetNodeAt(enemyPosition);

        List<Node> currentPath = Algorithms.AStar(GridData.Instance.GetNodeAt(currentBasePosition), enemyNode);

        if (currentPath == null)
        {
            AssignRoles();
        }

        for (int i = 0; i < m_agents.Count; i++)
        {
            if (i == 0)
            {
                m_agents[i].SwitchAgentRole(AllyAgentRole.GroupLeader, null);
                m_agents[i].groupLeader.SetCurrentPath(currentPath);
                groupLeader = m_agents[i];
            }
            else
            {
                m_agents[i].SwitchAgentRole(AllyAgentRole.FollowLeader, groupLeader);
            }


        }
    }



    public void AssignNewGroupLeader()
    {
        if (m_agents.Count > 0)
        {
            m_agents[0].SwitchAgentRole(AllyAgentRole.GroupLeader, null);
            groupLeader = m_agents[0];

            if (m_agents.Count > 1)
            {
                for (int i = 1; i < m_agents.Count; i++)
                {
                    m_agents[i].SwitchAgentRole(AllyAgentRole.FollowLeader, groupLeader);
                }
            }
        }
    }


    private void HandleDodgingRockets()
    {
        for (int i = 0; i < GameData.Instance.attacks.Count; i++)
        {
            Attack currentAttack = GameData.Instance.attacks[i];
            if (currentAttack.Type != Attack.AttackType.Rocket && currentAttack.IsEnemy)
            {
                if(!IsRocketValid(currentAttack))
                {
                    continue;
                }

                float m1 = (currentAttack.StartPosition.y - currentAttack.currentPosition.y) / (currentAttack.StartPosition.x - currentAttack.currentPosition.x);
                float c1 = currentAttack.StartPosition.y + (m1 * -currentAttack.StartPosition.x);
                float m2 = -1 / m1;

                for (int j = 0; j < m_agents.Count; j++)
                {
                    float c2 = m_agents[j].transform.position.y + (m1 * -m_agents[j].transform.position.x);
                    float gradientTotal = m1 - m2;
                    float cInterceptTotal = c1 - c2;
                    float xIntercept = cInterceptTotal / gradientTotal;
                    Vector3 intersectionPos = new Vector3(xIntercept, m1 * xIntercept + c1, 0f);

                    if(Vector3.SqrMagnitude(intersectionPos - m_agents[j].transform.position) <= Attack.RocketData.radius * Attack.RocketData.radius)
                    {
                        m_agents[j].followLeader.AdddodgeRocketForce(m_agents[j].transform.position - intersectionPos);
                    }

                }
            }
        }       
    }




    private bool IsRocketValid(Attack currentAttack)
    {
        if (currentAttack.Type != Attack.AttackType.Rocket)
        {
            return false;
        }

        if (currentAttack.StartPosition == currentAttack.currentPosition)
        {
            return false;
        }

        return true;
    }
}

