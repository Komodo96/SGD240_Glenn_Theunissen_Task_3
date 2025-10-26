using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles displaying the generated terrain — either as a texture or a mesh
public class MapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    // Displays a noise map as a simple 2D grayscale texture
    public void DrawTexture(Texture2D texture)
    {
        // Apply the texture to the material
        textureRenderer.sharedMaterial.mainTexture = texture;

        // Adjust the plane’s scale to match the texture dimensions
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    // Displays a 3D terrain mesh using the provided mesh data
    public void DrawMesh(MeshData meshData)
    {
        // Create and assign the generated mesh
        meshFilter.sharedMesh = meshData.CreateMesh();

        // Scale the mesh based on the terrain’s global uniform scale setting
        meshFilter.transform.localScale = Vector3.one * FindObjectOfType<MapGenerator>().terrainData.uniformScale;
    }
}

