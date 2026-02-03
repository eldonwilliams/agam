using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Manager;
    public static InputAction PlacePickaxe => Manager._input.actions.FindAction("Place");
    public static InputAction SlingPickaxe => Manager._input.actions.FindAction("BeginSling");
    
    private PlayerInput _input;
    
    void Awake()
    {
        if (Manager)
        {
            Destroy(gameObject);
            return;
        }

        Manager = this;
        _input = GetComponent<PlayerInput>();
        // DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        _input.actions.Enable();
    }
}