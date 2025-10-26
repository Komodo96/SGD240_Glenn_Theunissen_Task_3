using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
// Stores general terrain settings and height info
public class TerrainData : UpdatableData
{
    // Uniform scale applied to the terrain mesh
    public float uniformScale = 2.5f;

    // Whether the terrain uses flat shading
    public bool useFlatShading;

    // Whether to apply a falloff map to the terrain
    public bool useFallOff;

    // Multiplier for mesh height values
    public float meshHeightMultiplier;

    // Curve to shape terrain heights
    public AnimationCurve meshHeightCurve;

    // Minimum terrain height based on the curve and scale
    public float minHeight
    {
        get
        {
            return uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(0);
        }
    }

    // Maximum terrain height based on the curve and scale
    public float maxHeight
    {
        get
        {
            return uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(1);
        }
    }
}

