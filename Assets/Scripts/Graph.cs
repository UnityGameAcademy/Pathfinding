using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// class used to manage entire set of Nodes
public class Graph : MonoBehaviour
{
    // two-dimensional array of Nodes
    public Node[,] nodes;

    // List of Blocked Nodes
    public List<Node> walls = new List<Node>();

    // MapData component
    int[,] m_mapData;

    // map/array dimensions
    int m_width;
    public int Width { get { return m_width; } }
    int m_height;
    public int Height { get { return m_height; } }

    // compass directions used for checking neighboring Nodes
    public static readonly Vector2[] allDirections =
    {
        new Vector2(0f,1f),
        new Vector2(1f,1f),
        new Vector2(1f,0f),
        new Vector2(1f,-1f),
        new Vector2(0f,-1f),
        new Vector2(-1f,-1f),
        new Vector2(-1f,0f),
        new Vector2(-1f,1f)
    };

    // initialize the Graph using MapData
    public void Init(int[,] mapData)
    {
        // cache the MapData for later use
        m_mapData = mapData;

        // set the dimensions based on the array
        m_width = mapData.GetLength(0);
        m_height = mapData.GetLength(1);

        // initialize the array
        nodes = new Node[m_width, m_height];

        // at each (x,y) position in the array...
        for (int y = 0; y < m_height; y++)
        {
            for (int x = 0; x < m_width; x++)
            {
                // ... set the NodeType based on the MapData
                NodeType type = (NodeType)mapData[x, y];

                // generat a new Node and set in the array
                Node newNode = new Node(x, y, type);
                nodes[x, y] = newNode;

                // set the (x,z) Vector3 position position
                newNode.position = new Vector3(x, 0, y);

                // if NodeType is Blocked, store in separate List
                if (type == NodeType.Blocked)
                {
                    walls.Add((newNode));
                }
            }
        }

        // setup the neighbor Nodes for each node in the array
        for (int y = 0; y < m_height; y++)
        {
            for (int x = 0; x < m_width; x++)
            {
                if (nodes[x, y].nodeType != NodeType.Blocked)
                {
                    nodes[x, y].neighbors = GetNeighbors(x, y);
                }
            }
        }
    }

    // is (x,y) within the bounds of the Graph?
    public bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < m_width && y >= 0 && y < m_height);
    }

    // returns a List of neighboring Nodes from (x,y) coordinate, array of Nodes and compass directions
    List<Node> GetNeighbors(int x, int y, Node[,] nodeArray, Vector2[] directions)
    {
        // new empty List of Nodes
        List<Node> neighborNodes = new List<Node>();

        // in each direction vector...
        foreach (Vector2 dir in directions)
        {
            // find the (x,y) offset position
            int newX = x + (int)dir.x;
            int newY = y + (int)dir.y;

            // if the new position is within the graph and not blocked, add to List
            if (IsWithinBounds(newX, newY) && nodeArray[newX, newY] != null && nodeArray[newX, newY].nodeType != NodeType.Blocked)
            {
                neighborNodes.Add(nodeArray[newX, newY]);
            }
        }
        // return the List of neighbors
        return neighborNodes;

    }

	// returns a List of neighboring Nodes
	List<Node> GetNeighbors(int x, int y)
    {
        return GetNeighbors(x, y, nodes, allDirections);
    }

    // get approximate distance between nodes (using 1.4 for the square root of 2)
    public float GetNodeDistance(Node source, Node target)
    {
        int dx = Mathf.Abs(source.xIndex - target.xIndex);
        int dy = Mathf.Abs(source.yIndex - target.yIndex);

        int min = Mathf.Min(dx, dy);
        int max = Mathf.Max(dx, dy);

        int diagonalSteps = min;
        int straightSteps = max - min;

        return (1.4f * diagonalSteps + straightSteps);
    }

    // distance estimate between nodes using Manhattan/Taxicab distance
    public int GetManhattanDistance(Node source, Node target)
    {
		int dx = Mathf.Abs(source.xIndex - target.xIndex);
		int dy = Mathf.Abs(source.yIndex - target.yIndex);
        return (dx + dy);
    }

}
