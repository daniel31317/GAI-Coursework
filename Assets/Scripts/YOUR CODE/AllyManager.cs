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

    private bool scoutRolesAssigned = false;

    public Vector3 currentBasePosition { get; private set; }
    public AllyAgent groupLeader { get; private set; }


    private void Awake()
    {
        //create instance of itself and scout manager
        if (Instance == null)
        {
            Instance = this;
            ScoutManager = (ScoutManager)ScriptableObject.CreateInstance("ScoutManager");
        }
    }

    private void Start()
    {
        //add all componets to ally agents
        for (int i = 0; i < GameData.Instance.allies.Count; i++)
        {
            m_agents.Add((AllyAgent)GameData.Instance.allies[i]);
            m_agents[i].AddAllComponents();
        }
    }


    //this is called when there are no known enemies and assigns one leader scout and one follower scout the rest go idle
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

        scoutRolesAssigned = true;
    }


    public void Update()
    {
        //look for any dead agents and remove them from the list
        for (int i = 0; i < m_agents.Count; i++)
        {
            if (!m_agents[i].gameObject.activeSelf)
            {
                m_agents.Remove(m_agents[i]);
            }
        }


        //if the group leader is dead assign a new one
        if (groupLeader != null && !groupLeader.gameObject.activeSelf)
        {
            AssignNewGroupLeader();
        }

        //dodge incoming rockets
        HandleDodgingRockets();

        //if there are no visible enemies and we are in group leader phase switch roles abck to scout
        if (!AnyVisibleEnemies() && !scoutRolesAssigned && groupLeader.groupLeader.atShootPosition)
        {
            AssignRoles();
        }
    }


    //loops through all agents and enemies to see if any are visible
    private bool AnyVisibleEnemies()
    {
        for (int i = 0; i < m_agents.Count; i++)
        {
            for (int j = 0; j < GameData.Instance.enemies.Count; j++)
            {
                //if enemy is not active skip
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
        //unassign scout roles
        scoutRolesAssigned = false;

        //get enemy position and relevant node
        Vector3 enemyPosition = enemyToAttack.transform.position;

        Node enemyNode = GridData.Instance.GetNodeAt(enemyPosition);

        //get path to them
        List<Node> currentPath = Algorithms.AStar(GridData.Instance.GetNodeAt(currentBasePosition), enemyNode);

        //if no path found assign roles and return (mainly a safety check shouldn't really be called)
        if (currentPath == null)
        {
            AssignRoles();
            return;
        }

        //assign group leader and followers
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


    //make new group leader the first agent in the list and assign followers the new leader
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
        float dodgeRadius = Attack.RocketData.radius + 1;
        float dodgeSqr = dodgeRadius * dodgeRadius;
        for (int i = 0; i < GameData.Instance.attacks.Count; i++)
        {
            Attack currentAttack = GameData.Instance.attacks[i];
  
            if(!IsRocketValid(currentAttack))
            {
                continue;
            }


            /*this code finds the straight line of the rocket attack 
             * it then finds the perpendicular line from each agent to the rocket line
             * it then gets where the two lines intersect
             * and from there applies a force to doge if the intersection point is within a certain distance of the agent
            */
            float xDiff = currentAttack.currentPosition.x - currentAttack.StartPosition.x;

            //divide by zero checks for gradient calculations
            float m1 = 0;
            float m2 = 0;
            if (xDiff != 0)
            {
                m1 = (currentAttack.currentPosition.y - currentAttack.StartPosition.y) / xDiff;
                if(m1 != 0)
                {
                    m2 = -1 / m1;
                }
            }

            
            float c1 = currentAttack.StartPosition.y - (m1 * currentAttack.StartPosition.x);
            

            for (int j = 0; j < m_agents.Count; j++)
            {
                float c2 = m_agents[j].transform.position.y - (m2 * m_agents[j].transform.position.x);

                // y = m1x + c1

                // y = m2x + c2

                //m2x + c2 = m1x + c1

                //x = (c1 - c2) / (m2 - m1)

                float gradientTotal = m2 - m1;
                float cInterceptTotal = c1 - c2;
                float xIntercept = cInterceptTotal / gradientTotal;
                Vector3 intersectionPos = new Vector3(xIntercept, m1 * xIntercept + c1, 0f);

                //dodge if within radius
                float distSqr = Vector3.SqrMagnitude(intersectionPos - m_agents[j].transform.position);

                if (distSqr <= dodgeSqr)
                {
                    if (m_agents[j].agentRole == AllyAgentRole.FollowLeader)
                    {
                        m_agents[j].followLeader.AddDodgeRocketForce((intersectionPos - m_agents[j].transform.position));
                    }
                    else if (m_agents[j].agentRole == AllyAgentRole.GroupLeader)
                    {
                        m_agents[j].groupLeader.AddDodgeRocketForce((intersectionPos - m_agents[j].transform.position));
                    }

                }

            }
            
        }       
    }



    //conditions to check if rocket is valid for dodging
    private bool IsRocketValid(Attack currentAttack)
    {
        if (currentAttack.Type != Attack.AttackType.Rocket)
        {
            return false;
        }

        if (!currentAttack.IsEnemy)
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

