using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;


//node class that stores all the information for a single node to be used for pathfinding
public class Node : IComparable<Node>
{
    public Vector2 position { get; private set; }
    public Map.Terrain terrain { get; private set; }

    public List<Node> neighbours { get; private set; }
    public Node parent { get; private set; }

    public int visitOrder;

    public bool onOpenList = false;
    public bool onClosedList = false;

    //heuristics
    public int f = 0;
    public int g = 0;
    public int h = 0;

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


    public float SqrMagnitude(Node other)
    {
        return Vector3.SqrMagnitude(this.position - other.position);
    }

    public void Reset()
    {
        parent = null;
        visitOrder = 0;
        onOpenList = false;
        onClosedList = false;
        f = 0;
        g = 0;
        h = 0;

    }

    public int CompareTo(Node other)
    {
        if(f < other.f)
        {
            return -1;
        }
        else if(f > other.f)
        {
            return 1;
        }
        return 0;
    }


}
