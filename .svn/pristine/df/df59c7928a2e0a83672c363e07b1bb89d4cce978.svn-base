using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIVirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [System.Serializable]
    public class Event : UnityEvent<Vector2> { }

    [Header("Rect References")]
    public RectTransform containerRect;
    public RectTransform handleRect;

    [Header("Settings")]
    public float joystickRange = 50f;
    public float magnitudeMultiplier = 1f;
    public bool invertXOutputValue;
    public bool invertYOutputValue;

    [Header("Output")]
    public Event joystickOutputEvent;
    
    [Header("Visibility")]
    public List<Image> joystickVisibleElements;
    private bool isPointerDown = false;
    private Vector2 currentOutput = Vector2.zero;

    private bool allowMovement = false;

    private void Start()
    {
        SetupHandle();
        ShowJoystick(false);
    }

    public void SetupHandle()
    {
        if (handleRect)
            UpdateHandleRectPosition(Vector2.zero);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        OnDrag(eventData);
    }

    public bool IsEnabled()
    {
        return allowMovement;
    }
    public void ShowJoystick(bool show = true)
    {
        foreach (var element in joystickVisibleElements)
        {
            if (element != null)
                element.enabled = show;
        }
    }

    public void EnableJoystick(bool b)
    {
        allowMovement = b;
        ShowJoystick(false);
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (!isPointerDown || !allowMovement)
        {
            Debug.Log("Pointer down: " + isPointerDown + ", Allow Movement: " + allowMovement + ". Not processing drag.");
            return;
        }
            
            
        RectTransformUtility.ScreenPointToLocalPointInRectangle(containerRect, eventData.position, eventData.pressEventCamera, out Vector2 position);

        position = ApplySizeDelta(position);

        Vector2 clampedPosition = ClampValuesToMagnitude(position);

        Vector2 outputPosition = ApplyInversionFilter(clampedPosition); // Corregido: usar clamped para output
        currentOutput = outputPosition * magnitudeMultiplier;

        OutputPointerEventValue(currentOutput);

        if (handleRect)
        {
            UpdateHandleRectPosition(clampedPosition * joystickRange);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
        
        currentOutput = Vector2.zero;
        OutputPointerEventValue(Vector2.zero);

        if (handleRect)
        {
            UpdateHandleRectPosition(Vector2.zero);
        }
    }

    private void Update()
    {
        if (isPointerDown && currentOutput != Vector2.zero)
        {
            OutputPointerEventValue(currentOutput);
        }
    }

    private void OutputPointerEventValue(Vector2 pointerPosition)
    {
        joystickOutputEvent.Invoke(pointerPosition);
    }

    private void UpdateHandleRectPosition(Vector2 newPosition)
    {
        handleRect.anchoredPosition = newPosition;
    }

    Vector2 ApplySizeDelta(Vector2 position)
    {
        float x = (position.x / containerRect.sizeDelta.x) * 2.5f;
        float y = (position.y / containerRect.sizeDelta.y) * 2.5f;
        return new Vector2(x, y);
    }

    Vector2 ClampValuesToMagnitude(Vector2 position)
    {
        return Vector2.ClampMagnitude(position, 1);
    }

    Vector2 ApplyInversionFilter(Vector2 position)
    {
        if (invertXOutputValue) position.x = -position.x;
        if (invertYOutputValue) position.y = -position.y;
        return position;
    }
}
