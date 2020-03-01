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

    // delay between iterations
    public float timeStep = 0.1f;

    void Start()
    {
        if (mapData != null && graph != null)
        {
            // generate the map from text file or texture map
            int[,] mapInstance = mapData.MakeMap();

            // initialize the graph
            graph.Init(mapInstance);

            // generate the GraphView
            GraphView graphView = graph.gameObject.GetComponent<GraphView>();
            if (graphView != null)
            {
                graphView.Init(graph);
            }

            // initialize the Pathfinder and begin the graph search
            if (graph.IsWithinBounds(startX,startY) && graph.IsWithinBounds(goalX, goalY) && pathfinder != null)
            {
                Node startNode = graph.nodes[startX, startY];
                Node goalNode = graph.nodes[goalX, goalY];
                pathfinder.Init(graph, graphView, startNode, goalNode);
                StartCoroutine(pathfinder.SearchRoutine(timeStep));
            }
        }
    }

}
