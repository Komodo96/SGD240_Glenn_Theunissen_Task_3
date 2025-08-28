using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class is responsible for taking a 2D noise map and displaying it visually
public class MapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;

    // Draws a grayscale texture based on the noise map
    public void DrawNoiseMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);  // Get width of the noise map (first dimension)
        int height = noiseMap.GetLength(1); // Get height of the noise map (second dimension)

        // Create a new Texture2D to visualize the noise
        Texture2D texture = new Texture2D(width, height);

        // Prepare a 1D array of colors to assign to the texture
        Color[] colorMap = new Color[width * height];

        // Loop through each value in the noise map
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Interpolate between black (0) and white (1) based on the noise value
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }

        // Apply the colors to the texture
        texture.SetPixels(colorMap);
        texture.Apply(); // Applies changes and updates the GPU texture

        // Assign the texture to the Renderer’s material
        textureRenderer.sharedMaterial.mainTexture = texture;

        // Scale the Renderer to match the texture dimensions (for correct visual proportions)
        textureRenderer.transform.localScale = new Vector3(width, 1, height);
    }
}
