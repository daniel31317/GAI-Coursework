using System.Collections.Generic;
using UnityEngine;

public static class Algorithms 
{
    public const int movementXCost = 10;
    public const int movementYCost = 10;
    public const int movementDiagonalCost = 14;
    private static List<Node> nodesToReset = new List<Node>();

    #region pathfinding
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

            for (int i = 0; i < openNodeList[0].neighbours.Count; i++)
            {
                Node childNode = openNodeList[0].neighbours[i];
                if (!childNode.onClosedList)
                {
                    childNode.g = openNodeList[0].g + CalculateInitialCost(openNodeList[0].position, childNode.position);
                    childNode.h = ManhattanDistanceHeuristic(childNode.position, endNode.position);
                    int f = childNode.g + childNode.h;

                    if (childNode.terrain == Map.Terrain.Mud)
                    {
                        f *= 2;
                    }
                    else if (childNode.terrain == Map.Terrain.Water)
                    {
                        f *= 4;
                    }

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
        for(int i = 0; i < nodes.Count; i++)
        {
            nodes[i].Reset();
        }
        nodes.Clear();
    }

    #region from practical earlier in the year
    private static List<Node> GetFoundPath(Node endNode)
    {
        Node initialNode = endNode;
        int expectedPathLength = 0;
        while (endNode.parent != null)
        {
            if (endNode.parent.parent == endNode)
            {
                break;
            }
            endNode = endNode.parent;
            expectedPathLength++;
        }

        List<Node> foundPath = new List<Node>(expectedPathLength);
        
        endNode = initialNode;

        if (endNode != null)
        {
            foundPath.Add(endNode);

            while (endNode.parent != null)
            {
                if (endNode.parent.parent == endNode)
                {
                    break;
                }
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
    #endregion

    #endregion

    #region line of sight
    // the following algorithm is from 
    //https://playtechs.blogspot.com/2007/03/raytracing-on-grid.html
    //but has been adapted

    public static List<Node> RayTrace(Vector2 startPos, Vector2 endPos)
    {
        List<Node> rayTracedNodes = new List<Node>();

        double dx = Mathf.Abs(startPos.x - endPos.x);
        double dy = Mathf.Abs(startPos.y - endPos.y);

        int x = Mathf.FloorToInt(startPos.x);
        int y = Mathf.FloorToInt(startPos.y);

        int n = 1;
        int x_inc, y_inc;
        double error;

        if (dx == 0)
        {
            x_inc = 0;
            error = Mathf.Infinity;
        }
        else if (endPos.x > startPos.x)
        {
            x_inc = 1;
            n += Mathf.FloorToInt(endPos.x) - x;
            error = (Mathf.Floor(startPos.x) + 1 - startPos.x) * dy;
        }
        else
        {
            x_inc = -1;
            n += x - Mathf.FloorToInt(endPos.x);
            error = (startPos.x - Mathf.Floor(startPos.x)) * dy;
        }

        if (dy == 0)
        {
            y_inc = 0;
            error -= Mathf.Infinity;
        }
        else if (endPos.y > startPos.y)
        {
            y_inc = 1;
            n += Mathf.FloorToInt(endPos.y) - y;
            error -= (Mathf.Floor(startPos.y) + 1 - startPos.y) * dx;
        }
        else
        {
            y_inc = -1;
            n += y - Mathf.FloorToInt(endPos.y);
            error -= (startPos.y - Mathf.Floor(startPos.y)) * dx;
        }

        for (; n > 0; --n)
        {
            rayTracedNodes.Add(GridData.Instance.GetNodeAt(new Vector3(x, y, 0)));

            if (error > 0)
            {
                y += y_inc;
                error -= dx;
            }
            else
            {
                x += x_inc;
                error += dy;
            }
        }

        return rayTracedNodes;
    }

    public static bool IsPositionInLineOfSight(Vector2 startPos, Vector2 endPos)
    {
        List<Node> nodes = RayTrace(startPos, endPos);
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].terrain == Map.Terrain.Tree)
            {
                return false;
            }
        }
        return true;

    }
        #endregion
    }
