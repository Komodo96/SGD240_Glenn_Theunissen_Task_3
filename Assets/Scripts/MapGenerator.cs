using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class handles generating the noise map and passing it to the MapDisplay for visualization
public class MapGenerator : MonoBehaviour
{
    // Dimensions of the map (width and height)
    [SerializeField]
    private int mapWidth = 100;

    [SerializeField]
    private int mapHeight = 100;

    // Controls the scale of the noise; higher = more stretched terrain features
    [SerializeField]
    private float noiseScale = 25f;

    // Number of layers of noise to combine
    [SerializeField]
    private int octaves = 4;

    // Controls how much each successive octave contributes to overall height
    [SerializeField]
    [Range(0, 1)]
    private float persistence = 0.5f;

    // Controls frequency increase between octaves; higher = more frequent detail
    [SerializeField]
    private float lacunarity = 2f;

    // Seed for deterministic generation
    [SerializeField]
    private int seed;

    // Allows manual offsetting of the noise map
    [SerializeField]
    private Vector2 offset;

    // If true, updates the map automatically when parameters are changed in the editor
    public bool autoUpdate;

    // Generates a new noise map and displays it
    public void GenerateMap()
    {
        // Generate the noise map using the static Noise class
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistence, lacunarity, offset);

        // Find the MapDisplay in the scene and draw the noise map
        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.DrawNoiseMap(noiseMap);
    }

    // Validates inputs to ensure they stay within reasonable ranges
    private void OnValidate()
    {
        if (mapWidth < 1)
        {
            mapWidth = 1; // Ensure map width is at least 1
        }
        if (mapHeight < 1)
        {
            mapHeight = 1; // Ensure map height is at least 1
        }
        if (lacunarity < 1)
        {
            lacunarity = 1; // Lacunarity must be >= 1 to avoid flattening noise
        }
        if (octaves < 0)
        {
            octaves = 0; // Number of octaves cannot be negative
        }
    }
}
