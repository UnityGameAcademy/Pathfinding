using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//
public class Pathfinder : MonoBehaviour
{
    // the start of our path
    Node m_startNode;

    // the end
    Node m_goalNode;

    // Graph and GraphView components 
    Graph m_graph;
    GraphView m_graphView;

    // the "open set" of Nodes that are next to be explored
    PriorityQueue<Node> m_frontierNodes;

    // the "closed set" of Nodes that have been explored
    List<Node> m_exploredNodes;

    // the List of Nodes that make up our final path from start to goal
    List<Node> m_pathNodes;

    // colors to show our various NodeViews
    public Color startColor = Color.green;
    public Color goalColor = Color.red;
    public Color frontierColor = Color.magenta;
    public Color exploredColor = Color.gray;
    public Color pathColor = Color.cyan;
    public Color arrowColor = new Color(0.85f, 0.85f, 0.85f, 1f);
    public Color highlightColor = new Color(1f, 1f, 0.5f, 1f);

    // do we show each iteration?
    public bool showIterations = true;

    // do we show the colors on the NodeViews and GraphView?
    public bool showColors = true;

    // do we enable the arrow diagnostics?
    public bool showArrows = true;

    // do we terminate the search early?
    public bool exitOnGoal = true;

    // have we found the goal?
    public bool isComplete = false;

    // the number of iterations used
    int m_iterations = 0;

    // the various pathfinding algorithms
    public enum Mode
    {
        BreadthFirstSearch = 0,
        Dijkstra = 1,
        GreedyBestFirst = 2,
        AStar = 3
    }

    // active pathfinding mode/algorithm
    public Mode mode = Mode.BreadthFirstSearch;

    // initialize the pathfinder
    public void Init(Graph graph, GraphView graphView, Node start, Node goal)
    {
        // log an error if we are missing a start, goal, graph or graphView
        if (start == null || goal == null || graph == null || graphView == null)
        {
            Debug.LogWarning("PATHFINDER Init error: missing component(s)!");
            return;
        }

        // log an error if we have chosen a start or goal node within a wall
        if (start.nodeType == NodeType.Blocked || goal.nodeType == NodeType.Blocked)
        {
            Debug.LogWarning("PATHFINDER Init error: start and goal nodes must be unblocked!");
            return;
        }

        // cache our Graph, GraphView, starting Node, and goal Node
        m_graph = graph;
        m_graphView = graphView;
        m_startNode = start;
        m_goalNode = goal;

        // draw the background colors of the map
        ShowColors(graphView, start, goal);

        // our frontier begins with only the start Node
        m_frontierNodes = new PriorityQueue<Node>();
        m_frontierNodes.Enqueue(start);

        // initialize the explored Nodes and path Nodes as empty Lists
        m_exploredNodes = new List<Node>();
        m_pathNodes = new List<Node>();

        // reset all Nodes in the graph
        for (int x = 0; x < m_graph.Width; x++)
        {
            for (int y = 0; y < m_graph.Height; y++)
            {
                m_graph.nodes[x, y].Reset();
            }
        }

        // setup starting values
        isComplete = false;
        m_iterations = 0;
        m_startNode.distanceTraveled = 0;
    }

    // show colors on the GraphView
    void ShowColors(bool lerpColor = false, float lerpValue = 0.5f)
    {
        ShowColors(m_graphView, m_startNode, m_goalNode, lerpColor, lerpValue);
    }

	// show colors on the GraphView
	void ShowColors(GraphView graphView, Node start, Node goal, bool lerpColor = false, float lerpValue = 0.5f)
    {
        // return if we are missing GraphView, start Node or goal Node
        if (graphView == null || start == null || goal == null)
        {
            return;
        }

        // color frontier, explored and path Nodes
        if (m_frontierNodes != null)
        {
            graphView.ColorNodes(m_frontierNodes.ToList(), frontierColor, lerpColor, lerpValue);
        }

        if (m_exploredNodes != null)
        {
            graphView.ColorNodes(m_exploredNodes, exploredColor, lerpColor, lerpValue);
        }

        if (m_pathNodes != null && m_pathNodes.Count > 0)
        {
            graphView.ColorNodes(m_pathNodes, pathColor, lerpColor, lerpValue * 2f);
        }

        // color start NodeView and goal NodeView directly
        NodeView startNodeView = graphView.nodeViews[start.xIndex, start.yIndex];

        if (startNodeView != null)
        {
            startNodeView.ColorNode(startColor);
        }

        NodeView goalNodeView = graphView.nodeViews[goal.xIndex, goal.yIndex];

        if (goalNodeView != null)
        {
            goalNodeView.ColorNode(goalColor);
        }
    }

    // main graph search routine
    public IEnumerator SearchRoutine(float timeStep = 0.1f)
    {
        // starting time
        float timeStart = Time.realtimeSinceStartup;

        // wait one frame
        yield return null;

        // while we have not reached the goal node... (note that are missing a null reference check in the videos)
        while (!isComplete && m_frontierNodes != null)
        {
            // if there are still open Nodes to explore...
            if (m_frontierNodes.Count > 0)
            {
                // get the next available Node from the priority queue
                Node currentNode = m_frontierNodes.Dequeue();
                m_iterations++;

                // mark this Node as explored
                if (!m_exploredNodes.Contains(currentNode))
                {
                    m_exploredNodes.Add(currentNode);
                }

                // expand the frontier based on our search mode
                if (mode == Mode.BreadthFirstSearch)
                {
                    ExpandFrontierBreadthFirst(currentNode);
                }
                else if (mode == Mode.Dijkstra)
                {
                    ExpandFrontierDijkstra(currentNode);
                }
                else if (mode == Mode.GreedyBestFirst)
                {
                    ExpandFrontierGreedyBestFirst(currentNode);
                }
                else
                {
                    ExpandFrontierAStar(currentNode);
                }

                // if the goal node is in the frontier
                if (m_frontierNodes.Contains(m_goalNode))
                {
                    // set the path Nodes
                    m_pathNodes = GetPathNodes(m_goalNode);

                    // if exitOnGoal is checked, mark the search complete
                    if (exitOnGoal)
                    {
                        isComplete = true;
                        Debug.Log("PATHFINDER mode: " + mode.ToString() + "    path length = " + m_goalNode.distanceTraveled.ToString());
                    }
                }

                // draw colors and arrows
                if (showIterations)
                {
                    ShowDiagnostics(true, 0.5f);

                    yield return new WaitForSeconds(timeStep);
                }
            }
            // ...else we have explored the entire available graph
            else
            {
                isComplete = true;
            }
        }

        // draw colors and arrows once more
        ShowDiagnostics(true, 0.5f);

        // console log the elapsed time
        Debug.Log("PATHFINDER SearchRoutine: elapse time = " + (Time.realtimeSinceStartup - timeStart).ToString() + " seconds");
    }

    // wrapper method to show colors and arrows
    private void ShowDiagnostics(bool lerpColor = false, float lerpValue = 0.5f)
    {
        // show colors on the GraphView
        if (showColors)
        {
            ShowColors(lerpColor, lerpValue);
        }

        // show the arrows 
        if (m_graphView != null && showArrows)
        {
            // show arrows from the frontier in the arrow color
            m_graphView.ShowNodeArrows(m_frontierNodes.ToList(), arrowColor);

            // show the arrows along the completed path in the highlight color
            if (m_frontierNodes.Contains(m_goalNode))
            {
                m_graphView.ShowNodeArrows(m_pathNodes, highlightColor);
            }
        }
    }


	// expand the frontier nodes using Breadth First Search from a single node
	void ExpandFrontierBreadthFirst(Node node)
    {
        if (node != null)
        {
			// loop through the neighbors
			for (int i = 0; i < node.neighbors.Count; i++)
            {
				// if the current neighbor has not been explored and is not already part of the frontier
				if (!m_exploredNodes.Contains(node.neighbors[i]) && !m_frontierNodes.Contains(node.neighbors[i]))
                {
					// calculate distance to current neighbor
					float distanceToNeighbor = m_graph.GetNodeDistance(node, node.neighbors[i]);

					// calculate cumulative distance if we travel to neighbor via the current node
					float newDistanceTraveled = distanceToNeighbor + node.distanceTraveled + (int)node.nodeType;

					// create breadcrumb trail to neighbor node and set cumulative distance traveled
					node.neighbors[i].distanceTraveled = newDistanceTraveled;
                    node.neighbors[i].previous = node;

                    // add neighbor to explored Nodes, treat queue as if it were a first in-first out queue
                    node.neighbors[i].priority = m_exploredNodes.Count;
                    m_frontierNodes.Enqueue(node.neighbors[i]);
                }
            }
        }
    }

	// expand the frontier nodes using Dijkstra's algorithm from a single node
	void ExpandFrontierDijkstra(Node node)
    {
        if (node != null)
        {
			// loop through the neighbors
			for (int i = 0; i < node.neighbors.Count; i++)
            {
				// if the current neighbor has not been explored 
				if (!m_exploredNodes.Contains(node.neighbors[i]))
                {
                    float distanceToNeighbor = m_graph.GetNodeDistance(node, node.neighbors[i]);
                    float newDistanceTraveled = distanceToNeighbor + node.distanceTraveled + (int)node.nodeType;

					// if a shorter path exists to the neighbor via this node, re-route
					if (float.IsPositiveInfinity(node.neighbors[i].distanceTraveled) || newDistanceTraveled < node.neighbors[i].distanceTraveled)
                    {
                        node.neighbors[i].previous = node;
                        node.neighbors[i].distanceTraveled = newDistanceTraveled;
                    }

                    // if the current neighbor is not already part of the frontier...
                    if (!m_frontierNodes.Contains(node.neighbors[i]))
                    {
						// set the priority based on distance traveled from start Node and add to frontier
						node.neighbors[i].priority = node.neighbors[i].distanceTraveled;
                        m_frontierNodes.Enqueue(node.neighbors[i]);
                    }
                }
            }
        }
    }

	// expand the frontier nodes using Greedy Best-First search from a single node
	void ExpandFrontierGreedyBestFirst(Node node)
	{
		if (node != null)
		{
            // loop through the neighbors
			for (int i = 0; i < node.neighbors.Count; i++)
			{
				// if the current neighbor has not been explored and is not already part of the frontier
				if (!m_exploredNodes.Contains(node.neighbors[i]) && !m_frontierNodes.Contains(node.neighbors[i]))
				{
					// calculate distance to current neighbor
					float distanceToNeighbor = m_graph.GetNodeDistance(node, node.neighbors[i]);

					// calculate cumulative distance if we travel to neighbor via the current node
					float newDistanceTraveled = distanceToNeighbor + node.distanceTraveled + (int)node.nodeType;

                    // create breadcrumb trail to neighbor node and set cumulative distance traveled
					node.neighbors[i].distanceTraveled = newDistanceTraveled;
					node.neighbors[i].previous = node;

                    // set the priority based on estimated distance to goal Node...
                    if (m_graph != null)
                    {
                        node.neighbors[i].priority = m_graph.GetNodeDistance( node.neighbors[i], m_goalNode);
                    }
					
                    // ... and add to frontier
					m_frontierNodes.Enqueue(node.neighbors[i]);
				}
			}
		}
	}

    // expand the frontier nodes using AStar search from a single node
	void ExpandFrontierAStar(Node node)
	{
		if (node != null)
		{
            // loop through the neighbors
			for (int i = 0; i < node.neighbors.Count; i++)
			{
                // if the current neighbor has not been explored...
				if (!m_exploredNodes.Contains(node.neighbors[i]))
				{
                    // calculate distance to current neighbor
					float distanceToNeighbor = m_graph.GetNodeDistance(node, node.neighbors[i]);

                    // calculate cumulative distance if we travel to neighbor via the current node
					float newDistanceTraveled = distanceToNeighbor + node.distanceTraveled + (int)node.nodeType;

                    // if a shorter path exists to the neighbor via this node, re-route
					if (float.IsPositiveInfinity(node.neighbors[i].distanceTraveled) || newDistanceTraveled < node.neighbors[i].distanceTraveled)
					{
						node.neighbors[i].previous = node;
						node.neighbors[i].distanceTraveled = newDistanceTraveled;
					}

                    // if the neighbor is not part of the frontier, add this to the priority queue
                    if (!m_frontierNodes.Contains(node.neighbors[i]) && m_graph != null)
					{
                        // base priority, F score,  on G score (distance from start) + H score (estimated distance to goal)
                        float distanceToGoal = m_graph.GetNodeDistance(node.neighbors[i], m_goalNode);
                        node.neighbors[i].priority = node.neighbors[i].distanceTraveled + distanceToGoal;

                        // add to priority queue using the F score
						m_frontierNodes.Enqueue(node.neighbors[i]);
					}
				}
			}
		}
	}

    // generate a list of path Nodes working backward from an end Node
    List<Node> GetPathNodes(Node endNode)
    {
        List<Node> path = new List<Node>();
        if (endNode == null)
        {
            return path;
        }

        // start at the end Node
        path.Add(endNode);

		// follow the breadcrumb trail backward until we hit a node that has no previous node (usually the start Node)
		Node currentNode = endNode.previous;

        while (currentNode != null)
        {
            // insert the previous node at the first position in the path
            path.Insert(0, currentNode);

            // continue backward through the graph
            currentNode = currentNode.previous;
        }

        // return the list of Nodes
        return path;
    }

}
