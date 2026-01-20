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
    public float maxSlingDistance = 10f;
    public float minSlingDistance = 1f;
    public float forcePerUnitDistance = 1f;
    public float directionPreviewOffset = 220f;
    
    private TilemapManager _manager;
    private Rigidbody2D _rigidbody2D;
    private AudioSource _soundEffect;
    private CircleCollider2D _circleCollider2D;
    private bool _isSlinging = false;
    private Camera _cam;

    private void Start()
    {
        _manager = FindFirstObjectByType<TilemapManager>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _soundEffect = transform.AddComponent<AudioSource>();
        _soundEffect.clip = hitClip;
        _circleCollider2D = GetComponent<CircleCollider2D>();
        _cam = Camera.main;
    }

    private void OnEnable()
    {
        slingAction.action.performed += OnSlingAction;
    }

    private void OnDisable()
    {
        slingAction.action.performed -= OnSlingAction;
    }

    private void OnSlingAction(InputAction.CallbackContext ctx)
    {
        if (!_rigidbody2D) return;
        _isSlinging = ctx.ReadValueAsButton();
        if (_isSlinging) _rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private void Update()
    {
        Vector3 worldPos;
        
        if (!_isSlinging)
        {
            if (_rigidbody2D.constraints == RigidbodyConstraints2D.None) return;
            // Do the sling


            worldPos = _cam.ScreenToWorldPoint(Pointer.current.position.ReadValue());
            worldPos.z = 0;
            var dist = Math.Min(Vector2.Distance(_rigidbody2D.position, worldPos), maxSlingDistance);
            if (dist < minSlingDistance)
            {
                _rigidbody2D.constraints = RigidbodyConstraints2D.None;
                return;
            }

            var force = forcePerUnitDistance * dist;
            var direction = ((Vector2) worldPos - _rigidbody2D.position).normalized;
            var forceVector = -direction * force;
            _rigidbody2D.constraints = RigidbodyConstraints2D.None;
            _rigidbody2D.AddForce(forceVector, ForceMode2D.Impulse);
            return;
        }
        
        worldPos = _cam.ScreenToWorldPoint(Pointer.current.position.ReadValue());
        var directionAngle = Mathf.Atan2(worldPos.y - _rigidbody2D.position.y, worldPos.x - _rigidbody2D.position.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, directionAngle + directionPreviewOffset);
        
        // Update Graphics, we are actively slinging
        // Nothing physics is done until the sling is terminated.
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
