using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ScoutManager : ScriptableObject
{
    private AllyAgent leadScout;
    private AllyAgent followScout;
    public List<Node> nodesToScout { get; private set; }
    
    public void InitialiseScoutPositions()
    {
        ScoutPositionToCheck(leadScout.transform.position);
    }


    public void SetAgentAsScoutLead(AllyAgent newAgent)
    {
        leadScout = newAgent;
        leadScout.SwitchAgentRole(AllyAgentRole.LeadScout, null);
        leadScout.scoutLeader.ResetScout();
    }

    public void SetAgentAsScoutFollower(AllyAgent newAgent)
    {
        followScout = newAgent;
        followScout.SwitchAgentRole(AllyAgentRole.FollowLeader, leadScout);
        leadScout.scoutLeader.SetScoutFollower(followScout.followLeader);
        followScout.followLeader.SetAvoidAllies(false);
    }


    //gets the scout positions to check around based on a starting position
    private void ScoutPositionToCheck(Vector3 pos)
    {
        //keeps track of the nodes to check
        List<Node> closedNodeList = new List<Node>();

        Node startNode = GridData.Instance.GetNodeAt(pos);

        //score all accessible nodes from start node
        closedNodeList = Algorithms.ScoreAllAccessibleNodes(startNode);

        //remove nodes without certain amount of neighbours so it tries to avoid spaces with mots of obstacles
        RemoveNodesWithoutCertainAmountOfNeighbours(closedNodeList);

        //scort the list by to be furthest to closest
        SortNodeListByDescendingF(closedNodeList);

        //get points of interest and remove anthing within +/- 5 on x and y
        nodesToScout = Algorithms.GetNodesOfInterest(closedNodeList, 15);
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



    //sort list from highest f score to smallest
    private void SortNodeListByDescendingF(List<Node> list)
    {
        //this sorts it by ascending order so we reverse it after
        list.Sort();
        list.Reverse();
    }



    //returns the closest node for the lead scout to go to
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

        //sort through all nodes to find closest
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
        nodesToScout.Remove(node);
    }
}
