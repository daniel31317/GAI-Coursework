using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AllyManager : MonoBehaviour
{
    public static AllyManager Instance { get; private set; }
    public GameObject scoutBlock;
    public GameObject enemyBlock;
    public GameObject moveBlock;
    public static ScoutManager ScoutManager { get; private set; }

    public bool attackEnemies { get; private set; } = false;
    public Node enemyPosition { get; private set; }

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
        ScoutManager.scoutBlock = scoutBlock;   
        ScoutManager.enemyBlock = enemyBlock;   
    }


    public void AssignRoles()
    {
        Vector3 positionTotal = Vector3.zero;
        int amount = 0;
        for (int i = 0; i < GameData.Instance.allies.Count; i++)
        {
            ((AllyAgent)GameData.Instance.allies[i]).SetAgentRole(AllyAgentRole.Soldier);
            ((AllyAgent)GameData.Instance.allies[i]).AddComponent<RunToLocatedEnemy>();
            ((AllyAgent)GameData.Instance.allies[i]).GetComponent<RunToLocatedEnemy>().enabled = false;
            positionTotal += GameData.Instance.allies[i].transform.position;
            amount++;
        }


        currentBasePosition = positionTotal / amount;

        ScoutManager.SetAgentAsScoutLead((AllyAgent)GameData.Instance.allies[0]);
        ScoutManager.SetAgentAsScoutFollower((AllyAgent)GameData.Instance.allies[1]);
    }

    
    public void FoundEnemyToAttack(Vector2 position)
    {
        enemyPosition = GridData.Instance.GetNodeAt(position);
        attackEnemies = true;
        List<Node> movePositions = GetPositionsDistanceAwayFromNode(enemyPosition, 15);
        

        for (int i = 0; i < GameData.Instance.allies.Count; i++)
        {
            if(((AllyAgent)GameData.Instance.allies[i]).agentRole == AllyAgentRole.LeadScout)
            {
                ((AllyAgent)GameData.Instance.allies[i]).GetComponent<ScoutWander>().enabled = false;
            }
            else if(((AllyAgent)GameData.Instance.allies[i]).agentRole == AllyAgentRole.FollowerScout)
            {
                ((AllyAgent)GameData.Instance.allies[i]).GetComponent<ScoutFollow>().enabled = false;
            }

            List<Node> currentPathToEnemy = new List<Node>();
            if(movePositions.Count > 0)
            {
                currentPathToEnemy = Algorithms.AStar(GridData.Instance.GetNodeAt(((AllyAgent)GameData.Instance.allies[i]).transform.position), movePositions[0]);
                Vector3 offset = new Vector3(0.5f, 0.5f, 1f);
                GameObject temp = Instantiate(moveBlock, (Vector3)movePositions[0].position + offset, Quaternion.identity);
                temp.transform.parent = transform;
                movePositions.RemoveAt(0);
            }
            

            ((AllyAgent)GameData.Instance.allies[i]).SetAgentRole(AllyAgentRole.Soldier);
            ((AllyAgent)GameData.Instance.allies[i]).GetComponent<RunToLocatedEnemy>().enabled = true;
            ((AllyAgent)GameData.Instance.allies[i]).GetComponent<RunToLocatedEnemy>().SetCurrentPath(currentPathToEnemy);
        }
    }


    private List<Node> GetPositionsDistanceAwayFromNode(Node node, int distance)
    {
        List<Node> positionNodes = Algorithms.ScoreAllAccessibleNodes(node, distance, distance + 5);
        positionNodes = Algorithms.GetNodesOfInterest(positionNodes, 2, node.position);
        return positionNodes;

    }







}
