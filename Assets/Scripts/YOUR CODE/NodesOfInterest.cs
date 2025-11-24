using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public struct NodesOfInterest
{
    public List<Node> nodesInLos;
    public List<Node> nodesNotLos;

    public NodesOfInterest(List<Node> los, List<Node> notLos)
    {
        nodesInLos = los;
        nodesNotLos = notLos;
    }
}
