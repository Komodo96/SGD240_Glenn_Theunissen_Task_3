using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Static class for generating procedural noise maps
public static class Noise
{

    public enum NormalizeMode { Local, Global};

    // Generates a 2D noise map based on Perlin noise with multiple octaves
    public static float[,] GenerateNoiseMap(
        int mapWidth,         // Width of the noise map
        int mapHeight,        // Height of the noise map
        int seed,             // Seed for deterministic generation
        float scale,          // Scale of the noise
        int octaves,          // Number of noise layers to combine for detail
        float persistence,    // How amplitude decreases per octave
        float lacunarity,     // How frequency increases per octave
        Vector2 offset,        // Offset to shift the entire map
        NormalizeMode normalizeMode
    )
    {
        // Initialize the 2D array that will hold noise values
        float[,] noiseMap = new float[mapWidth, mapHeight];

        // Pseudo random number generator based on seed for reproducibility
        System.Random prng = new System.Random(seed);

        // Calculate offsets for each octave to prevent repetition
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;     // initial amplitude for this position
        float frequency = 1;     // initial frequency for this position

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x; // random X offset for this octave
            float offsetY = prng.Next(-100000, 100000) - offset.y; // random Y offset for this octave
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistence;

        }

        // Prevent division by zero
        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        // Track min and max noise heights for normalization later
        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        // Center the noise map around 0 for symmetry
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        // Loop through each position in the map
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                amplitude = 1;     // initial amplitude for this position
                frequency = 1;     // initial frequency for this position
                float noiseHeight = 0;   // accumulated noise value for this position

                // Combine multiple octaves for richer terrain detail
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

                    // PerlinNoise returns 0-1; convert to -1 to 1 for symmetry
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    // Update amplitude and frequency for next octave
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                // Track min and max for normalization
                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }

                // Assign the raw noise height
                noiseMap[x, y] = noiseHeight;
            }
        }

        // Normalize the noise map to 0-1
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (normalizeMode == NormalizeMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
                else
                {
                    float normalizedHeight = (noiseMap [x, y] +1) / (maxPossibleHeight);
                    noiseMap[x, y] = Mathf.Clamp (normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        // Return the final normalized noise map
        return noiseMap;
    }
}
