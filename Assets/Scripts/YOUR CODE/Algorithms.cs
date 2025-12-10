using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public static class Algorithms 
{
    private const int movementXCost = 10;
    private const int movementYCost = 10;
    private const int movementDiagonalCost = 14;
    private static List<Node> nodesToReset = new List<Node>();

    #region pathfinding

    //from pseudocode in powerpoint on pathfinding
    public static List<Node> AStar(Node startNode, Node endNode)
    {
        //reset all nodes to default so they dont interfere with pathfinding
        ResetNodesToDefaualt();

        //if the end node is a tree find the nearest non tree node
        if (endNode.terrain == Map.Terrain.Tree)
        {
            List<Node> neighbours = GridData.Instance.GetNeighbouringNodesAt(endNode.position);
            for (int i = 0; i < neighbours.Count; i++)
            {
                if (neighbours[i].terrain != Map.Terrain.Tree)
                {
                    endNode = neighbours[i];
                    break;
                }
            }
        }

        int visitOrder = 0;
        List<Node> openNodeList = new List<Node>();
        
        startNode.onOpenList = true;

        openNodeList.Add(startNode);

        while (openNodeList.Count > 0)
        {
            openNodeList.Sort();

            //openNodeList[0] = currentNode
            openNodeList[0].onOpenList = false;
            openNodeList[0].onClosedList = true;
            openNodeList[0].visitOrder = visitOrder++;

            //if we have reached the end node return the found path
            if (openNodeList[0] == endNode)
            {
                return GetFoundPath(endNode);
            }

            //handle the neighbours of the current node
            for (int i = 0; i < openNodeList[0].neighbours.Count; i++)
            {
                Node childNode = openNodeList[0].neighbours[i];
                //if the child node is not on the closed list give it a f score and add it to the open list if it isnt already there
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
                    }
                }
            }

            openNodeList.RemoveAt(0);
        }

        //if no path found return a null path
        return GetFoundPath(null);
    }


    //resets all node to default
    public static void ResetNodesToDefaualt()
    {
        for (int i = 0; i < nodesToReset.Count; i++)
        {
            nodesToReset[i].Reset();
        }
    }

    #region from practical earlier in the year


    //get found path based on the parents of the nodes found
    private static List<Node> GetFoundPath(Node endNode)
    {
        //if no end node return null
        if (endNode == null)
        {
            return null;
        }

        List<Node> foundPath = new List<Node>();      

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


    //calculate the cost of moving between two nodes
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


    //manhattan distance heuristic for a grid based system
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



    //checks wether the end point is in line of sight of the start point
    public static bool IsPositionInLineOfSight(Vector2 startPos, Vector2 endPos)
    {
        //ratraces between the tow points and gets the nodes inbetween
        List<Node> nodes = RayTrace(startPos, endPos);

        //if any of the nodes between the two points are trees return false as in no los
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


    //scores all accessible nodes from a start node and returns them in a list
    //kind of like dijkstra minus the sorting part
    public static List<Node> ScoreAllAccessibleNodes(Node startNode)
    {
        List<Node> closedList = new List<Node>();
        List<Node> openList = new List<Node>();

        startNode.onOpenList = true;
        openList.Add(startNode);

        //score every single possible grid we can go to using a scoring system simlar to dijsktra but wiht no sorting
        while (openList.Count > 0)
        {
            Node currentNode = openList[0];
            openList.RemoveAt(0);

            //add it to the closed list
            currentNode.onOpenList = false;
            currentNode.onClosedList = true;
            closedList.Add(currentNode);

            //this is how we find all the nodes we can visit so we add them to a list to reset later
            nodesToReset.Add(currentNode);

            //add and score all neighbours based on cost from start node to the open list
            for (int i = 0; i < currentNode.neighbours.Count; i++)
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
                    }
                }
            }
        }

        return closedList;
    }


    //calculate the cost of moving between two nodes without diagonal movement so just difference in x and y * 10
    private static int CalculateInitialCostNoHorizontal(Vector3 pos1, Vector3 pos2)
    {
        Vector3 cost = pos2 - pos1;
        return (int)(cost.x + cost.y) * movementXCost;
    }

    //get points of interest and remove anthing within a certain distance
    public static List<Node> GetNodesOfInterest(List<Node> nodesToSearch, int removeNodesInDistance)
    {
        List<Node> nodesOfInterest = new List<Node>();

        //loop until no nodes left to search
        while (nodesToSearch.Count > 0)
        {
            //add the first node as a point of interest
            nodesOfInterest.Add(nodesToSearch[0]);
            nodesToSearch[0].Reset();
            nodesToSearch.RemoveAt(0);

            //if there is another node to compare distance to
            if (nodesToSearch.Count > 0)
            {
                //remove nodes within a certain distance
                List<Node> nodesToRemove = GetNodesToRemove(nodesToSearch, nodesOfInterest[nodesOfInterest.Count - 1], removeNodesInDistance);

                //remove those nodes
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



    //does a distancesqr cjeck to see if two nodes are within a certain distance
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


    //takes in a node and adds a random offset to x and y to make the movemnet seem less grid like
    public static Vector3 GenerateNewTargetPosWithOffset(Node node)
    {
        Vector3 newTargetPos = new Vector3(node.position.x, node.position.y, 0f);
        newTargetPos.x += Random.Range(0f, 1f);
        newTargetPos.y += Random.Range(0f, 1f);
        return newTargetPos;
    }


    //returns the closeest enemy in line of sight to a given position
    public static EnemyAgent GetClosestEnemyInLos(Vector3 position)
    {
        EnemyAgent closestEnemy = null;
        float closestDistance = 0;

        for (int i = 0; i < GameData.Instance.enemies.Count; i++)
        {
            //if the enemy is inactive skip it
            if (!GameData.Instance.enemies[i].gameObject.activeSelf)
            {
                continue;
            }
            float newDistance = Vector3.SqrMagnitude(position - GameData.Instance.enemies[i].transform.position);

            if (IsPositionInLineOfSight(position, GameData.Instance.enemies[i].transform.position) && (newDistance < closestDistance || closestEnemy == null))
            {
                closestEnemy = (EnemyAgent)GameData.Instance.enemies[i];
                closestDistance = newDistance;
            }
        }

        return closestEnemy;
    }



    #endregion




    #region steering behaviours

    //calculates a force to avoid obstacles based on nearby trees
    //https://www.red3d.com/cwr/papers/1999/gdc99steer.pdf
    public static Vector2 CalcualteObstacleAvoidanceForce(Vector3 currentPosition)
    {
        Vector2 totalForce = Vector2.zero;
        Node currentNode = GridData.Instance.GetNodeAt(currentPosition);

        if (currentNode.neighbours.Count == 8)
        {
            return Vector2.zero;
        }

        //loop thrugh all surrounding nodes
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2 checkPos = currentNode.position + new Vector2(x, y);

                //if that position is out of bounds add a force away from it
                if (checkPos.x < 0 || checkPos.y < 0 || checkPos.x >= 100 || checkPos.y >= 100)
                {
                    totalForce += (Vector2)currentPosition - (currentNode.position + new Vector2(x, y));
                    continue;
                }

                //also if there is a tree there add a force away from it
                if (GameData.Instance.Map.GetTerrainAt((int)currentNode.position.x + x, (int)currentNode.position.y + y) == Map.Terrain.Tree)
                {
                    totalForce += (Vector2)currentPosition - (currentNode.position + new Vector2(x, y));
                }

            }
        }

        //return the total force scaled by max speed so it has a little more impact
        return totalForce * SteeringAgent.MaxCurrentSpeed;
    }


    //returns a foce to seperate from nearby allied agents
    //https://www.red3d.com/cwr/papers/1999/gdc99steer.pdf
    public static Vector2 CalcualteSeperationForce(GameObject currentAgent)
    {
        Vector2 totalForce = Vector2.zero;
        int amountOfAlliesNearby = 0;

        //loop through all allied agents
        for (int i = 0; i < AllyManager.Instance.m_agents.Count; i++)
        {
            //if they are not the current agent
            if (AllyManager.Instance.m_agents[i].gameObject != currentAgent)
            {

                float distSqr = Vector2.SqrMagnitude(currentAgent.transform.position - AllyManager.Instance.m_agents[i].transform.position);

                //if within certain distance add a force away from them
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

        //again multiply by max speed so it has more impact
        return totalForce * SteeringAgent.MaxCurrentSpeed;
    }

    #endregion
}
