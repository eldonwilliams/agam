using System;
using System.Collections.Generic;
using Solo.MOST_IN_ONE;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class DestroyOnHit : MonoBehaviour
{
    public AudioClip hitClip;
    public InputActionReference slingAction;
    public GameObject joystickElement;
    public GameObject uiDotElement;
    public float maxSlingDistance = 10f;
    public float minSlingDistance = 1f;
    public float forcePerUnitDistance = 1f;
    public float directionPreviewOffset = 220f;
    public int uiDotCount = 6;
    public float uiDotTimeDelta = 0.25f;

    private FollowCamera _cameraController;
    private Transform _canvas;
    private TilemapManager _manager;
    private Rigidbody2D _rigidbody2D;
    private AudioSource _soundEffect;
    private CircleCollider2D _circleCollider2D;
    private bool _isSlinging = false;
    private JoystickUIController _joystickUIController;
    private Transform[] _uiDotsTransform;
    private Vector2 _startSlingPosition;
    private Camera _cam;

    private void Start()
    {
        _manager = FindFirstObjectByType<TilemapManager>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _soundEffect = transform.AddComponent<AudioSource>();
        _soundEffect.clip = hitClip;
        _circleCollider2D = GetComponent<CircleCollider2D>();
        _canvas = FindFirstObjectByType<Canvas>().transform;
        _cam = Camera.main;
        if (_cam)
            _cameraController = _cam.GetComponent<FollowCamera>();
    }

    private void OnEnable()
    {
        slingAction.action.performed += OnSlingAction;
    }

    private void OnDisable()
    {
        slingAction.action.performed -= OnSlingAction;
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
        var force = forcePerUnitDistance * Math.Min(Vector2.Distance(_startSlingPosition, pointerPosition), maxSlingDistance);;
        var direction = (pointerPosition - _startSlingPosition).normalized;
        return -direction * force;
    }

    private void OnSlingAction(InputAction.CallbackContext ctx)
    {
        if (!_rigidbody2D) return;
        _isSlinging = ctx.ReadValueAsButton();
        if (_isSlinging)
        {
            _startSlingPosition = Pointer.current.position.ReadValue();
            var joystick = Instantiate(joystickElement, _canvas.transform).transform;
            joystick.position = (Vector2) _cam.ScreenToWorldPoint(_startSlingPosition);
            _joystickUIController = joystick.GetComponent<JoystickUIController>();
            _uiDotsTransform = new Transform[uiDotCount];
            for (int i = 0; i < uiDotCount; i++)
            {
                var dotTransform = Instantiate(uiDotElement, _canvas.transform).transform;
                _uiDotsTransform[i] = dotTransform;
            }
            _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    private void Update()
    {
        var pointerPosition = Pointer.current.position.ReadValue();
        
        if (!_isSlinging)
        {
            if (_rigidbody2D.constraints == RigidbodyConstraints2D.None) return;
            // Clean up

            _cameraController.deflection = Vector3.zero;
            
            Destroy(_joystickUIController.gameObject);
            foreach (var uiDot in _uiDotsTransform)
            {
                Destroy(uiDot.gameObject);
            }
            
            // Do the sling
            var dist = Math.Min(Vector2.Distance(_startSlingPosition, pointerPosition), maxSlingDistance);
            if (dist < minSlingDistance)
            {
                _rigidbody2D.constraints = RigidbodyConstraints2D.None;
                return;
            }
            var forceVector = CalculateSlingForce();
            _rigidbody2D.constraints = RigidbodyConstraints2D.None;
            _rigidbody2D.AddForce(forceVector, ForceMode2D.Impulse);
            return;
        }
        
        // Update Graphics, we are actively slinging
        // Nothing physics is done until the sling is terminated.

        var directionAngle = Mathf.Atan2(pointerPosition.y - _startSlingPosition.y, pointerPosition.x - _startSlingPosition.x) * Mathf.Rad2Deg;
        var magitude = Vector2.Distance(pointerPosition, _startSlingPosition);
        transform.rotation = Quaternion.Euler(0f, 0f, directionAngle + directionPreviewOffset);
        
        _cameraController.deflection = new Vector3(Mathf.Cos((directionAngle - 180f) * Mathf.Deg2Rad) * magitude / 350f,
            Mathf.Sin((directionAngle - 180f) * Mathf.Deg2Rad) * magitude / 350f,
            0);
        
        if (_joystickUIController)
            _joystickUIController.UpdateKnob(_cam.ScreenToWorldPoint(_startSlingPosition + Vector2.ClampMagnitude(pointerPosition - _startSlingPosition, maxSlingDistance)));

        for (int i = 0; i < _uiDotsTransform.Length; i++)
        {
            var uiDot = _uiDotsTransform[i];
            uiDot.transform.position = CalculateEstimatedPosition((i + 1) * uiDotTimeDelta);
        }
    }


    private void OnCollisionEnter2D(Collision2D other)
    {
        _soundEffect.pitch = 1.0f + Random.Range(-0.1f, 0.1f);
        _soundEffect.Play();
        MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.SoftImpact);
        _rigidbody2D.AddTorque(Random.Range(-2f, 2f), ForceMode2D.Force);
        var damagedTiles = new HashSet<Vector2Int>();
        foreach (var contact in other.contacts)
        {
            var cellPos = _manager.WorldToCell(contact.point - contact.normal * 0.2f);
            if (!damagedTiles.Add(cellPos)) continue; // Don't double damage a cell
            _manager.DamageTile(cellPos, 1);
            // totalNormal += contact.normal * 2;
        }
        // _rigidbody2D.AddForce(totalNormal, ForceMode2D.Impulse);
    }
}
