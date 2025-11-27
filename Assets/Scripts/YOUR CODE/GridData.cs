using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GridData : MonoBehaviour
{
    public static GridData Instance;

    public Node[,] Data { get; private set; } = new Node[100, 100];


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }

        //initialise all grid nodes with position and terrain
        for (int i = 0; i < Data.GetLength(0); i++)
        {
            for (int j = 0; j < Data.GetLength(1); j++)
            {
                Data[i, j] = new Node();
            }
        }
    }


    void Start()
    {
        //initialise all grid nodes with position and terrain
        for(int i = 0; i < Data.GetLength(0); i++)
        {
            for(int j = 0; j < Data.GetLength(1); j++)
            {
                InitialiseGridNode(Data[i, j], i, j);
            }
        }
        
        
        for(int i = 0; i < Data.GetLength(0); i++)
        {
            for(int j = 0; j < Data.GetLength(1); j++)
            {
                Data[i, j].SetNeighbours(GetNeighboursOfNode(i, j));
            }
        }

        AllyManager.Instance.AssignRoles();
        AllyManager.ScoutManager.InitialiseScoutPositions();
    }

    private void InitialiseGridNode(Node node, int x, int y)
    {
        node.SetPosition(new Vector2(x, y));
        node.SetTerrain(GameData.Instance.Map.GetTerrainAt(x, y));
    }

    private List<Node> GetNeighboursOfNode(int x, int y)
    {
        List<Node> nodes = new List<Node>();

        //loop through the neghbouring nodes 
        for(int i = -1; i <= 1 ; i++)
        {
            for(int j = -1; j <= 1; j++)
            {
                if(HandleContinueConditions(i, j, x, y))
                {
                    continue;
                }
                nodes.Add(Data[x + i, y + j]);
            }
        }
        
        return nodes;
    }



    private bool HandleContinueConditions(int i, int j, int x, int y)
    {
        //if i and j are zero we do not add this node since this is the current node
        if (i == 0 && j == 0)
        {
            return true;
        }


        //make sure neighbour is in map
        if(i + x < 0 || i + x > 99)
        {
            return true;
        }
        
        
        if(j + y < 0 || j + y > 99)
        {
            return true;
        }

        if (Data[x +  i, y + j].terrain == Map.Terrain.Tree)
        {
            return true;
        }



        return false;
    }



    public Node GetNodeAt(int x, int y)
    {
        if(x < 0 || x > 99 || y < 0 || y > 99)
        {
            Debug.LogWarning("Trying to access a node out of range at x : " + x + ", y : " + y);
            return null;
        }
        return Data[x, y];
    }
    
    //ignoring z axis
    public Node GetNodeAt(Vector3 position)
    {
        if((int)position.x < 0 || (int)position.x > 99 || (int)position.y < 0 || (int)position.y > 99)
        {
            Debug.LogWarning("Trying to access a node out of range at x : " + position.x + ", y : " + position.y);
            return null;
        }
        return Data[(int)position.x, (int)position.y];
    }

}
