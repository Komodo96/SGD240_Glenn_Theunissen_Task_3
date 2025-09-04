using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp; 
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap) 
    {
        int width = heightMap.GetLength(0);  // Get width of the noise map (first dimension)
        int height = heightMap.GetLength(1); // Get height of the noise map (second dimension)

        // Prepare a 1D array of colors to assign to the texture
        Color[] colorMap = new Color[width * height];

        // Loop through each value in the noise map
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Interpolate between black (0) and white (1) based on the noise value
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }

        return TextureFromColorMap(colorMap, width, height);

    }
}
