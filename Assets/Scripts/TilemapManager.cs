using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class TilemapManager : MonoBehaviour
{
    public AudioClip destroyClip;
    public GameObject breakFx;
    public Material baseBreakFxMaterial;
    
    public Action<Vector2Int, TileData> OnTileDamaged;
    
    /**
     * The storage for all data
     *
     * While I recognize a Vector3Int keyed dictionary is HORRIBLY inefficient,
     * I will allow this to become a point of optimization in the future
     * Ideas for efficiency: use a quad-tree, don't store data until a block is altered (as most might not be altered)
     * TODO: Optimize
     */
    private Dictionary<Vector2Int, TileData> _data = new();

    private Dictionary<MaterialBase, Material> _breakEffectMaterialLookup = new();

    private Tilemap _tilemap;
    private AudioSource _destroyAudioSource;
    
    void Start()
    {
        _tilemap = GetComponent<Tilemap>();
        OnTileDamaged += ShouldDamagedCellBeDestroyed;
        _destroyAudioSource = transform.AddComponent<AudioSource>();
        _destroyAudioSource.clip = destroyClip;
    }

    /**
     * Sets data at a position using the default data of a material
     */
    public void SetData(Vector2Int pos, MaterialBase material)
    {
        TileData emptyData = new TileData(material);
        SetData(pos, emptyData);
    }

    public void SetData(Vector2Int pos, TileData data)
    {
        _data.Add(pos, data);
    }

    public TileData GetData(Vector2Int pos)
    {
        return _data.GetValueOrDefault(pos, null);
    }

    public bool HasData(Vector2Int pos)
    {
        return _data.ContainsKey(pos);
    }

    public void DamageTile(Vector2Int pos, float amount)
    {
        if (!_data.TryGetValue(pos, out var tileData))
            return;
        // If the default health is < 0 (i.e. -1) the tile should be undamagable
        if (tileData.Material.defaultHealth < 0)
            return;
        
        // If the tile is already dead, return
        if (tileData.Health <= 0.0f)
            return;
        
        tileData.Health = Math.Max(0, tileData.Health - amount);
        OnTileDamaged.Invoke(pos, tileData);
    }

    /**
     * Places a tile of material at a given position. Also handles the data part
     */
    public void PlaceMaterial(Vector2Int pos, MaterialBase material)
    {
        SetData(pos, material);
        _tilemap.SetTile(new Vector3Int(pos.x, pos.y), material.tile);
    }

    public Vector2Int WorldToCell(Vector3 pos)
    {
        Vector3Int cellPos = _tilemap.WorldToCell(pos);
        return new Vector2Int(cellPos.x, cellPos.y);
    }
    
    public Vector3 CellToWorld(Vector2Int cell)
    {
        return _tilemap.CellToWorld(new Vector3Int(cell.x, cell.y, 0));
    }

    public Vector3 CellCenterToWorld(Vector2Int cell)
    {
        return _tilemap.GetCellCenterWorld(new Vector3Int(cell.x, cell.y));
    }

    public bool IsTileDead(Vector2Int cell)
    {
        return GetData(cell)?.Health <= 0f;
    }
    
    public TileData GetDataAtWorldPosition(Vector3 pos)
    {
        return GetData(WorldToCell(pos));
    }

    private Material FetchBreakMaterial(MaterialBase materialBase)
    {
        if (_breakEffectMaterialLookup.TryGetValue(materialBase, out var value)) return value;

        var newMaterial = Instantiate(baseBreakFxMaterial);
        newMaterial.mainTexture = ((Tile) materialBase.tile).sprite.texture;
        _breakEffectMaterialLookup.Add(materialBase, newMaterial);
        return newMaterial;
    }

    private void CellDestroyedBreakEffect(Vector2Int pos, TileData tileData)
    {
        var breakParticles = Instantiate(breakFx);
        breakParticles.transform.position = _tilemap.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));
        var particleSystem = breakParticles.GetComponent<ParticleSystem>();
        particleSystem.GetComponent<ParticleSystemRenderer>().material = FetchBreakMaterial(tileData.Material);
        particleSystem.Play();
        Destroy(breakParticles, 5.0f);
    }

    public void ShouldDamagedCellBeDestroyed(Vector2Int pos, TileData tileData)
    {
        if (tileData.Health > 0f) return;

        CellDestroyedBreakEffect(pos, tileData);
        
        _destroyAudioSource.pitch = 1f + Random.Range(-0.1f, 0.1f);
        _destroyAudioSource.Play();
        _tilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), null); 
        // _tilemap.GetComponent<TilemapCollider2D>().ProcessTilemapChanges();
    }
}
