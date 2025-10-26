using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    // Controls how far the viewer must move before chunks are updated
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    // Distance within which colliders should be generated for terrain chunks
    const float colliderGenerationDistanceThreshold = 5;

    public int colliderLODIndex;                // LOD index used for collision mesh generation
    public LODInfo[] detailLevels;              // Array defining the LOD distances
    public static float maxViewDst;             // Maximum distance the viewer can see chunks

    public Transform viewer;                    // The player or camera transform
    public Material mapMaterial;                // Material applied to generated terrain meshes

    public static Vector2 viewerPosition;       // 2D position of the viewer in world space (X, Z)
    Vector2 viewerPositionOld;                  // Previous frame’s viewer position
    public static MapGenerator mapGenerator;    // Reference to the MapGenerator that provides height and mesh data

    int chunkSize;                              // Size of each terrain chunk in units
    int chunksVisibleInViewDst;                 // Number of chunks visible based on view distance

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();  // Keeps track of all loaded chunks
    static List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();                            // List of currently visible chunks

    private void Start()
    {
        // Get reference to the MapGenerator in the scene
        mapGenerator = FindObjectOfType<MapGenerator>();

        // Determine the maximum view distance based on the furthest LOD level
        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;

        // Calculate chunk size and how many fit within view distance
        chunkSize = mapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);

        // Generate the first set of visible chunks
        UpdateVisibleChunks();
    }

    void Update()
    {
        // Update viewer position relative to the terrain’s uniform scale
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapGenerator.terrainData.uniformScale;

        // Update collision meshes if the viewer has moved
        if (viewerPosition != viewerPositionOld)
        {
            foreach (TerrainChunk chunk in visibleTerrainChunks)
            {
                chunk.UpdateCollisionMesh();
            }
        }

        // Only update visible chunks when the viewer moves a significant distance
        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    // Updates which chunks should be visible based on the viewer’s position
    void UpdateVisibleChunks()
    {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();

        // Loop through all visible chunks and refresh them
        for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--)
        {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        // Determine the current chunk the viewer is standing on
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        // Check surrounding chunks within the visible range
        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                // Update existing chunks or create new ones if not already loaded
                if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                {
                    if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    }
                    else
                    {
                        terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, colliderLODIndex, transform, mapMaterial));
                    }
                }
            }
        }
    }

    // Represents a single chunk of terrain
    public class TerrainChunk
    {
        public Vector2 coord;               // Grid coordinate of the chunk
        GameObject meshObject;              // The actual GameObject for the terrain mesh
        Vector2 position;                   // Position in world space
        Bounds bounds;                      // Used for distance calculations

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        int colliderLODIndex;

        MapData mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;
        bool hasSetCollider;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Material material)
        {
            this.coord = coord;
            this.detailLevels = detailLevels;
            this.colliderLODIndex = colliderLODIndex;

            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);

            // Create the GameObject for this terrain chunk
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            meshObject = new GameObject("TerrainChunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();

            meshRenderer.material = material;

            // Set transform properties
            meshObject.transform.position = positionV3 * mapGenerator.terrainData.uniformScale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * mapGenerator.terrainData.uniformScale;

            SetVisible(false); // Start hidden until within view

            // Initialize LOD meshes
            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod);
                lodMeshes[i].updateCallback += UpdateTerrainChunk;

                // The LOD used for colliders also updates the collision mesh
                if (i == colliderLODIndex)
                {
                    lodMeshes[i].updateCallback += UpdateCollisionMesh;
                }
            }

            // Request the heightmap data for this chunk
            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;
            UpdateTerrainChunk();
        }

        // Updates which LOD to display and whether this chunk should be visible
        public void UpdateTerrainChunk()
        {
            if (mapDataReceived)
            {
                float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

                bool wasVisible = IsVisible();
                bool visible = viewerDstFromNearestEdge <= maxViewDst;

                if (visible)
                {
                    int lodIndex = 0;

                    // Determine the correct LOD level based on viewer distance
                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    // If LOD changed, update the mesh
                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }
                }

                // Toggle visibility and update visible chunk list
                if (wasVisible != visible)
                {
                    if (visible)
                    {
                        visibleTerrainChunks.Add(this);
                    }
                    else
                    {
                        visibleTerrainChunks.Remove(this);
                    }
                    SetVisible(visible);
                }
            }
        }

        // Generates or updates the collider mesh for physics interactions
        public void UpdateCollisionMesh()
        {
            if (!hasSetCollider)
            {
                float sqrDstFromViewerToEdge = bounds.SqrDistance(viewerPosition);

                if (sqrDstFromViewerToEdge < detailLevels[colliderLODIndex].visibleDstThreshold)
                {
                    if (!lodMeshes[colliderLODIndex].hasRequestedMesh)
                    {
                        lodMeshes[colliderLODIndex].RequestMesh(mapData);
                    }
                }

                if (sqrDstFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
                {
                    if (lodMeshes[colliderLODIndex].hasMesh)
                    {
                        meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                        hasSetCollider = true;
                    }
                }
            }
        }

        // Enables or disables the chunk GameObject
        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        // Returns true if the chunk is currently active
        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }

    // Handles requesting and storing mesh data for each LOD
    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        public event System.Action updateCallback;

        public LODMesh(int lod)
        {
            this.lod = lod;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;
            updateCallback();
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        [Range(0, MeshGenerator.numSupportedLODs - 1)]
        public int lod;
        public float visibleDstThreshold;

        public float sqrVisibleDstThreshold
        {
            get { return visibleDstThreshold * visibleDstThreshold; }
        }
    }
}

