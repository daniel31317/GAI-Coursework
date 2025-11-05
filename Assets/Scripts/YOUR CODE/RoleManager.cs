using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class RoleManager : MonoBehaviour
{
    public static RoleManager Instance;
    public GameObject testBlock;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    private const int NUMBER_OF_SCOUTS = 1;


    public void AssignRoles()
    {
        int currentRoleIndex = 0;
        for (int i = 0; i < NUMBER_OF_SCOUTS; i++)
        {
            AllyAgent agent = ((AllyAgent)GameData.Instance.allies[i]);
            agent.SetAgentRole(AllyAgentRole.Scout);
            agent.AddComponent<Wander>();
            currentRoleIndex++;
            ScoutPositionToCheck(agent.transform.position);
        }


        //now start looping i starting from currentRoleIndex
    }


    private List<Node> ScoutPositionToCheck(Vector3  pos)
    {

        List<Node> openNodeList = new List<Node>();
        List<Node> closedNodeList = new List<Node>();

        Node startNode = GridData.Instance.GetNodeAt(pos);

        startNode.onOpenList = true;
        openNodeList.Add(startNode);

        ScoreAllAccessibleNodes(openNodeList, closedNodeList);

        RemoveNodesWith8Neighbours(closedNodeList);

        SortNodeListByDescendingF(closedNodeList);

        //get points of interest and remove anthing within +/- 5 on x and y
        List<Node> pointOfInterest = GetNodesOfInterest(closedNodeList);


        //return GetNodesOfInterest(closedNodeList);



        //show blocks can be removed later
        Vector2 offset = new Vector2(0.5f, 0.5f);

        for(int i = 0; i < pointOfInterest.Count; i++)
        {
            Instantiate(testBlock, pointOfInterest[i].position + offset, Quaternion.identity);
        }



        return pointOfInterest;
    }

    private void ScoreAllAccessibleNodes(List<Node> openList, List<Node> closedList)
    {
        //score every single possible grid we can go to using a scoring system simlar to dijsktra but wiht no sorting
        while (openList.Count > 0)
        {
            Node currentNode = openList[0];
            openList.RemoveAt(0);

            currentNode.onOpenList = false;
            currentNode.onClosedList = true;
            closedList.Add(currentNode);


            foreach (Node childNode in currentNode.neighbours)
            {
                if (!childNode.onClosedList)
                {
                    int f = currentNode.f + CalculateInitialCost(currentNode.position, childNode.position);
                    if (f <= childNode.f || !childNode.onOpenList)
                    {
                        childNode.f = f;
                    }
                    if (!childNode.onOpenList)
                    {
                        childNode.SetParent(currentNode);
                        childNode.onOpenList = true;
                        openList.Add(childNode);
                    }
                }
            }
        }
    }




    private void RemoveNodesWith8Neighbours(List<Node> nodes)
    {
        //remove non 8 neighboured tiles from list
        //we dont do this before so the algorithm can still find nodes possible to reach through tight spaces
        for (int i = 0; i < nodes.Count;)
        {
            if (nodes[i].neighbours.Count != 8)
            {
                nodes.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }
    }

    //sort list fro0m highest f score to smallest
    private void SortNodeListByDescendingF(List<Node> list)
    {
        
        for (int i = 0; i < list.Count - 1; i++)
        {
            for (int j = 0; j < list.Count - 1 - i; j++)
            {
                if (list[j].f < list[j + 1].f)
                {
                    Node temp = list[j];
                    list[j] = list[j + 1];
                    list[j + 1] = temp;
                }
            }
        }
    }



    //get points of interest and remove anthing within +/- 5 on x and y
    private List<Node> GetNodesOfInterest(List<Node> nodesToSearch)
    {
        List<Node> nodesOfInterest = new List<Node>();
        while (nodesToSearch.Count > 0)
        {
            nodesOfInterest.Add(nodesToSearch[0]);
            nodesToSearch.RemoveAt(0);

            if (nodesToSearch.Count > 1)
            {
                List<Node> nodesToRemove = GetNodesToRemove(nodesToSearch, nodesOfInterest[nodesOfInterest.Count - 1]);

                for (int i = 0; i < nodesToRemove.Count; i++)
                {
                    nodesToSearch.Remove(nodesToRemove[i]);
                }
            }
        }

        return nodesOfInterest;
    }



    //gets the nodes needed to be removed from the closed list based on distance to an important node
    private List<Node> GetNodesToRemove(List<Node> nodes, Node rangeNode)
    {
        List<Node> removeTheseNodes = new List<Node>();
        //loop thorugh closed list with current point of interest and anything out of distance add to remove list
        for (int i = 0; i < nodes.Count; i++)
        {
            if (IsNodeInRange(rangeNode,  nodes[i]))
            {
                removeTheseNodes.Add(nodes[i]);
            }
        }

        return removeTheseNodes;
    }



    private int CalculateInitialCost(Vector3 pos1, Vector3 pos2)
    {
        Vector3 cost = pos2 - pos1;
        if ((cost.x + cost.y) < 2)
        {
            if (cost.x > 0)
            {
                return 10;
            }
            return 10;
        }
        return 20;
    }

    private bool IsNodeInRange(Node originNode, Node otherNode)
    {
        int distanceLimit = 15;

        Vector2 posDiff = originNode.position - otherNode.position;
        if (posDiff.sqrMagnitude <= distanceLimit * distanceLimit)
        {
            return true;
        }
        return false;
    }


}
