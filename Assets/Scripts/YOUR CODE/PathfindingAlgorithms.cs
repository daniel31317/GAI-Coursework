using System.Collections.Generic;
using UnityEngine;

public static class PathfindingAlgorithms 
{
    public const int movementXCost = 10;
    public const int movementYCost = 10;
    public const int movementDiagonalCost = 14;
    private static List<Node> nodesToReset = new List<Node>();

    public static List<Node> AStar(Node startNode, Node endNode)
    {
        ResetNodesToDefaualt(nodesToReset);

        int visitOrder = 0;
        List<Node> openNodeList = new List<Node>();
        

        startNode.onOpenList = true;

        openNodeList.Add(startNode);
        nodesToReset.Add(startNode);

        while (openNodeList.Count > 0)
        {
            //bubble sort from lowest to biggest f
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

            //openNodeList[0] = currentNode
            openNodeList[0].onOpenList = false;
            openNodeList[0].onClosedList = true;
            openNodeList[0].visitOrder = visitOrder++;

            if (openNodeList[0] == endNode)
            {          
                return GetFoundPath(endNode);
            }

            foreach (Node childNode in openNodeList[0].neighbours)
            {
                if (!childNode.onClosedList)
                {
                    childNode.g = openNodeList[0].g + CalculateInitialCost(openNodeList[0].position, childNode.position);
                    childNode.h = ManhattanDistanceHeuristic(childNode.position, endNode.position);
                    int f = childNode.g + childNode.h;

                    if (f <= childNode.f || !childNode.onOpenList)
                    {
                        childNode.f = f;
                    }
                    if (!childNode.onOpenList)
                    {
                        childNode.SetParent(openNodeList[0]);
                        childNode.onOpenList = true;
                        openNodeList.Add(childNode);
                        nodesToReset.Add(childNode);
                    }
                }
            }

            openNodeList.RemoveAt(0);
        }

        return GetFoundPath(null);
    }

    public static void ResetNodesToDefaualt(List<Node> nodes)
    {
        foreach(Node node in nodes)
        {
            node.Reset();
        }
        nodes.Clear();
    }


    //from practical earlier in the year
    private static List<Node> GetFoundPath(Node endNode)
    {
        List<Node> foundPath = new List<Node>();
        if (endNode != null)
        {
            foundPath.Add(endNode);

            while (endNode.parent != null)
            {
                foundPath.Add(endNode.parent);
                endNode = endNode.parent;
            }

            // Reverse the path so the start node is at index 0
            foundPath.Reverse();
        }
        return foundPath;
    }


    public static int CalculateInitialCost(Vector2 firstNodePos, Vector2 secondNodePos)
    {
        int xCost = (int)Mathf.Abs(secondNodePos.x - firstNodePos.x);
        int yCost = (int)Mathf.Abs(secondNodePos.y - firstNodePos.y);
        if ((xCost + yCost) < 2)
        {
            if (xCost > 0)
            {
                return movementXCost;
            }
            return movementYCost;
        }
        return movementDiagonalCost;
    }

    private static int ManhattanDistanceHeuristic(Vector2 currentPos, Vector2 targetPos)
    {
        return (int)((Mathf.Abs(currentPos.x - targetPos.x) * movementXCost) + (Mathf.Abs(currentPos.y - targetPos.y) * movementYCost));
    }
}
