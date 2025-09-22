using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassSpawner : MonoBehaviour
{
    public Terrain terrain;
    public GameObject grassPrefab;
    public float spawnRadius = 10f; // Radius to spawn grass
    public float spacing = 2f; // Spacing between grass
    public List<Texture2D> allowedTextures;
    public int maxGrassCount = 100;
    public Transform grassParent;

    private Queue<GameObject> grassPool = new Queue<GameObject>();
    private List<GameObject> activeGrass = new List<GameObject>();
    private Camera mainCamera;

    private Vector3 lastCameraPosition; // To track camera movement

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No camera found with tag 'MainCamera'. Please ensure there is a camera in the scene tagged as 'MainCamera'.");
            return;
        }

        lastCameraPosition = mainCamera.transform.position;
        InitializeGrassPool();
        SpawnGrassBeyondView(); // Initial spawn at start
    }

    void InitializeGrassPool()
    {
        for (int i = 0; i < maxGrassCount; i++)
        {
            GameObject grass = Instantiate(grassPrefab, grassParent);
            grass.SetActive(false);
            grassPool.Enqueue(grass);
        }
    }

    void Update()
    {
        // Check if the camera has moved
        if (Vector3.Distance(mainCamera.transform.position, lastCameraPosition) > 0.1f)
        {
            lastCameraPosition = mainCamera.transform.position; // Update last position
            UpdateGrassInstances();
        }
    }

    void UpdateGrassInstances()
    {
        RemoveGrassOutOfView();
        SpawnGrassBeyondView();
    }

    void RemoveGrassOutOfView()
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

        for (int i = activeGrass.Count - 1; i >= 0; i--)
        {
            if (!GeometryUtility.TestPlanesAABB(frustumPlanes, activeGrass[i].GetComponent<Collider>().bounds))
            {
                activeGrass[i].SetActive(false);
                grassPool.Enqueue(activeGrass[i]);
                activeGrass.RemoveAt(i);
            }
        }
    }

    void SpawnGrassBeyondView()
    {
        Vector3 cameraPosition = mainCamera.transform.position;

        // Calculate the extra distance to spawn grass beyond the camera's view
        float extraSpawnDistance = 5f; // Adjust this to increase/decrease the spawning area

        for (float x = -spawnRadius; x <= spawnRadius + extraSpawnDistance; x += spacing)
        {
            for (float z = -spawnRadius; z <= spawnRadius + extraSpawnDistance; z += spacing)
            {
                if (grassPool.Count == 0) return;

                float randomOffsetX = Random.Range(-spacing, spacing);
                float randomOffsetZ = Random.Range(-spacing, spacing);

                Vector3 spawnPosition = new Vector3(cameraPosition.x + x + randomOffsetX, 0, cameraPosition.z + z + randomOffsetZ);
                spawnPosition.y = terrain.SampleHeight(spawnPosition) + terrain.GetPosition().y;

                if (IsPositionOnAllowedTexture(spawnPosition) && IsWithinCameraView(spawnPosition))
                {
                    Vector3 terrainNormal = GetTerrainNormal(spawnPosition);

                    GameObject grassInstance = grassPool.Dequeue();
                    grassInstance.transform.position = spawnPosition;
                    grassInstance.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(mainCamera.transform.forward, terrainNormal), terrainNormal);
                    grassInstance.SetActive(true);
                    activeGrass.Add(grassInstance);
                }
            }
        }
    }

    bool IsWithinCameraView(Vector3 position)
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, new Bounds(position, Vector3.one));
    }

    bool IsPositionOnAllowedTexture(Vector3 position)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPosition = position - terrain.transform.position;
        int mapX = Mathf.RoundToInt((terrainPosition.x / terrainData.size.x) * terrainData.alphamapWidth);
        int mapZ = Mathf.RoundToInt((terrainPosition.z / terrainData.size.z) * terrainData.alphamapHeight);
        float[,,] alphaMaps = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        for (int i = 0; i < terrainData.terrainLayers.Length; i++)
        {
            if (allowedTextures.Contains(terrainData.terrainLayers[i].diffuseTexture) && alphaMaps[0, 0, i] > 0.5f)
            {
                return true;
            }
        }
        return false;
    }

    Vector3 GetTerrainNormal(Vector3 position)
    {
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPosition = position - terrain.transform.position;
        int mapX = Mathf.RoundToInt((terrainPosition.x / terrainData.size.x) * terrainData.heightmapResolution);
        int mapZ = Mathf.RoundToInt((terrainPosition.z / terrainData.size.z) * terrainData.heightmapResolution);
        return terrainData.GetInterpolatedNormal((float)mapX / terrainData.heightmapResolution, (float)mapZ / terrainData.heightmapResolution);
    }
}
