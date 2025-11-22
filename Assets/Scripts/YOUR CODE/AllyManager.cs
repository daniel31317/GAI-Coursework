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
        (List<Node>, List<Node>) movePositions = GetPositionsDistanceAwayFromNode(enemyPosition, 15);
        

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

            if (movePositions.Item1.Count > 0)
            {
                

                while (!validPath && movePositions.Item1.Count > 0)
                {
                    currentPathToEnemy = Algorithms.AStar(GridData.Instance.GetNodeAt(((AllyAgent)GameData.Instance.allies[i]).transform.position), movePositions.Item1[0]);
                    validPath = CheckPathDoesntGoWithinRangeOfEnemy(currentPathToEnemy, 15);
                    if (validPath)
                    {
                        blockPos = (Vector3)movePositions.Item1[0].position;
                    }
                    movePositions.Item1.RemoveAt(0);
                    
                }
                 
                                 
            }
            if(movePositions.Item2.Count > 0 && !validPath)
            {

                while (!validPath && movePositions.Item2.Count > 0)
                {
                    currentPathToEnemy = Algorithms.AStar(GridData.Instance.GetNodeAt(((AllyAgent)GameData.Instance.allies[i]).transform.position), movePositions.Item2[0]);
                    validPath = CheckPathDoesntGoWithinRangeOfEnemy(currentPathToEnemy, 15);
                    if(validPath)
                    {
                        blockPos = (Vector3)movePositions.Item2[0].position;
                    }
                    movePositions.Item2.RemoveAt(0);
                    
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


    private (List<Node>, List<Node>) GetPositionsDistanceAwayFromNode(Node node, int distance)
    {
        List<Node> allNodes = Algorithms.ScoreAllAccessibleNodes(node, distance);
        (List<Node>, List<Node>) positionNodes = Algorithms.GetNodesOfInterest(allNodes, 2, node.position);

        (List<float>, List<float>) distancesFromBaseToNodes = (new List<float>(), new List<float>());

        //calculate distances
        for(int i = 0; i < positionNodes.Item1.Count; i++)
        {
            distancesFromBaseToNodes.Item1.Add(Vector2.SqrMagnitude(positionNodes.Item1[i].position - enemyPosition.position));
        }

        //calculate distances
        for(int i = 0; i < positionNodes.Item2.Count; i++)
        {
            distancesFromBaseToNodes.Item2.Add(Vector2.SqrMagnitude(positionNodes.Item2[i].position - enemyPosition.position));
        }

        //bubble sort from lowest to biggest sqr distance so closer nodes are preffered
        for (int i = 0; i < positionNodes.Item1.Count - 1; i++)
        {
            for (int j = 0; j < positionNodes.Item1.Count - 1 - i; j++)
            {
                if (distancesFromBaseToNodes.Item1[j] > distancesFromBaseToNodes.Item1[j + 1])
                {
                    Node temp = positionNodes.Item1[j];
                    float distTemp = distancesFromBaseToNodes.Item1[j];

                    positionNodes.Item1[j] = positionNodes.Item1[j + 1];
                    positionNodes.Item1[j + 1] = temp;

                    distancesFromBaseToNodes.Item1[j] = distancesFromBaseToNodes.Item1[j + 1];
                    distancesFromBaseToNodes.Item1[j + 1] = distTemp;
                }
            }
        }


        //bubble sort from lowest to biggest sqr distance so closer nodes are preffered
        for (int i = 0; i < positionNodes.Item2.Count - 1; i++)
        {
            for (int j = 0; j < positionNodes.Item2.Count - 1 - i; j++)
            {
                if (distancesFromBaseToNodes.Item2[j] > distancesFromBaseToNodes.Item2[j + 1])
                {
                    Node temp = positionNodes.Item2[j];
                    float distTemp = distancesFromBaseToNodes.Item2[j];

                    positionNodes.Item2[j] = positionNodes.Item2[j + 1];
                    positionNodes.Item2[j + 1] = temp;

                    distancesFromBaseToNodes.Item2[j] = distancesFromBaseToNodes.Item2[j + 1];
                    distancesFromBaseToNodes.Item2[j + 1] = distTemp;
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
