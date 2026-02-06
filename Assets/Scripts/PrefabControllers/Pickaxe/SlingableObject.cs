using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SlingableObject : MonoBehaviour
{
    public GameObject joystickElement;
    public GameObject uiDotElement;
    public float maxSlingDistance = 450f;
    public float minSlingDistance = 25f;
    public float forcePerUnitDistance = 0.0195f;
    public float directionPreviewOffset = 142.5f;
    public int uiDotCount = 10;
    public float uiDotTimeDelta = 0.1f;

    private FollowCamera _cameraController;
    private Transform _canvas;
    private Rigidbody2D _rigidbody2D;
    private bool _isSlinging;
    private JoystickUIController _joystickUIController;
    private Transform[] _uiDotsTransform;
    private Vector2 _startSlingPosition;
    private Camera _cam;
    private InputAction _slingAction;

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _canvas = FindFirstObjectByType<Canvas>().transform;
        _cam = Camera.main;
        if (_cam)
            _cameraController = _cam.GetComponent<FollowCamera>();
        _slingAction = InputManager.SlingPickaxe;
        _slingAction.performed += OnSlingAction;
    }

    /**
     * Calculates the estimated position of the slung pickaxe in x time
     * assumes v0 is 0 and there is no obsticles
     * TODO: Implement a raycast for more accurate, (i.e. hit a object stop the t at p(f)
     */
    private Vector2 CalculateEstimatedPosition(float t)
    {
        var mass = _rigidbody2D.mass;
        var acceleration = CalculateSlingForce() / mass;
        var g = Physics2D.gravity * _rigidbody2D.gravityScale;
        var position = _rigidbody2D.position;
        position += g * (0.5f * (float)Math.Pow(t, 2));
        position += (_rigidbody2D.linearVelocity + acceleration) * t;
        return position;
    }

    private Vector2 CalculateSlingForce()
    {
        var pointerPosition = Pointer.current.position.ReadValue();
        var force = forcePerUnitDistance *
                    Math.Min(Vector2.Distance(_startSlingPosition, pointerPosition), maxSlingDistance);
        ;
        var direction = (pointerPosition - _startSlingPosition).normalized;
        return -direction * force;
    }

    /**
     * Updates preview graphics for a sling
     */
    private void UpdateGraphics()
    {
        var pointerPosition = Pointer.current.position.ReadValue();
        var directionAngle =
            Mathf.Atan2(pointerPosition.y - _startSlingPosition.y, pointerPosition.x - _startSlingPosition.x) *
            Mathf.Rad2Deg;
        var magnitude = Vector2.Distance(pointerPosition, _startSlingPosition);
        transform.rotation = Quaternion.Euler(0f, 0f, directionAngle + directionPreviewOffset);

        _cameraController.deflection = Vector3.ClampMagnitude(new Vector3(
            Mathf.Cos((directionAngle - 180f) * Mathf.Deg2Rad) * magnitude / 350f,
            Mathf.Sin((directionAngle - 180f) * Mathf.Deg2Rad) * magnitude / 350f,
            0), maxSlingDistance / 350f);

        if (_joystickUIController)
            _joystickUIController.UpdateKnob(_cam.ScreenToWorldPoint(_startSlingPosition +
                                                                     Vector2.ClampMagnitude(
                                                                         pointerPosition - _startSlingPosition,
                                                                         maxSlingDistance)));

        for (var i = 0; i < _uiDotsTransform.Length; i++)
        {
            var uiDot = _uiDotsTransform[i];
            uiDot.transform.position = CalculateEstimatedPosition((i + 1) * uiDotTimeDelta);
        }
    }

    /**
     * Instatiates new graphics objects, cleaning up any previous ones
     */
    private void MakeGraphics()
    {
        CleanUpGraphics();
        _startSlingPosition = Pointer.current.position.ReadValue();
        var joystick = Instantiate(joystickElement, _canvas.transform).transform;
        joystick.position = (Vector2)_cam.ScreenToWorldPoint(_startSlingPosition);
        _joystickUIController = joystick.GetComponent<JoystickUIController>();
        _uiDotsTransform = new Transform[uiDotCount];
        for (int i = 0; i < uiDotCount; i++)
        {
            var dotTransform = Instantiate(uiDotElement, _canvas.transform).transform;
            _uiDotsTransform[i] = dotTransform;
        }

        _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private void CleanUpGraphics()
    {
        _cameraController.deflection = Vector3.zero;

        if (_joystickUIController && _joystickUIController.gameObject)
            Destroy(_joystickUIController.gameObject);

        if (_uiDotsTransform == null)
            return;
        
        foreach (var uiDot in _uiDotsTransform)
        {
            if (uiDot && uiDot.gameObject)
                Destroy(uiDot.gameObject);
        }
    }

    private void PerformSling()
    {
        var pointerPosition = Pointer.current.position.ReadValue();
        var dist = Math.Min(Vector2.Distance(_startSlingPosition, pointerPosition), maxSlingDistance);
        if (dist < minSlingDistance)
        {
            _rigidbody2D.constraints = RigidbodyConstraints2D.None;
            return;
        }

        var forceVector = CalculateSlingForce();
        _rigidbody2D.constraints = RigidbodyConstraints2D.None;
        _rigidbody2D.AddForce(forceVector, ForceMode2D.Impulse);
    }

    private void OnSlingAction(InputAction.CallbackContext ctx)
    {
        StartCoroutine(OnSlingActionCoroutine(ctx));
    }

    private IEnumerator OnSlingActionCoroutine(InputAction.CallbackContext ctx)
    {
        yield return null;
        if (EventSystem.current.IsPointerOverGameObject() && !_isSlinging) yield break;
        if (!_rigidbody2D) yield break;
        _isSlinging = ctx.ReadValueAsButton();
        if (_isSlinging)
        {
            MakeGraphics();
        }
    }

    private void Update()
    {
        if (_isSlinging)
        {
            UpdateGraphics();
            return;
        }

        if (_rigidbody2D.constraints == RigidbodyConstraints2D.None) return;
        CleanUpGraphics();
        PerformSling();
    }
}