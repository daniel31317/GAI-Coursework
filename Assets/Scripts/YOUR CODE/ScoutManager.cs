using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ScoutManager : ScriptableObject
{
    private AllyAgent leadScout;
    private AllyAgent followScout;
    public List<Node> nodesToScout { get; private set; }

    public GameObject scoutBlock;
    public GameObject enemyBlock;

    public Node tempEnemyLocation;
    



    public void SetAgentAsScoutLead(AllyAgent newAgent)
    {
        leadScout = newAgent;
        leadScout.SetAgentRole(AllyAgentRole.LeadScout);
        leadScout.AddComponent<ScoutWander>();
        ScoutPositionToCheck(leadScout.transform.position);
    }

    public void SetAgentAsScoutFollower(AllyAgent newAgent)
    {
        followScout = newAgent;
        followScout.SetAgentRole(AllyAgentRole.FollowerScout);
        followScout.AddComponent<ScoutFollow>();
        leadScout.GetComponent<ScoutWander>().SetFollowerScout(followScout);
    }

    private void ScoutPositionToCheck(Vector3 pos)
    {

        List<Node> openNodeList = new List<Node>();
        List<Node> closedNodeList = new List<Node>();

        Node startNode = GridData.Instance.GetNodeAt(pos);

        startNode.onOpenList = true;
        openNodeList.Add(startNode);

        ScoreAllAccessibleNodes(openNodeList, closedNodeList);

        tempEnemyLocation = GridData.Instance.GetNodeAt(new Vector3(55.5f, 70.5f, 0));//closedNodeList[Random.Range(0, closedNodeList.Count - 1)];  

        RemoveNodesWithoutCertainAmountOfNeighbours(closedNodeList);

        SortNodeListByDescendingF(closedNodeList);

        //get points of interest and remove anthing within +/- 5 on x and y
        nodesToScout = GetNodesOfInterest(closedNodeList);

        //show blocks can be removed later
        Vector2 offset = new Vector2(0.5f, 0.5f);
        for (int i = 0; i < nodesToScout.Count; i++)
        {
            GameObject temp = Instantiate(scoutBlock, nodesToScout[i].position + offset, Quaternion.identity);
            temp.transform.parent = AllyManager.Instance.transform;
        }

       

        GameObject temp1 = Instantiate(enemyBlock, tempEnemyLocation.position + offset, Quaternion.identity);
        temp1.transform.parent = AllyManager.Instance.transform;

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




    private void RemoveNodesWithoutCertainAmountOfNeighbours(List<Node> nodes)
    {
        //remove non certain amount of neighboured tiles from list
        //we dont do this before so the algorithm can still find nodes possible to reach through tight spaces
        for (int i = 0; i < nodes.Count;)
        {
            if (nodes[i].neighbours.Count < 6)
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
            nodesToSearch[0].Reset();
            nodesToSearch.RemoveAt(0);

            if (nodesToSearch.Count > 1)
            {
                List<Node> nodesToRemove = GetNodesToRemove(nodesToSearch, nodesOfInterest[nodesOfInterest.Count - 1]);

                for (int i = 0; i < nodesToRemove.Count; i++)
                {
                    nodesToSearch.Remove(nodesToRemove[i]);
                    nodesToRemove[i].Reset();
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
            if (IsNodeInRange(rangeNode, nodes[i]))
            {
                removeTheseNodes.Add(nodes[i]);
            }
        }

        return removeTheseNodes;
    }



    private int CalculateInitialCost(Vector3 pos1, Vector3 pos2)
    {
        Vector3 cost = pos2 - pos1;
        return (int)(cost.x + cost.y) * 10;
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



    public Node GetClosestScoutNode(Vector3 pos)
    {
        if(nodesToScout.Count <= 0)
        {
            return null;
        }
        //used to get centre of a node
        Vector2 offsetVector = new Vector2(0.5f, 0.5f);
        Vector2 agentPosition = new Vector2(leadScout.transform.position.x, leadScout.transform.position.y);

        Node closestNode = nodesToScout[0];
        float closestDistance = Vector2.SqrMagnitude((nodesToScout[0].position + offsetVector) - agentPosition);

        int currentIndex = 0;


        for (int i = 1; i < nodesToScout.Count; i++)
        {
            float distance = Vector2.SqrMagnitude((nodesToScout[i].position + offsetVector) - agentPosition);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestNode = nodesToScout[i];
                currentIndex = i;
            }
        }

        return closestNode;
    }





    public void RemoveScoutedNode(Node node)
    {
        int index = nodesToScout.FindIndex(n => n == node);
        Destroy(AllyManager.Instance.transform.GetChild(index).gameObject);
        nodesToScout.Remove(node);
    }
}
