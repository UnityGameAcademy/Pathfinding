using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// terrain type (Open = no movement penalty, Blocked = no movement possible)
public enum NodeType
{
    Open = 0,
    Blocked = 1,
    LightTerrain = 2,
    MediumTerrain = 3,
    HeavyTerrain = 4
}

// Node class implements the IComparable interface
public class Node: IComparable<Node>
{
    // Node's terrain type
    public NodeType nodeType = NodeType.Open;

    // x and y index in the graph array
    public int xIndex = -1;
    public int yIndex = -1;

    // (x,y,z) position in 3d space
    public Vector3 position;

    // list of neighbor Nodes
    public List<Node> neighbors = new List<Node>();

    // total distance traveled from the start Node
    public float distanceTraveled = Mathf.Infinity;

    // reference to preceding null in the current graph search
    public Node previous = null;

    // priority used to set place in queue
    public float priority;

    // constructor
    public Node(int xIndex, int yIndex, NodeType nodeType)
    {
        this.xIndex = xIndex;
        this.yIndex = yIndex;
        this.nodeType = nodeType;
    }

    // required by IComparable, method to compare this node with another Node based on priority
    public int CompareTo(Node other)
    {
        if (this.priority < other.priority)
        {
            return -1;
        }
        else if (this.priority > other.priority)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
   
    // set the preceding Node to null
    public void Reset()
    {
        previous = null;
    }


}
