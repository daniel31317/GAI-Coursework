using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;

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
    public const int viewDistance = 10;

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

    
    public void FoundEnemyToAttack(List<Node> currentPathToEnemyReverse, Vector2 position)
    {
        alliesReadyToAttack = 0;
        enemyPosition = GridData.Instance.GetNodeAt(position);
        attackEnemies = true;


        Profiler.BeginSample("GetPositionsDistanceAwayFromNode");
        NodesOfInterest movePositions = GetPositionsDistanceAwayFromNode(enemyPosition);
        Profiler.EndSample();

        currentPathToEnemyReverse.Reverse();


        Profiler.BeginSample("For loop");
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

            Vector3 blockPos = Vector3.zero;

            List<Node> additionalPathFind = null;

            Profiler.BeginSample("Selecting Node and path");
            while(movePositions.nodesInLos.Count > 0 && additionalPathFind == null)
            {
                currentPathToEnemy.Clear();
                int closestNodeOnPathIndex = GetClosestNodeOnPathIndex(currentPathToEnemyReverse, movePositions.nodesInLos[0]);
                for (int j = 0; j < closestNodeOnPathIndex; j++)
                {
                    currentPathToEnemy.Add(currentPathToEnemyReverse[j]);
                }

                Profiler.BeginSample("A star");
                additionalPathFind = Algorithms.AStar(currentPathToEnemyReverse[closestNodeOnPathIndex], movePositions.nodesInLos[0], enemyPosition, viewDistance, true);
                Profiler.EndSample();
                blockPos = (Vector3)movePositions.nodesInLos[0].position;
                movePositions.nodesInLos.RemoveAt(0);

                if (additionalPathFind == null)
                {
                    continue;
                }

                for (int j = 0; j < additionalPathFind.Count; j++)
                {
                    currentPathToEnemy.Add(additionalPathFind[j]);
                }

                       
            }

            while (movePositions.nodesNotLos.Count > 0 && additionalPathFind == null)
            {
                currentPathToEnemy.Clear();
                int closestNodeOnPathIndex = GetClosestNodeOnPathIndex(currentPathToEnemyReverse, movePositions.nodesNotLos[0]);
                for (int j = 0; j < closestNodeOnPathIndex; j++)
                {
                    currentPathToEnemy.Add(currentPathToEnemyReverse[j]);
                }

                Profiler.BeginSample("A star");
                additionalPathFind = Algorithms.AStar(currentPathToEnemyReverse[closestNodeOnPathIndex], movePositions.nodesNotLos[0], enemyPosition, viewDistance, true);
                Profiler.EndSample();

                blockPos = (Vector3)movePositions.nodesNotLos[0].position;
                movePositions.nodesNotLos.RemoveAt(0);

                if (additionalPathFind == null)
                {
                    continue;
                }

                for (int j = 0; j < additionalPathFind.Count; j++)
                {
                    currentPathToEnemy.Add(additionalPathFind[j]);
                }

                
            }
            Profiler.EndSample();


            Vector3 offset = new Vector3(0.5f, 0.5f, 1f);
            GameObject temp = Instantiate(moveBlock, blockPos + offset, Quaternion.identity);
            temp.transform.parent = transform;

            ((AllyAgent)GameData.Instance.allies[i]).SetAgentRole(AllyAgentRole.Soldier);
            ((AllyAgent)GameData.Instance.allies[i]).GetComponent<RunToLocatedEnemy>().enabled = true;
            ((AllyAgent)GameData.Instance.allies[i]).GetComponent<RunToLocatedEnemy>().SetCurrentPath(currentPathToEnemy);
        }


        Profiler.EndSample();


    }


    private NodesOfInterest GetPositionsDistanceAwayFromNode(Node node)
    {
        Profiler.BeginSample("GetPositionsDistanceAwayFromNode");
        Profiler.BeginSample("Get Nodes");
        NodesOfInterest positionNodes = Algorithms.GetNodesOfInterest(Algorithms.FIndAllAccesableNodes(node, viewDistance), 2, node.position, viewDistance);
        Profiler.EndSample();


        Profiler.BeginSample("First sort");
        //sort from closest to furthest for both lists
        positionNodes.nodesInLos.Sort((x, y) =>
        {
            float xNodeDist = Vector2.SqrMagnitude(x.position - enemyPosition.position);
            float yNodeDist = Vector2.SqrMagnitude(y.position - enemyPosition.position);
            return xNodeDist.CompareTo(yNodeDist);
        });
        Profiler.EndSample();

        Profiler.BeginSample("Second Loop");
        positionNodes.nodesNotLos.Sort((x, y) =>
        {
            float xNodeDist = Vector2.SqrMagnitude(x.position - enemyPosition.position);
            float yNodeDist = Vector2.SqrMagnitude(y.position - enemyPosition.position);
            return xNodeDist.CompareTo(yNodeDist);
        });

        Profiler.EndSample();

        Profiler.EndSample();   
        return positionNodes;
    }



    private int GetClosestNodeOnPathIndex(List<Node> path, Node targetNode)
    {
        float shortestDistance = 100000000;
        int index = 0;
        for (int j = 0; j < path.Count; j++)
        {
            if (enemyPosition.SqrMagnitude(path[j]) < viewDistance * viewDistance)
            {
                continue;
            }
            float currentDistance = path[j].SqrMagnitude(targetNode);
            if (currentDistance < shortestDistance)
            {
                shortestDistance = currentDistance;
                index = j;
            }
        }
        return index;
    }


    public void AllyInPositionToAttack()
    {
        alliesReadyToAttack++;
    }






}
