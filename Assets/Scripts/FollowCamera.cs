using System;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    public Vector3 deflection;
    public float speed = 3.5f;

    private Camera _camera;
    private float _startingCamSize;

    void Start()
    {
        _camera = GetComponent<Camera>();
        _startingCamSize = _camera.orthographicSize;
    }

    void Update()
    {
        if (!target) return;

        var targetViewportPosition = _camera.WorldToViewportPoint(target.position);
        if ((Mathf.Abs(targetViewportPosition.x) > 1f || Mathf.Abs(targetViewportPosition.y) > 1f) && _camera.orthographicSize < _startingCamSize * 2f)
        {
            _camera.orthographicSize += 1f * Time.deltaTime;
        }
        else if (_camera.orthographicSize > _startingCamSize)
        {
            _camera.orthographicSize -= 1f * Time.deltaTime;
        }
        else
        {
            _camera.orthographicSize = _startingCamSize;
        }
        
        transform.position = Vector3.Lerp(transform.position, target.position + offset + deflection, Time.deltaTime * speed);
    }
}
