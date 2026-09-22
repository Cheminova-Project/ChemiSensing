using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class SelectPointMouse : MonoBehaviour
{
    public LayerMask interestLayer;
    private Vector3 alterationPoint;
    private bool _isSelecting;
    private Camera mainCamera;
    private Vector2 startInputPosition;
    private bool isDragging;
    private bool startedOnUI;
    private const float dragThreshold = 15f;
    private bool wasPressed;
    private Vector2 lastPointerPosition;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (!_isSelecting) 
            return;
        
        if (FindAnyObjectByType<AnnotationManager>() == null)
        {
            EnableSelectingPoint(false);
            return;
        }

        bool isCurrentlyPressed = false;
        Vector2 currentPosition = Vector2.zero;

        if (Mouse.current != null)
        {
            isCurrentlyPressed = Mouse.current.leftButton.isPressed;
            currentPosition = Mouse.current.position.ReadValue();
        }

        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            bool touchDetected = false;
            foreach (var touch in Touchscreen.current.touches)
            {
                var phase = touch.phase.ReadValue();
                if (phase == UnityEngine.InputSystem.TouchPhase.Began ||
                    phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                    phase == UnityEngine.InputSystem.TouchPhase.Stationary)
                {
                    if (!touchDetected)
                    {
                        isCurrentlyPressed = true;
                        currentPosition = touch.position.ReadValue();
                        touchDetected = true;
                    }
                }
            }
        }

        if (isCurrentlyPressed)
            lastPointerPosition = currentPosition;
        
        bool isPressed = isCurrentlyPressed;

        bool pressedThisFrame = isPressed && !wasPressed;
        bool releasedThisFrame = !isPressed && wasPressed;

        if (pressedThisFrame)
        {
            if (IsPointerOverUI())
            {
                startedOnUI = true;
            }
            else
            {
                startedOnUI = false;
                startInputPosition = lastPointerPosition;
                isDragging = false;
            }
        }
        else if (releasedThisFrame)
        {
            if (!startedOnUI)
            {
                float distance = Vector2.Distance(startInputPosition, lastPointerPosition);
                
                if (distance > dragThreshold)
                    isDragging = true;

                if (!isDragging)
                    ProcessClick(lastPointerPosition);
            }
        }

        wasPressed = isPressed;
    }

    private void ProcessClick(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, interestLayer))
        {
            Vector3 point = hit.point;
            alterationPoint = point;
        }
    }
    
    public void EnableSelectingPoint(bool enable)
    {
        _isSelecting = enable;
    }
    
    private bool IsPointerOverUI()
    {
        if (EventSystem.current != null)
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return true;

            if (Pointer.current != null && EventSystem.current.IsPointerOverGameObject(Pointer.current.deviceId))
                return true;
        }
        return false;
    }
}