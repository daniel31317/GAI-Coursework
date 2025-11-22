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

    private int alliesReadyToAttack = 0;

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

    private void Update()
    {
        if(!attackEnemies)
        {
            return;
        }

        if(alliesReadyToAttack == GameData.Instance.allies.Count)
        {
            alliesReadyToAttack++;
            for (int i = 0; i < GameData.Instance.allies.Count; i++)
            {
                ((AllyAgent)GameData.Instance.allies[i]).GetComponent<AllyAgent>().StartAttacking();
            }
        }



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
        alliesReadyToAttack = 0;
        enemyPosition = GridData.Instance.GetNodeAt(position);
        attackEnemies = true;
        NodesOfInterest movePositions = GetPositionsDistanceAwayFromNode(enemyPosition, 10);
        

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

            bool validPath = false;

            Vector3 blockPos = Vector3.zero;

            if (movePositions.nodesInLosAndRange.Count > 0)
            {
                while (!validPath && movePositions.nodesInLosAndRange.Count > 0)
                {
                    currentPathToEnemy = Algorithms.AStar(GridData.Instance.GetNodeAt(((AllyAgent)GameData.Instance.allies[i]).transform.position), movePositions.nodesInLosAndRange[0]);
                    validPath = CheckPathDoesntGoWithinRangeOfEnemy(currentPathToEnemy, 10);
                    if (validPath)
                    {
                        blockPos = (Vector3)movePositions.nodesInLosAndRange[0].position;
                    }
                    movePositions.nodesInLosAndRange.RemoveAt(0);
                    
                }                             
            }
            if(movePositions.nodesInLos.Count > 0 && !validPath)
            {
                while (!validPath && movePositions.nodesInLos.Count > 0)
                {
                    currentPathToEnemy = Algorithms.AStar(GridData.Instance.GetNodeAt(((AllyAgent)GameData.Instance.allies[i]).transform.position), movePositions.nodesInLos[0]);
                    validPath = CheckPathDoesntGoWithinRangeOfEnemy(currentPathToEnemy, 10);
                    if(validPath)
                    {
                        blockPos = (Vector3)movePositions.nodesInLos[0].position;
                    }
                    movePositions.nodesInLos.RemoveAt(0);                   
                }
            }
            if (movePositions.nodesInNeither.Count > 0 && !validPath)
            {
                while (!validPath && movePositions.nodesInNeither.Count > 0)
                {
                    currentPathToEnemy = Algorithms.AStar(GridData.Instance.GetNodeAt(((AllyAgent)GameData.Instance.allies[i]).transform.position), movePositions.nodesInNeither[0]);
                    validPath = CheckPathDoesntGoWithinRangeOfEnemy(currentPathToEnemy, 10);
                    if (validPath)
                    {
                        blockPos = (Vector3)movePositions.nodesInNeither[0].position;
                    }
                    movePositions.nodesInNeither.RemoveAt(0);
                }
            }




            Vector3 offset = new Vector3(0.5f, 0.5f, 1f);
            GameObject temp = Instantiate(moveBlock, blockPos + offset, Quaternion.identity);
            temp.transform.parent = transform;

            ((AllyAgent)GameData.Instance.allies[i]).SetAgentRole(AllyAgentRole.Soldier);
            ((AllyAgent)GameData.Instance.allies[i]).GetComponent<RunToLocatedEnemy>().enabled = true;
            ((AllyAgent)GameData.Instance.allies[i]).GetComponent<RunToLocatedEnemy>().SetCurrentPath(currentPathToEnemy);
        }
    }


    private NodesOfInterest GetPositionsDistanceAwayFromNode(Node node, int distance)
    {
        List<Node> allNodes = Algorithms.ScoreAllAccessibleNodes(node, distance);
        NodesOfInterest positionNodes = Algorithms.GetNodesOfInterest(allNodes, 2, node.position, distance);

        List<float> distancesFromBaseToNodes = new List<float>();

        //calculate distances
        for(int i = 0; i < positionNodes.nodesInLosAndRange.Count; i++)
        {
            distancesFromBaseToNodes.Add(Vector2.SqrMagnitude(positionNodes.nodesInLosAndRange[i].position - enemyPosition.position));
        }

        //bubble sort from lowest to biggest sqr distance so closer nodes are preffered
        for (int i = 0; i < positionNodes.nodesInLosAndRange.Count - 1; i++)
        {
            for (int j = 0; j < positionNodes.nodesInLosAndRange.Count - 1 - i; j++)
            {
                if (distancesFromBaseToNodes[j] > distancesFromBaseToNodes[j + 1])
                {
                    Node temp = positionNodes.nodesInLosAndRange[j];
                    float distTemp = distancesFromBaseToNodes[j];

                    positionNodes.nodesInLosAndRange[j] = positionNodes.nodesInLosAndRange[j + 1];
                    positionNodes.nodesInLosAndRange[j + 1] = temp;

                    distancesFromBaseToNodes[j] = distancesFromBaseToNodes[j + 1];
                    distancesFromBaseToNodes[j + 1] = distTemp;
                }
            }
        }


        distancesFromBaseToNodes.Clear();



        //calculate distances
        for (int i = 0; i < positionNodes.nodesInLos.Count; i++)
        {
            distancesFromBaseToNodes.Add(Vector2.SqrMagnitude(positionNodes.nodesInLos[i].position - enemyPosition.position));
        }

        //bubble sort from lowest to biggest sqr distance so closer nodes are preffered
        for (int i = 0; i < positionNodes.nodesInLos.Count - 1; i++)
        {
            for (int j = 0; j < positionNodes.nodesInLos.Count - 1 - i; j++)
            {
                if (distancesFromBaseToNodes[j] > distancesFromBaseToNodes[j + 1])
                {
                    Node temp = positionNodes.nodesInLos[j];
                    float distTemp = distancesFromBaseToNodes[j];

                    positionNodes.nodesInLos[j] = positionNodes.nodesInLos[j + 1];
                    positionNodes.nodesInLos[j + 1] = temp;

                    distancesFromBaseToNodes[j] = distancesFromBaseToNodes[j + 1];
                    distancesFromBaseToNodes[j + 1] = distTemp;
                }
            }
        }


        distancesFromBaseToNodes.Clear();


        //calculate distances
        for (int i = 0; i < positionNodes.nodesInNeither.Count; i++)
        {
            distancesFromBaseToNodes.Add(Vector2.SqrMagnitude(positionNodes.nodesInNeither[i].position - enemyPosition.position));
        }

        //bubble sort from lowest to biggest sqr distance so closer nodes are preffered
        for (int i = 0; i < positionNodes.nodesInNeither.Count - 1; i++)
        {
            for (int j = 0; j < positionNodes.nodesInNeither.Count - 1 - i; j++)
            {
                if (distancesFromBaseToNodes[j] > distancesFromBaseToNodes[j + 1])
                {
                    Node temp = positionNodes.nodesInNeither[j];
                    float distTemp = distancesFromBaseToNodes[j];

                    positionNodes.nodesInNeither[j] = positionNodes.nodesInNeither[j + 1];
                    positionNodes.nodesInNeither[j + 1] = temp;

                    distancesFromBaseToNodes[j] = distancesFromBaseToNodes[j + 1];
                    distancesFromBaseToNodes[j + 1] = distTemp;
                }
            }
        }


        return positionNodes;

    }


    private bool CheckPathDoesntGoWithinRangeOfEnemy(List<Node> path, float distance)
    {
        for (int i = 0; i < path.Count; i++)
        {
            if(Vector2.SqrMagnitude(path[i].position - enemyPosition.position) < distance * distance)
            {
                return false;
            }
        }
        return true;
    }






    public void AllyInPositionToAttack()
    {
        alliesReadyToAttack++;
    }






}
