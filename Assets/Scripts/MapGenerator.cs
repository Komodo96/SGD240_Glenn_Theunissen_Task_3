using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

// This class handles generating the noise map and passing it to the MapDisplay for visualization
public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColorMap, Mesh, FalloffMap };
    public DrawMode drawMode;

    public Noise.NormalizeMode normalizeMode;

    public const int mapChunkSize = 239;
    [Range(0, 6)]
    public int editorPreviewLOD;

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

    public bool useFallOff;

    public float meshHeightMultiplier;

    public AnimationCurve meshHeightCurve;

    // If true, updates the map automatically when parameters are changed in the editor
    public bool autoUpdate;

    [SerializeField]
    private TerrainType[] regions;

    float[,] fallOffMap;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    private void Awake()
    {
        fallOffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize);
    }

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);
        // Find the MapDisplay in the scene and draw the noise map
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.FalloffMap) 
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapChunkSize)));
        }
    }

    public void RequestMapData(Vector2 centre, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(centre, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 centre, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(centre);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate 
        {
            MeshDataThread(mapData, lod, callback);
        };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++) 
            { 
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count >0)
        {
            for (int i = 0;i < meshDataThreadInfoQueue.Count;i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    // Generates a new noise map and displays it
    MapData GenerateMapData(Vector2 centre)
    {
        // Generate the noise map using the static Noise class
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize +2, mapChunkSize +2, seed, noiseScale, octaves, persistence, lacunarity, centre + offset, normalizeMode);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                if (useFallOff)
                {
                    noiseMap[x,y] = Mathf.Clamp01(noiseMap[x,y] - fallOffMap[x,y]); 
                }
                float currentHeight = noiseMap[x, y];

                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight >= regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        return new MapData(noiseMap, colorMap);
    }
    // Validates inputs to ensure they stay within reasonable ranges
    private void OnValidate()
    {
        if (lacunarity < 1)
        {
            lacunarity = 1; // Lacunarity must be >= 1 to avoid flattening noise
        }
        if (octaves < 0)
        {
            octaves = 0; // Number of octaves cannot be negative
        }

        fallOffMap = FalloffGenerator.GenerateFalloffMap(mapChunkSize); 
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo (Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}


    [System.Serializable]
    public struct TerrainType
    {
        public float height;

        public Color color;

        public string name;
    }

    public struct MapData
    {
        public readonly float[,] heightMap;
        public readonly Color[] colorMap;

        public MapData(float[,] heightMap, Color[] colorMap)
        {
            this.heightMap = heightMap;
            this.colorMap = colorMap;
        }
    }
 
