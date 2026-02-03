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
        InputManager.PlacePickaxe.performed += OnPlaceAction;
    }
    
    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.numerator / (int)Screen.currentResolution.refreshRateRatio.denominator;
    }

    private void OnPlaceAction(InputAction.CallbackContext ctx)
    {
        var pointerPosition = Pointer.current.position.ReadValue();
        var pos = _cam.ScreenToWorldPoint(pointerPosition);
        var data = _manager.GetDataAtWorldPosition(pos);
        if (data != null) return;
        
        hideOnPlace.SetActive(false);
        var pickaxeInstance = Instantiate(pickaxe);
        pickaxeInstance.transform.position = pos;
        pickaxeInstance.transform.Rotate(Vector3.forward, Random.Range(0, 360f));
        _cam.GetComponent<FollowCamera>().target = pickaxeInstance.transform;
        InputManager.PlacePickaxe.performed -= OnPlaceAction;
    }
}
