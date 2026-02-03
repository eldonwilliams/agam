using System;
using System.Collections.Generic;
using Solo.MOST_IN_ONE;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class DamageSource : MonoBehaviour
{
    public AudioClip hitClip;

    private TilemapManager _manager;
    private Rigidbody2D _rigidbody2D;
    private AudioSource _soundEffect;

    private void Start()
    {
        _manager = FindFirstObjectByType<TilemapManager>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _soundEffect = transform.AddComponent<AudioSource>();
        _soundEffect.clip = hitClip;
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
        }
    }
}
