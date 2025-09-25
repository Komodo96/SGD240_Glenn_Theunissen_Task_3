using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseData : UpdatableData
{
    public Noise.NormalizeMode normalizeMode;

    // Controls the scale of the noise; higher = more stretched terrain features
    public float noiseScale = 25f;

    // Number of layers of noise to combine
    public int octaves = 4;

    // Controls how much each successive octave contributes to overall height
    [Range(0, 1)]
    public float persistence = 0.5f;

    // Controls frequency increase between octaves; higher = more frequent detail
    public float lacunarity = 2f;

    // Seed for deterministic generation
    public int seed;

    // Allows manual offsetting of the noise map
    public Vector2 offset;

    protected override void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1; // Lacunarity must be >= 1 to avoid flattening noise
        }
        if (octaves < 0)
        {
            octaves = 0; // Number of octaves cannot be negative
        }
        base.OnValidate();
    }

}
