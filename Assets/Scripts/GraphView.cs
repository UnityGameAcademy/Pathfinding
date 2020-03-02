using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// User Interface component representing a set of NodeViews, attached to Graph gameObject
[RequireComponent(typeof(Graph))]
public class GraphView : MonoBehaviour
{

    // do we enable the arrow diagnostics?
    public bool showArrows = true;

    // NodeView prefab
    public GameObject nodeViewPrefab;

    // two-dimensional array of NodeViews
    public NodeView[,] nodeViews;

    // colors to show our various NodeViews
    public Color startColor = Color.green;
    public Color goalColor = Color.red;
    public Color frontierColor = Color.magenta;
    public Color exploredColor = Color.gray;
    public Color pathColor = Color.cyan;
    public Color arrowColor = new Color(0.85f, 0.85f, 0.85f, 1f);
    public Color highlightColor = new Color(1f, 1f, 0.5f, 1f);

    private Pathfinder m_pathfinder;
    private Graph m_graph;

    private void Awake()
    {
        m_pathfinder = FindObjectOfType<Pathfinder>();
        m_graph = GetComponent<Graph>();

        // subscribe to Pathfinder events/actions
        if (m_pathfinder)
        {
            m_pathfinder.initLevelAction += OnInitLevel;
            m_pathfinder.drawIterationAction += OnDraw;
            m_pathfinder.foundPathAction += OnFoundPath;
        }
    }

    private void OnDisable()
    {
        if (m_pathfinder)
        {
            m_pathfinder.initLevelAction -= OnInitLevel;
            m_pathfinder.drawIterationAction -= OnDraw;
            m_pathfinder.foundPathAction -= OnFoundPath;
        }
    }

    private void OnDestroy()
    {
        if (m_pathfinder)
        {
            m_pathfinder.initLevelAction -= OnInitLevel;
            m_pathfinder.drawIterationAction -= OnDraw;
            m_pathfinder.foundPathAction -= OnFoundPath;
        }
    }

    // draw the blank map
    private void OnInitLevel()
    {
        InitGraphView();
    }

    // color nodes and draw arrows
    private void OnDraw()
    {
        if (!m_pathfinder.HasFoundGoal)
        {
            ColorFrontierNodes();
            ColorExploredNodes();
            ColorPathNodes();
            ColorStartGoalNodes(m_pathfinder.StartNode, m_pathfinder.GoalNode);

            if (showArrows)
            {
                ShowFrontierArrows();

            }
        }
    }

    // draw final path and arrows
    private void OnFoundPath()
    {
        ColorPathNodes();
        ColorStartGoalNodes(m_pathfinder.StartNode, m_pathfinder.GoalNode);
        if (showArrows)
        {
            ShowPathArrows();
        }
    }

    // initialize using a Graph
    public void InitGraphView()
    {
        if (m_pathfinder == null)
        {
            Debug.LogWarning("GRAPHVIEW Init: No Pathfinder to initialize!");
            return;
        }

        if (nodeViews != null)
        {
            foreach (NodeView nodeView in nodeViews)
            {
                Destroy(nodeView.gameObject);
            }
        }

        ColorMapNodes(m_graph);
        ColorStartGoalNodes(m_pathfinder.StartNode, m_pathfinder.GoalNode);
    }

    // color the initial NodeViews of the map (walls, open spaces)
    private void ColorMapNodes(Graph graph)
    {
        if (nodeViewPrefab == null)
        {
            Debug.LogWarning("GRAPHVIEW ColorMapNodes: Missing NodeViewPrefab!");
            return;
        }

        if (graph == null)
        {
            Debug.LogWarning("GRAPHVIEW ColorMapNodes: No graph to initialize!");
            return;
        }

        // setup array of NodeViews
        nodeViews = new NodeView[graph.Width, graph.Height];


        foreach (Node n in graph.nodes)
        {
            // create a NodeView for each corresponding Node

            GameObject instance = Instantiate(nodeViewPrefab, Vector3.zero, Quaternion.identity);
            NodeView nodeView = instance.GetComponent<NodeView>();

            if (nodeView != null)
            {
                // initialize each NodeView
                nodeView.Init(n);

                // store each NodeView in the array
                nodeViews[n.xIndex, n.yIndex] = nodeView;

                // find the corresponding Color from the MapData
                Color originalColor = MapData.GetColorFromNodeType(n.nodeType);

                // color code the NodeView
                nodeView.ColorNode(originalColor);

            }
        }
    }

    public void ColorStartGoalNodes(Node start, Node goal)
    {

        // color start NodeView and goal NodeView directly
        NodeView startNodeView = this.nodeViews[start.xIndex, start.yIndex];

        if (startNodeView != null)
        {
            startNodeView.ColorNode(startColor);
        }

        NodeView goalNodeView = this.nodeViews[goal.xIndex, goal.yIndex];

        if (goalNodeView != null)
        {
            goalNodeView.ColorNode(goalColor);
        }
    }

    public void ColorFrontierNodes(bool lerpColor = false, float lerpValue = 0.5f)
    {
        if (m_pathfinder != null && m_pathfinder.FrontierNodes != null)
        {
            ColorNodes(m_pathfinder.FrontierNodes.ToList(), frontierColor, lerpColor, lerpValue);
        }
    }

    public void ColorPathNodes(bool lerpColor = false, float colorMultiplier = 2f, float lerpValue = 0.5f)
    {
        if (m_pathfinder != null && m_pathfinder.PathNodes != null && m_pathfinder.PathNodes.Count > 0)
        {
            ColorNodes(m_pathfinder.PathNodes, pathColor, lerpColor, lerpValue * colorMultiplier);
        }
    }

    public void ColorExploredNodes(bool lerpColor = false, float lerpValue = 0.5f)
    {
        if (m_pathfinder != null && m_pathfinder.ExploredNodes != null && m_pathfinder.ExploredNodes.Count > 0)
        {
            ColorNodes(m_pathfinder.ExploredNodes, exploredColor, lerpColor, lerpValue);
        }
    }


    // color a List of NodeViews, given a List of Nodes, optionally blending color with original color
    public void ColorNodes(List<Node> nodes, Color color, bool lerpColor = false, float lerpValue = 0.5f)
    {
        // for each Node...
        foreach (Node n in nodes)
        {
            if (n != null)
            {
                // find the corresponding NodeView
                NodeView nodeView = nodeViews[n.xIndex, n.yIndex];

                // default target color
                Color newColor = color;

                // if we are blending colors...
                if (lerpColor)
                {
                    // ... find the original color
                    Color originalColor = MapData.GetColorFromNodeType(n.nodeType);

                    // calculate the blended color
                    newColor = Color.Lerp(originalColor, newColor, lerpValue);
                }

                // color the NodeView
                if (nodeView != null)
                {
                    nodeView.ColorNode(newColor);
                }
            }
        }
    }

    // show the arrows as the frontier nodes expand
    public void ShowFrontierArrows()
    {
        if (m_pathfinder != null)
        {
            ShowNodeArrows(m_pathfinder.FrontierNodes.ToList(), arrowColor);
        }
    }

    // draw highlighted arrow trail to goal
    public void ShowPathArrows()
    {
        if (m_pathfinder != null)
        {
            ShowNodeArrows(m_pathfinder.PathNodes, highlightColor);
            ShowNodeArrows(m_pathfinder.GoalNode, highlightColor);
            //Debug.Log("GoalNode =" + m_pathfinder.GoalNode.xIndex + "," + m_pathfinder.GoalNode.yIndex);
        }
    }

    // show the path arrows for a single Node
    public void ShowNodeArrows(Node node, Color color)
    {
        if (node != null)
        {
            NodeView nodeView = nodeViews[node.xIndex, node.yIndex];
            if (nodeView != null)
            {
                nodeView.ShowArrow(color);
            }
        }
    }

    // show the path arrows for a List of Nodes
    public void ShowNodeArrows(List<Node> nodes, Color color)
    {
        foreach (Node n in nodes)
        {
            ShowNodeArrows(n, color);
        }
    }
}
