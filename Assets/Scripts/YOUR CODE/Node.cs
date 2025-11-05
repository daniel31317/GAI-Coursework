using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Vector2 position { get; private set; }
    public Map.Terrain terrain { get; private set; }

    public List<Node> neighbours { get; private set; }
    public Node parent { get; private set; }

    public bool onOpenList = false;
    public bool onClosedList = false;

    public int f;
    public int g;
    public int h;

    public void SetPosition(Vector2 position)
    {
        this.position = position;
    }
    
    public void SetTerrain(Map.Terrain terrain)
    {
        this.terrain = terrain;
    }
    
    public void SetNeighbours(List<Node> neighbours)
    {
        this.neighbours = neighbours;
    }
    
    public void AddNeighbour(Node node)
    {
        neighbours.Add(node);
    }

    public void SetParent(Node parent)
    {
        this.parent = parent;
    }

    

}
