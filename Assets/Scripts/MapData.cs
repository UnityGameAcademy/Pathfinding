using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class MapData : MonoBehaviour
{
    // map dimensions
    public int width = 10;
    public int height = 5;

    // text file containing map data
    public TextAsset textAsset;

    // texture map containing map data
    public Texture2D textureMap;

    // subfolder inside /Resources folder
    public string resourcePath = "Mapdata";

    // terrain colors
    public Color32 openColor = Color.white;
    public Color32 blockedColor = Color.black;
    public Color32 lightTerrainColor = new Color32(124, 194, 78, 255);
    public Color32 mediumTerrainColor = new Color32(252, 255, 52, 255);
    public Color32 heavyTerrainColor = new Color32(255, 129, 12, 255);

    // Dictionary relating Color32 with NodeType
    static Dictionary<Color32, NodeType> terrainLookupTable = new Dictionary<Color32, NodeType>();

    void Awake()
    {
        // setup the Dictionary
        SetupLookupTable();
    }

    void Start()
    {
        // automatically load any texture map or text file named for the current scene
        string levelName = SceneManager.GetActiveScene().name;

        if (textureMap == null)
        {
            textureMap = Resources.Load(resourcePath + "/" + levelName) as Texture2D;
        }

        if (textAsset == null)
        {

            textAsset = Resources.Load(resourcePath + "/" + levelName) as TextAsset;
        }
    }
       
    // return the level map from a specified text file asset
    public List<string> GetMapFromTextFile(TextAsset tAsset)
    {
        List<string> lines = new List<string>();

        // split text string by end-of-line characters and return as List of strings
        if (tAsset != null)
        {
            string textData = tAsset.text;
            string[] delimiters = { "\r\n", "\n" };
            lines.AddRange(textData.Split(delimiters, System.StringSplitOptions.None));
            lines.Reverse();
        }
        else
        {
            Debug.LogWarning("MAPDATA GetTextFromFile Error: invalid TextAsset");
        }
        return lines;
    }


    // return a level map from default text file asset
    public List<string> GetMapFromTextFile()
    {
        return GetMapFromTextFile(textAsset);
    }

    // 
    public List<string> GetMapFromTexture(Texture2D texture)
    {
        List<string> lines = new List<string>();

        if (texture != null)
        {
            for (int y = 0; y < texture.height; y++)
            {
                string newLine = "";

                for (int x = 0; x < texture.width; x++)
                {
                    Color pixelColor = texture.GetPixel(x, y);

                    if (terrainLookupTable.ContainsKey(pixelColor))
                    {
                        NodeType nodeType = terrainLookupTable[pixelColor];
                        int nodeTypeNum = (int)nodeType;
                        newLine += nodeTypeNum;
                    }
                    else
                    {
                        newLine += '0';
                    }
                }
                lines.Add(newLine);
            }
        }

        return lines;
    }

    // set the height and width of our map from a List of strings
    public void SetDimensions(List<string> textLines)
    {
        height = textLines.Count;

        foreach (string line in textLines)
        {
            if (line.Length > width)
            {
                width = line.Length;
            }
        }
    }
    // convert a texture or text file to an array of integers representing our map
    public int[,] MakeMap()
    {
        List<string> lines = new List<string>();

        if (textureMap != null)
        {
            lines = GetMapFromTexture(textureMap);
        }
        else
        {
            lines = GetMapFromTextFile(textAsset);
        }

        SetDimensions(lines);

        int[,] map = new int[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (lines[y].Length > x)
                {
                    map[x, y] = (int)Char.GetNumericValue(lines[y][x]);
                }
            }
        }
        return map;

    }

    // setup the colors and NodeTypes
    void SetupLookupTable()
    {
        terrainLookupTable.Add(openColor, NodeType.Open);
        terrainLookupTable.Add(blockedColor, NodeType.Blocked);
        terrainLookupTable.Add(lightTerrainColor, NodeType.LightTerrain);
        terrainLookupTable.Add(mediumTerrainColor, NodeType.MediumTerrain);
        terrainLookupTable.Add(heavyTerrainColor, NodeType.HeavyTerrain);

    }

    // public method to convert a NodeType to a color
    public static Color GetColorFromNodeType(NodeType nodeType)
    {
        // 
        if (terrainLookupTable.ContainsValue(nodeType))
        {
            Color colorKey = terrainLookupTable.FirstOrDefault(x => x.Value == nodeType).Key;
            return colorKey;
        }

        return Color.white;
    }
}
