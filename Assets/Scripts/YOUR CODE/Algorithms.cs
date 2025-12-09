using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

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
        endNode.Reset();

        int visitOrder = 0;
        List<Node> openNodeList = new List<Node>();
        

        startNode.onOpenList = true;

        openNodeList.Add(startNode);
        nodesToReset.Add(startNode);

        while (openNodeList.Count > 0)
        {
            openNodeList.Sort();

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
                    int g = openNodeList[0].g + CalculateInitialCost(openNodeList[0].position, childNode.position);
                    int h = ManhattanDistanceHeuristic(childNode.position, endNode.position);
                    int f = g + h;

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
                        childNode.g = g;
                        childNode.SetParent(openNodeList[0]);
                    }
                    if (!childNode.onOpenList)
                    {
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
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].Reset();
        }
        nodes.Clear();
    }

    #region from practical earlier in the year
    private static List<Node> GetFoundPath(Node endNode)
    {
        if(endNode == null)
        {
            return null;
        }


        Node initialNode = endNode;
        int expectedPathLength = 0;
        while (endNode.parent != null)
        {
            endNode = endNode.parent;
            expectedPathLength++;
        }

        List<Node> foundPath = new List<Node>(expectedPathLength);
        
        endNode = initialNode;

        foundPath.Add(endNode);

        while (endNode.parent != null)
        {
            foundPath.Add(endNode.parent);
            endNode = endNode.parent;
        }

        // Reverse the path so the start node is at index 0
        foundPath.Reverse();
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

    //the following algorithm is from 
    //https://playtechs.blogspot.com/2007/03/raytracing-on-grid.html
    //but has been adapted


    //returns a list of nodes beyween two points based on a line drawn between those two points
    private static List<Node> RayTrace(Vector2 startPos, Vector2 endPos)
    {
        List<Node> rayTracedNodes = new List<Node>();

        //holds the differnece in positions
        float dx = Mathf.Abs(startPos.x - endPos.x);
        float dy = Mathf.Abs(startPos.y - endPos.y);

        //startPos
        int x = Mathf.FloorToInt(startPos.x);
        int y = Mathf.FloorToInt(startPos.y);

        //start n at 1 because we cant loop for zero grid tiles
        int n = 1;
        int x_inc, y_inc;
        float error;

        //so we avoid divide by zero cases
        if (dx == 0)
        {
            x_inc = 0;
            error = Mathf.Infinity;
        }
        //these set the x incriment to the endPos
        //then gets the number of grid tiles between positions
        //error determines wether we start moving horizontally or not 
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

        //so we avoid divide by zero cases
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


        //loop over number of n moves
        for (; n > 0; --n)
        {
            //add a node that we have looped over
            if(x > -1 && y > -1)
            {
                rayTracedNodes.Add(GridData.Instance.GetNodeAt(new Vector3(x, y, 0)));
            }
            
            //if error is positive we move in the y direction else move in x direction
            //both based on incriments determined earlier
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
            if(nodes[i] == null)
            {
                continue;
            }
            if (nodes[i].terrain == Map.Terrain.Tree)
            {
                return false;
            }
        }
        return true;

    }


    #endregion

    #region scoring nodes

    public static List<Node> ScoreAllAccessibleNodes(Node startNode)
    {
        ResetNodesToDefaualt(nodesToReset);

        List<Node> closedList = new List<Node>();
        List<Node> openList = new List<Node>();

        startNode.onOpenList = true;
        openList.Add(startNode);
        nodesToReset.Add(startNode);

        //score every single possible grid we can go to using a scoring system simlar to dijsktra but wiht no sorting
        while (openList.Count > 0)
        {
            Node currentNode = openList[0];
            openList.RemoveAt(0);

            currentNode.onOpenList = false;
            currentNode.onClosedList = true;
            closedList.Add(currentNode);
            nodesToReset.Add(currentNode);


            for(int i = 0; i < currentNode.neighbours.Count; i++)
            {
                if (!currentNode.neighbours[i].onClosedList)
                {
                    int f = currentNode.f + CalculateInitialCostNoHorizontal(currentNode.position, currentNode.neighbours[i].position);
                    if (f <= currentNode.neighbours[i].f || !currentNode.neighbours[i].onOpenList)
                    {
                        currentNode.neighbours[i].f = f;
                    }
                    if (!currentNode.neighbours[i].onOpenList)
                    {
                        currentNode.neighbours[i].SetParent(currentNode);
                        currentNode.neighbours[i].onOpenList = true;
                        openList.Add(currentNode.neighbours[i]);
                        nodesToReset.Add(currentNode.neighbours[i]);
                    }
                }
            }
        }

        return closedList;
    }

    public static List<Node> FindAllAccesableNodes(Node startNode, int minDistance)
    {
        ResetNodesToDefaualt(nodesToReset);

        int minDistanceSqr = minDistance * minDistance;

        List<Node> closedList = new List<Node>();
        List<Node> openList = new List<Node>();

        startNode.onOpenList = true;
        openList.Add(startNode);

        //score every single possible grid we can go to using a scoring system simlar to dijsktra but wiht no sorting
        while (openList.Count > 0)
        {
            Node currentNode = openList[0];
            openList.RemoveAt(0);

            currentNode.onOpenList = false;
            currentNode.onClosedList = true;

            float distanceSqrd = Vector3.SqrMagnitude(startNode.position - currentNode.position);
            if (distanceSqrd >= minDistanceSqr)
            {
                closedList.Add(currentNode);
            }

            nodesToReset.Add(currentNode);

            //if we have more than 1000 nodes to look at that is probably more than enough
            //just better for later steps not to loop though for example 8000 nodes where most of them will be useless
            if (closedList.Count > 1000)
            {
                return closedList;
            }

            for (int i = 0; i < currentNode.neighbours.Count; i++)
            {
                if (!currentNode.neighbours[i].onClosedList && !currentNode.neighbours[i].onOpenList)
                {
                    currentNode.neighbours[i].SetParent(currentNode);
                    currentNode.neighbours[i].onOpenList = true;
                    openList.Add(currentNode.neighbours[i]);
                }
            }
        }

        return closedList;
    }

    private static int CalculateInitialCostNoHorizontal(Vector3 pos1, Vector3 pos2)
    {
        Vector3 cost = pos2 - pos1;
        return (int)(cost.x + cost.y) * 10;
    }

    //get points of interest and remove anthing within a certain distance
    public static List<Node> GetNodesOfInterest(List<Node> nodesToSearch, int removeNodesInDistance)
    {
        List<Node> nodesOfInterest = new List<Node>();
        while (nodesToSearch.Count > 0)
        {
            nodesOfInterest.Add(nodesToSearch[0]);
            nodesToSearch[0].Reset();
            nodesToSearch.RemoveAt(0);

            if (nodesToSearch.Count > 1)
            {
                //remove nodes within a certain distance
                List<Node> nodesToRemove = GetNodesToRemove(nodesToSearch, nodesOfInterest[nodesOfInterest.Count - 1], removeNodesInDistance);

                for (int i = 0; i < nodesToRemove.Count; i++)
                {
                    nodesToSearch.Remove(nodesToRemove[i]);
                    nodesToRemove[i].Reset();
                }
            }
        }

        return nodesOfInterest;
    }


    public static NodesOfInterest GetNodesOfInterest(List<Node> nodesToSearch, int removeNodesInDistance, Vector2 losPos, float distance)
    {
        NodesOfInterest nodes = new NodesOfInterest(new List<Node>(), new List<Node>());

        float distanceSqr = distance * distance;

        while (nodesToSearch.Count > 0)
        {
            if(Vector2.SqrMagnitude(nodesToSearch[0].position - losPos) < distanceSqr)
            {
                nodesToSearch[0].Reset();
                nodesToSearch.RemoveAt(0);
                continue;
            }

            if(IsPositionInLineOfSight(nodesToSearch[0].position, losPos))
            {
                nodes.nodesInLos.Add(nodesToSearch[0]);
            }
            else
            {
                nodes.nodesNotLos.Add(nodesToSearch[0]);
            }

            nodesToReset.Add(nodesToSearch[0]);

            if (nodesToSearch.Count > 0)
            {
                Node referenceNode = nodesToSearch[0];
                nodesToSearch.RemoveAll(n => IsNodeInRange(referenceNode, n, removeNodesInDistance));
            }

            if(nodes.nodesInLos.Count > 100)
            {
                return nodes;
            }
        }

        return nodes;
    }



    //gets the nodes needed to be removed from the closed list based on distance to an important node
    private static List<Node> GetNodesToRemove(List<Node> nodes, Node rangeNode, int distance)
    {
        List<Node> removeTheseNodes = new List<Node>();
        //loop thorugh closed list with current point of interest and anything out of distance add to remove list
        for (int i = 0; i < nodes.Count; i++)
        {
            if(nodes[i] == rangeNode)
            {
                continue;
            }

            if (IsNodeInRange(rangeNode, nodes[i], distance))
            {
                removeTheseNodes.Add(nodes[i]);
            }
        }

        return removeTheseNodes;
    }




    private static bool IsNodeInRange(Node originNode, Node otherNode, int distance)
    {
        Vector2 posDiff = originNode.position - otherNode.position;
        if (posDiff.sqrMagnitude <= distance * distance)
        {
            return true;
        }
        return false;
    }

    #endregion

    #region other
    public static Vector3 GenerateNewTargetPosWithOffset(Node node)
    {
        Vector3 newTargetPos = new Vector3(node.position.x, node.position.y, 0f);
        newTargetPos.x += Random.Range(0f, 1f);
        newTargetPos.y += Random.Range(0f, 1f);
        return newTargetPos;
    }

    public static EnemyAgent GetClosestEnemyInLos(Vector3 position)
    {
        EnemyAgent closestEnemy = null;
        float closestDistance = 10000000;

        for (int i = 0; i < GameData.Instance.enemies.Count; i++)
        {
            if (!GameData.Instance.enemies[i].gameObject.activeSelf)
            {
                continue;
            }
            float newDistance = Vector3.SqrMagnitude(position - GameData.Instance.enemies[i].transform.position);

            if (IsPositionInLineOfSight(position, GameData.Instance.enemies[i].transform.position) && newDistance < closestDistance)
            {
                closestEnemy = (EnemyAgent)GameData.Instance.enemies[i];
                closestDistance = newDistance;
            }
        }

        return closestEnemy;
    }



    #endregion




    #region steering behaviours
    public static Vector2 CalcualteObstacleAvoidanceForce(Vector3 currentPosition)
    {
        Vector2 totalForce = Vector2.zero;
        Node currentNode = GridData.Instance.GetNodeAt(currentPosition);

        if (currentNode.neighbours.Count == 8)
        {
            return Vector2.zero;
        }

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2 checkPos = currentNode.position + new Vector2(x, y);

                if(checkPos.x < 0 || checkPos.y < 0 || checkPos.x >= 100 || checkPos.y >= 100)
                {
                    totalForce += (Vector2)currentPosition - (currentNode.position + new Vector2(x, y));
                    continue;
                }

                if (GameData.Instance.Map.GetTerrainAt((int)currentNode.position.x + x, (int)currentNode.position.y + y) == Map.Terrain.Tree)
                {
                    totalForce += (Vector2)currentPosition - (currentNode.position + new Vector2(x, y));
                }

            }
        }

        return totalForce * SteeringAgent.MaxCurrentSpeed;
    }


    public static Vector2 CalcualteSeperationForce(GameObject currentAgent)
    {
        Vector2 totalForce = Vector2.zero;
        int amountOfAlliesNearby = 0;

        for (int i = 0; i < AllyManager.Instance.m_agents.Count; i++)
        {
            if (AllyManager.Instance.m_agents[i].gameObject != currentAgent)
            {
                float distSqr = Vector2.SqrMagnitude(currentAgent.transform.position - AllyManager.Instance.m_agents[i].transform.position);

                if (distSqr <= 4 && distSqr > 0)
                {
                    Vector2 pushForce = currentAgent.transform.position - AllyManager.Instance.m_agents[i].transform.position;

                    totalForce += pushForce;
                    amountOfAlliesNearby++;
                }

            }
        }

        //so we dont get divide by zero errors on line after if statement
        if (amountOfAlliesNearby == 0)
        {
            return totalForce;
        }

        totalForce /= amountOfAlliesNearby;

        return totalForce * SteeringAgent.MaxCurrentSpeed;
    }

    #endregion
}
