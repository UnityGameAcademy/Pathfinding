using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoController : MonoBehaviour
{
    // MapData component
    public MapData mapData;

    // Graph component
    public Graph graph;

    // reference to Pathfinder component
    public Pathfinder pathfinder;

    // (x,z) coordinate of start Node
    public int startX = 0;
    public int startY = 0;

    // (x,z) coordinate of goal Node
    public int goalX = 15;
    public int goalY = 1;

    public KeyCode restartKey = KeyCode.Space;
    // delay between iterations
    public float timeStep = 0.1f;

    void Start()
    {
        if (mapData != null && graph != null)
        {
            InitMapAndGraph();
            FindPath();
        }
    }

    private void InitMapAndGraph()
    {
        // generate the map from text file or texture map
        int[,] mapInstance = mapData.MakeMap();

        // initialize the graph
        graph.Init(mapInstance);
    }

    // initialize the Pathfinder and begin the graph search
    private void FindPath()
    {
        if (pathfinder == null)
        {
            Debug.LogWarning("DEMOCONTROLLER FindPath: Missing Pathfinder!");
            return;
        }
        if (graph == null)
        {
            Debug.LogWarning("DEMOCONTROLLER FindPath: Missing Graph!");
            return;
        }

        if (!graph.IsWithinBounds(startX, startY) || !graph.IsWithinBounds(goalX, goalY))
        {
            Debug.LogWarning("DEMOCONTROLLER FindPath: start or goal out of bounds!");
            return;
        }

        Node startNode = graph.nodes[startX, startY];
        Node goalNode = graph.nodes[goalX, goalY];


        pathfinder.Init(graph, startNode, goalNode);

        StartCoroutine(pathfinder.SearchRoutine(timeStep));

    }

    private void Update()
    {

        if (Input.GetKeyDown(restartKey))
        {
            StopAllCoroutines();
            InitMapAndGraph();

            FindPath();
        }
    }
}
