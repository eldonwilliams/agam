using System.Collections.Generic;
using UnityEngine;

public class DamageIndicatorManager : MonoBehaviour
{
    public GameObject prefab;

    private Dictionary<Vector2Int, GameObject> _indicators = new();
    private TilemapManager _manager; 
    
    void Start()
    {
        _manager = FindFirstObjectByType<TilemapManager>();
        _manager.OnTileDamaged += OnTileDamaged;
    }

    void OnTileDamaged(Vector2Int pos, TileData tileData)
    {
        // If tile isnt damaged, skip
        if (tileData.Health >= tileData.Material.defaultHealth) return;
        if (tileData.Health <= 0)
        {
            if (_indicators.TryGetValue(pos, out var toDestroy))
            {
                _indicators.Remove(pos);
                Destroy(toDestroy);
            }
            return;
        }

        if (!_indicators.TryGetValue(pos, out var indicator))
        {
            indicator = Instantiate(prefab, transform);
            indicator.transform.position = _manager.CellToWorld(pos) + new Vector3(0,0,-1);
            _indicators.Add(pos, indicator);
        }
        
        indicator.GetComponent<DamageIndicatorSprite>().UpdateSprite(tileData.Health / tileData.Material.defaultHealth);
    }
}
