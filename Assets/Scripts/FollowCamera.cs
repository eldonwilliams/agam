using System;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    void Update()
    {
        if (!target) return;
        transform.position = Vector3.Lerp(transform.position, target.position + offset, Time.deltaTime * 2f);
    }
}
