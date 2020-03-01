using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// User Interface component representing a set of NodeViews, attached to Graph gameObject
[RequireComponent(typeof(Graph))]
public class GraphView : MonoBehaviour
{
    // NodeView prefab
    public GameObject nodeViewPrefab;

    // two-dimensional array of NodeViews
    public NodeView[,] nodeViews;

    // initialize using a Graph
    public void Init(Graph graph)
    {
        if (graph == null)
        {
            Debug.LogWarning("GRAPHVIEW No graph to initialize!");
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
