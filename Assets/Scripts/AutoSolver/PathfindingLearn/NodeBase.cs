using System;
using System.Collections.Generic;

public class NodeBase
{
    public NodeBase Connection { get; private set; }
    public float G { get; private set; }
    public float H { get; private set; }
    public float F => G + H;

    public void SetConnection(NodeBase nodeBase) => Connection = nodeBase;

    public void SetG(float g) => G = g;
    public void SetH(float h) => H = h;


    internal float GetDistance(NodeBase neighbor)
    {
        return 1;
    }

    public List<NodeBase> Neighbors;

    public bool Walkable = true;
} 