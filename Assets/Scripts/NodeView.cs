using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeView : MonoBehaviour
{
    // tile geometry
    public GameObject tile;

    // arrow geometry
    public GameObject arrow;

    // corresponding node
    Node m_node;

    // empty border width around each tile
    [Range(0, 0.5f)]
    public float borderSize = 0.15f;

    // initilize the NodeView with the corresponding Node
    public void Init(Node node)
    {
        if (tile != null)
        {
            // rename the NodeView gameObject to include the node's x and y index
            gameObject.name = "Node (" + node.xIndex + "," + node.yIndex + ")";

            // move the NodeView to the Vector3 position in the corresponding Node
            gameObject.transform.position = node.position;

            // scale the tile geometry to make an empty border around the edge
            tile.transform.localScale = new Vector3(1f - borderSize, 1f, 1f - borderSize);

            // save the corresponding Node for later use
            m_node = node;

            // disable the arrow geometry
            EnableObject(arrow, false);
        }
    }

    // method to color a gameObject
    void ColorNode(Color color, GameObject go)
    {
        if (go != null)
        {
            Renderer goRenderer = go.GetComponent<Renderer>();

            if (goRenderer != null)
            {
                goRenderer.material.color = color;
            }
        }
    }

    // method to color the tile geometry
    public void ColorNode(Color color)
    {
        ColorNode(color, tile);
    }

    // general method to toggle an object on/off
    void EnableObject(GameObject go, bool state)
    {
        if (go != null)
        {
            go.SetActive(state);
        }
    }

    // show the arrow with a color
    public void ShowArrow(Color color)
    {
        // verify we have a corresponding node, arrow geometry, and have a target for our arrow
        if (m_node != null && arrow != null && m_node.previous != null)
        {
            // turn the arrow geometry on
            EnableObject(arrow, true);

            // calculate direction to target Node
            Vector3 dirToPrevious = (m_node.previous.position - m_node.position).normalized;

            // rotate arrow toward target
            arrow.transform.rotation = Quaternion.LookRotation(dirToPrevious);

            // modify the arrow's material color
            Renderer arrowRenderer = arrow.GetComponent<Renderer>();
            if (arrowRenderer != null)
            {
                arrowRenderer.material.color = color;
            }
        }
    }
}
