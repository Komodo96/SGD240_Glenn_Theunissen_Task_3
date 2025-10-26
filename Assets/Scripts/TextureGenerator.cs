using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator
{
    // Creates a texture from a given color map
    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;       // Keeps sharp pixel edges
        texture.wrapMode = TextureWrapMode.Clamp;    // Prevents texture tiling
        texture.SetPixels(colorMap);                 // Assign all color values
        texture.Apply();                             // Apply changes to the texture
        return texture;
    }

    // Creates a grayscale texture from a 2D heightmap
    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);   // Width of the map
        int height = heightMap.GetLength(1);  // Height of the map

        // Create an array of colors for the texture
        Color[] colorMap = new Color[width * height];

        // Loop through every point in the heightmap
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Map height value (0–1) to a color between black and white
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }

        // Generate texture from the color map
        return TextureFromColorMap(colorMap, width, height);
    }
}

