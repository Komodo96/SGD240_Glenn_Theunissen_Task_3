using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    //The width of the terrain on the X axis
    [SerializeField]
    private int width = 256;

    //The height of the terrain on the Z axis
    [SerializeField]
    private int height = 256;

    //The maximum vertical height of the terrain on the Y axis
    [SerializeField]
    private int depth = 20;

    //Controls how stretched or zoomed in the Perlin noise looks
    [SerializeField]
    private float scale = 20f;

    //User provided seed so terrain is reproducible
    [SerializeField]
    private int seed = 0;

    //Offsets to shift the Perlin noise sample position. Ensures different seeds create different terrains
    private float offsetX;
    private float offsetY;
   

    void Start()
    {
        //Initialize a pseudo-random number generator using the seed
        System.Random prng = new System.Random(seed);

        //Generate deterministic offsets from the Pseudo Ramdom Number Generator
        offsetX = prng.Next(0, 10000);
        offsetY = prng.Next(0, 10000);

        //Get the Terrain component attached to this GameObject and assign its terrainData with our generated terrain
        Terrain terrain = GetComponent<Terrain>();
        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        // Set resolution of the terrain heightmap
        terrainData.heightmapResolution = width + 1;

        //Set the physical size of the terrain in Unity, X = horizontal width, Y = ground depth, Z = vertical height
        terrainData.size = new Vector3(width, depth, height);

        //Fill the terrain with height values generated from noise
        terrainData.SetHeights(0, 0, GenerateHeights());
        return terrainData;
    }

    float[,] GenerateHeights()
    {
        //Create a 2D array of height values
        float[,] heights = new float[width, height];

        //Loop through every coordinate in the terrain grid
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                //Calculate height at each point using Perlin noise
                heights[x, y] = CalculateHeight(x, y);
            }
        }
        return heights;
    }

    float CalculateHeight(int x, int y)
    {
        //Convert terrain coordinates into Perlin noise coordinates
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / height * scale + offsetY;
        return Mathf.PerlinNoise(xCoord, yCoord);
    }
}