using System;
using UnityEngine;

public class JoystickUIController : MonoBehaviour
{
    private Vector2 _screenKnobPosition;
    [SerializeField] private GameObject knob;
    private RectTransform _rectTransform;

    private void Start()
    {
        _rectTransform = knob.GetComponent<RectTransform>();
    }

    public void UpdateKnob(Vector2 screenKnobPosition)
    {
        _rectTransform.position = screenKnobPosition;
    }
}
