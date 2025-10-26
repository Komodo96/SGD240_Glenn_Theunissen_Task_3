using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Generates a falloff map used to fade terrain edges towards the borders
public class FalloffGenerator
{
    public static float[,] GenerateFalloffMap(int size)
    {
        float[,] map = new float[size, size];

        // Loop through every coordinate on the map
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                // Convert coordinates to a range between -1 and 1
                float x = i / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;

                // Use the larger of the two absolute values to form a diamond-shaped gradient
                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));

                // Apply the falloff curve to smooth the edge
                map[i, j] = Evaluate(value);
            }
        }
        return map;
    }

    // Controls how quickly the falloff fades based on distance from center
    static float Evaluate(float value)
    {
        float a = 3;
        float b = 2.2f;

        // Applies a smooth curve function to shape the falloff
        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}

