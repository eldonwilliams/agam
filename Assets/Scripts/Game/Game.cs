using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Internal;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour
{
    
    public MaterialRegistry materialRegistry;
    public int boundsX;
    public InputActionReference placeAction;
    public GameObject pickaxe;
    public GameObject hideOnPlace;

    private Camera _cam;
    private Tilemap _tilemap;
    private TilemapManager _manager;
    
    void Start()
    {
        _tilemap = FindFirstObjectByType<Tilemap>();
        _manager = FindFirstObjectByType<TilemapManager>();
        _cam = Camera.main;
    }

    private void OnEnable()
    {
        placeAction.action.performed += OnPlaceAction;
    }
    
    void Awake()
    {
        // Disable VSync to prevent it from overriding the target frame rate
        QualitySettings.vSyncCount = 0;

        // Set the target frame rate to the display's native refresh rate (e.g., 120 Hz)
        // This is the recommended approach for modern Unity versions and iOS
        Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.numerator / (int)Screen.currentResolution.refreshRateRatio.denominator;

        // You can also explicitly set it to 120, but the above is more robust
        // Application.targetFrameRate = 120;
    }
    
    private void OnDisable()
    {
        placeAction.action.performed -= OnPlaceAction;
    }

    private void OnPlaceAction(InputAction.CallbackContext ctx)
    {
        hideOnPlace.SetActive(false);
        var pointerPosition = Pointer.current.position.ReadValue();
        var pos = _cam.ScreenToWorldPoint(pointerPosition);
        var data = _manager.GetDataAtWorldPosition(pos);
        if (data != null) return;
        var pickaxeInstance = Instantiate(pickaxe);
        pickaxeInstance.transform.position = pos;
        pickaxeInstance.transform.Rotate(Vector3.forward, Random.Range(0, 360f));
        _cam.GetComponent<FollowCamera>().target = pickaxeInstance.transform;
        OnDisable();
    }

    void Update()
    {
        float height = _cam.orthographicSize * 2f;
        float width = height * _cam.aspect;

        Vector3 camCenter = _cam.transform.position;

        Vector3 bottomLeft = camCenter + new Vector3(-width / 2f, -height / 2f, 0);
        Vector3 topRight   = camCenter + new Vector3( width / 2f,  height / 2f, 0);
        
        Vector3Int minCell = _tilemap.WorldToCell(bottomLeft);
        Vector3Int maxCell = _tilemap.WorldToCell(topRight);
        
        for (int x = minCell.x; x <= maxCell.x; x++)
        {
            if (x < -boundsX || x > boundsX)
            {
                for (int y = minCell.y; y <= maxCell.y; y++)
                {
                    Vector3Int cellPos = new Vector3Int(x, y, 0);

                    if (_manager.HasData(new Vector2Int(x, y))) continue;
                
                    if (!_tilemap.HasTile(cellPos))
                    {
                        _manager.PlaceMaterial(new Vector2Int(x, y), materialRegistry.materials[2]);
                    }
                }
                continue;
            }
            
            for (int y = minCell.y; y <= Math.Min(0, maxCell.y); y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);

                if (_manager.HasData(new Vector2Int(x, y))) continue;
                
                if (!_tilemap.HasTile(cellPos))
                {
                    _manager.PlaceMaterial(new Vector2Int(x, y), materialRegistry.materials[y == 0 ? 0 : 1]);
                }
            }
        }

    }
}
