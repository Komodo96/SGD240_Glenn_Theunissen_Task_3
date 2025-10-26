using UnityEngine;
using System.Collections;
using System.Linq;

[CreateAssetMenu()]
public class TextureData : UpdatableData
{
    // Size and format for the generated texture arrays
    const int textureSize = 512;
    const TextureFormat textureFormat = TextureFormat.RGB565;

    // Array of terrain layers (textures + color info)
    public Layer[] layers;

    // Keep track of min and max height for mesh height updates
    float savedMinHeight;
    float savedMaxHeight;

    // Apply this texture data to a material
    public void ApplyToMaterial(Material material)
    {
        // Send basic layer info to shader
        material.SetInt("layerCount", layers.Length);
        material.SetColorArray("baseColors", layers.Select(x => x.tint).ToArray());
        material.SetFloatArray("baseStartHeights", layers.Select(x => x.startHeight).ToArray());
        material.SetFloatArray("baseBlends", layers.Select(x => x.blendStrength).ToArray());
        material.SetFloatArray("baseColorStrength", layers.Select(x => x.tintStrength).ToArray());
        material.SetFloatArray("baseTextureScales", layers.Select(x => x.textureScale).ToArray());

        // Generate a Texture2DArray for the shader
        Texture2DArray texturesArray = GenerateTextureArray(layers.Select(x => x.texture).ToArray());
        material.SetTexture("baseTextures", texturesArray);

        // Update shader with mesh height info
        UpdateMeshHeights(material, savedMinHeight, savedMaxHeight);
    }

    // Update min/max height values in the shader
    public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
    {
        savedMinHeight = minHeight;
        savedMaxHeight = maxHeight;

        material.SetFloat("minHeight", minHeight);
        material.SetFloat("maxHeight", maxHeight);
    }

    // Convert a list of textures into a Texture2DArray for the shader
    Texture2DArray GenerateTextureArray(Texture2D[] textures)
    {
        Texture2DArray textureArray = new Texture2DArray(textureSize, textureSize, textures.Length, textureFormat, true);
        for (int i = 0; i < textures.Length; i++)
        {
            textureArray.SetPixels(textures[i].GetPixels(), i);
        }
        textureArray.Apply();
        return textureArray;
    }

    [System.Serializable]
    public class Layer
    {
        public Texture2D texture;          // Base texture for the layer
        public Color tint;                 // Color tint applied to the layer
        [Range(0, 1)]
        public float tintStrength;         // How strongly the tint affects the layer
        [Range(0, 1)]
        public float startHeight;          // Height at which this layer starts blending
        [Range(0, 1)]
        public float blendStrength;        // How smoothly this layer blends with others
        public float textureScale;         // Scale of the texture on the terrain
    }
}
