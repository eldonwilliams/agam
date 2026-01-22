using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    // Properties for the terrain generator
    public int chunkSize = 16;
    // The amount of chunks to generate surrounding 
    public int paddingChunks = 2;
    public int bedrockWallPosition = 10;
    public MaterialRegistry materialRegistry;
    public float perlinMapSize = 25f;
    
    // A list of currently loaded chunks
    private readonly HashSet<Vector2Int> _loadedChunks = new();
    private TilemapManager _tilemapManager;
    private Vector2 _seed;

    private void Start()
    {
        _seed = Random.insideUnitSphere;
        _tilemapManager = FindFirstObjectByType<TilemapManager>();
    }

    /**
     * Fetches the bounding box a cam should use for generating terrain with GenerateTerrainForBoundingBox
     */
    public BoundsInt BoundingBoxForCamera(Camera cam)
    {
        var height = cam.orthographicSize * 2f;
        var width = height * cam.aspect;

        var camCenter = cam.transform.position;

        var bottomLeft = camCenter + new Vector3(-width / 2f, -height / 2f, 0);
        var topRight = camCenter + new Vector3(width / 2f, height / 2f, 0);
        
        var minChunk = TilemapToChunk(_tilemapManager.WorldToCell(bottomLeft));
        var maxChunk = TilemapToChunk(_tilemapManager.WorldToCell(topRight));

        var bounds = new BoundsInt();
        bounds.SetMinMax((Vector3Int)minChunk, (Vector3Int)maxChunk);
        return bounds;
    }

    /**
     * Generates terrain within a given bounding box,
     * the box is also padding using the provided padding chunks property
     */
    public void GenerateTerrainForBoundingBox(BoundsInt box)
    {
        box.min -= new Vector3Int(paddingChunks, paddingChunks, 0);
        box.max += new Vector3Int(paddingChunks, paddingChunks, 0);
        
        // Check the corners of the box, if they have already been generated then the entire box has been generated
        // This is two contains calls which is most cases will save n^2 time complexity
        if (_loadedChunks.Contains((Vector2Int) box.min) && _loadedChunks.Contains((Vector2Int) box.max))
            return;
        
        for (var x = box.min.x; x < box.max.x; x++)
        for (var y = box.min.y; y < box.max.y; y++)
            GenerateChunk(new Vector2Int(x, y));
    }
    
    /**
     * Attempts to generate a chunk at the given chunk coordinates,
     * returns true if the chunk was successfully generated,
     * otherwise returns false.
     */
    private bool GenerateChunk(Vector2Int chunk)
    {
        if (_loadedChunks.Contains(chunk))
            return false;

        for (int x = 0; x < chunkSize; x++)
        for (int y = 0; y < chunkSize; y++)
        {
            var tilemapPosition = ChunkToTilemap(chunk, new Vector2Int(x, y));
            var material = MaterialToGenerateAt(tilemapPosition);
            _tilemapManager.PlaceMaterial(tilemapPosition, material);
        }
        
        return _loadedChunks.Add(chunk);
    }

    private MaterialBase MaterialToGenerateAt(Vector2Int tilemapPosition)
    {
        // This is the logic that truly will control terrain generation
        // This is a major feat for the future, currently the bedrock material will be generated at the sides and the rest is a layer of dirt and stone to infinity

        
        
        if (Mathf.Abs(tilemapPosition.x) == bedrockWallPosition)
            return materialRegistry.materials[2];

        if (tilemapPosition.y > 0 || Mathf.Abs(tilemapPosition.x) > bedrockWallPosition)
            return null;

        var perlinNoiseValue = Mathf.PerlinNoise(tilemapPosition.x / perlinMapSize + _seed.x, tilemapPosition.y / perlinMapSize + _seed.y);
        return materialRegistry.materials[perlinNoiseValue <= 0.2f ? 0 : 1];
    }

    public Vector2Int ChunkToTilemap(Vector2Int chunk)
    {
        return ChunkToTilemap(chunk, Vector2Int.zero);
    }

    public Vector2Int ChunkToTilemap(Vector2Int chunk, Vector2Int position)
    {
        // If the position is not within a chunk, adjust it
        // if (position.x >= chunkSize || position.y >= chunkSize || position.x < 0 || position.y < 0)
        // {
        //     chunk = new Vector2Int(chunk.x + position.x / chunkSize, chunk.y + position.y / chunkSize);
        //     position.x %= chunkSize;
        //     position.y %= chunkSize;
        // }
        return chunk * chunkSize + position;
    }

    public Vector2Int WorldToChunk(Vector3 worldPos)
    {
        var tilemapPosition = _tilemapManager.WorldToCell(worldPos);
        return TilemapToChunk(tilemapPosition);
    }
    
    public Vector2Int TilemapToChunk(Vector2Int tilemapPosition)
    {
        var chunkPosition = new Vector2Int(tilemapPosition.x / chunkSize, tilemapPosition.y / chunkSize);
        return chunkPosition;
    }
}
