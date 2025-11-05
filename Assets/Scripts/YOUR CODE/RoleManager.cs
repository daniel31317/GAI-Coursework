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

        while (openNodeList.Count > 0)
        {
            //sort list
            for (int i = 0; i < openNodeList.Count - 1; i++)
            {
                for (int j = 0; j < openNodeList.Count - 1 - i; j++)
                {
                    if (openNodeList[j].f > openNodeList[j + 1].f)
                    {
                        Node temp = openNodeList[j];
                        openNodeList[j] = openNodeList[j + 1];
                        openNodeList[j + 1] = temp;
                    }
                }
            }

            Node currentNode = openNodeList[0];
            openNodeList.RemoveAt(0);

            currentNode.onOpenList = false;
            currentNode.onClosedList = true;
            closedNodeList.Add(currentNode);


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
                        openNodeList.Add(childNode);
                    }
                }
            }
        }



        for(int i = 0; i < closedNodeList.Count;)
        {
            if (closedNodeList[i].neighbours.Count != 8)
            {
                closedNodeList.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }



        //sort closed list
        for (int i = 0; i < closedNodeList.Count - 1; i++)
        {
            for (int j = 0; j < closedNodeList.Count - 1 - i; j++)
            {
                if (closedNodeList[j].f < closedNodeList[j + 1].f)
                {
                    Node temp = closedNodeList[j];
                    closedNodeList[j] = closedNodeList[j + 1];
                    closedNodeList[j + 1] = temp;
                }
            }
        }


        //get points of interest and remove anthing within +/- 5 on x and y
        List<Node> pointOfInterest = new List<Node>();

        if(closedNodeList.Count > 0)
        {
            while (closedNodeList.Count > 0)
            {
                pointOfInterest.Add(closedNodeList[0]);
                closedNodeList.RemoveAt(0);

                List<Node> nodesToRemove = new List<Node>();    

                if(closedNodeList.Count > 1)
                {
                    for (int i = 0; i < closedNodeList.Count; i++)
                    {
                        if (IsNodeInRange(pointOfInterest[pointOfInterest.Count - 1], closedNodeList[i]))
                        {
                            nodesToRemove.Add(closedNodeList[i]);
                        }

                    }

                    for(int i = 0;i < nodesToRemove.Count; i++)
                    {
                        closedNodeList.Remove(nodesToRemove[i]);
                    }
                }    
            }
        }

        Vector2 offset = new Vector2(0.5f, 0.5f);

        for(int i = 0; i < pointOfInterest.Count; i++)
        {
            Instantiate(testBlock, pointOfInterest[i].position + offset, Quaternion.identity);
        }



        return pointOfInterest;
    }


    public bool IsNodeInRange(Node originNode, Node otherNode)
    {
        Vector2 posDiff = originNode.position - otherNode.position;

        if(Mathf.Abs(posDiff.x) * Mathf.Abs(posDiff.y) <= 144)
        {
            return true;
        }
        return false;
    }




    public int CalculateInitialCost(Vector3 pos1, Vector3 pos2)
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
}
