using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class is responsible for taking a 2D noise map and displaying it visually
public class MapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    // Draws a grayscale texture based on the noise map
    public void DrawTexture(Texture2D texture)
    {

        // Assign the texture to the Renderer’s material
        textureRenderer.sharedMaterial.mainTexture = texture;

        // Scale the Renderer to match the texture dimensions (for correct visual proportions)
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData, Texture2D texture)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer. sharedMaterial.mainTexture = texture; 
    }
}
