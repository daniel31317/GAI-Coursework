using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public struct NodesOfInterest
{
    public List<Node> nodesInLosAndRange;
    public List<Node> nodesInLos;
    public List<Node> nodesInNeither;

    public NodesOfInterest(List<Node> losAndRange, List<Node> los, List<Node> neither)
    {
        nodesInLosAndRange = losAndRange;
        nodesInLos = los;
        nodesInNeither = neither;
    }
}
