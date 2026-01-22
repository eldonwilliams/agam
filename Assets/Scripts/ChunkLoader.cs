using UnityEngine;

/**
 * This monobehaviour is attached to an object to cause it to update terrain
 * This may operate in two modes,
 * for normal objects with a transform
 * - Loads the current chunk (and those within the padding margin)
 * for camera objects
 * - Loads all chunks 
 */
public class ChunkLoader : MonoBehaviour
{
    private TerrainGenerator _terrainGenerator;
    private Camera _cam;
    
    void Start()
    {
        _terrainGenerator = FindFirstObjectByType<TerrainGenerator>();
        _cam = GetComponent<Camera>();
    }
    
    void Update()
    {
        if (_cam)
            UpdateCamera();
        else
            UpdateTransform();
    }

    private void UpdateCamera()
    {
        var boundingBox = _terrainGenerator.BoundingBoxForCamera(_cam);
        _terrainGenerator.GenerateTerrainForBoundingBox(boundingBox);
    }

    private void UpdateTransform()
    {
        var transformPositionChunk = _terrainGenerator.WorldToChunk(transform.position);
        var boundingBox = new BoundsInt((Vector3Int)transformPositionChunk, new Vector3Int(1, 1, 0));
        _terrainGenerator.GenerateTerrainForBoundingBox(boundingBox);
    }
}
