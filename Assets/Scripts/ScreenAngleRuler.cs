using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class ScreenAngleRuler : AngleRuler
{
    private Transform originalObject;
    private Vector2 inputPointerPosition;
    private bool alreadyPressed;
    private Camera raycastCamera;

    [SerializeField] private LayerMask layerMask;
    [SerializeField] private PointerInputActionReferences pointerInputActionReferences;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        raycastCamera = Camera.main;
        pointerInputActionReferences.m_pointerPress.action.Enable();
        pointerInputActionReferences.m_pointerPositon.action.Enable();
        alreadyPressed = false;
    }

    protected override void Update()
    {
        base.Update();
        
        if(originalObject == null)
        {
            originalObject = GetInspectedObject();
            SetMeasuredObject(originalObject);
        }
        
        bool isPointerDown = false;
        Vector2 rawInputPos = Vector2.zero;
        
        if (Mouse.current != null)
        {
            isPointerDown = Mouse.current.leftButton.isPressed;
            rawInputPos = Mouse.current.position.ReadValue();
        }
        else
        {
            isPointerDown = pointerInputActionReferences.m_pointerPress.action.ReadValue<float>() > 0.5f;
            rawInputPos = pointerInputActionReferences.m_pointerPositon.action.ReadValue<Vector2>();
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
                        isPointerDown = true;
                        rawInputPos = touch.position.ReadValue();
                        touchDetected = true;
                    }
                }
            }
        }

        if (isPointerDown)
        {
            if (!alreadyPressed)
            {
                alreadyPressed = true;
                
                inputPointerPosition = new Vector2(
                    Mathf.Clamp(rawInputPos.x, 0, Screen.width),
                    Mathf.Clamp(rawInputPos.y, 0, Screen.height)
                );
                
                TryAddPoint();
            }
        }
        else
            alreadyPressed = false;
    }
    
    private void TryAddPoint()
    {
        if (originalObject == null) return;
        if (IsPointerOverUI()) return;

        Ray ray = raycastCamera.ScreenPointToRay(inputPointerPosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            RequestAddPoint(hit.point);
    }
    
    private bool IsPointerOverUI()
    {
        if (uIDocument != null && uIDocument.rootVisualElement != null && uIDocument.rootVisualElement.panel != null)
        {
            Vector2 uiPos = RuntimePanelUtils.ScreenToPanel(
                uIDocument.rootVisualElement.panel, 
                inputPointerPosition 
            );
            
            var picked = uIDocument.rootVisualElement.panel.Pick(uiPos);
            
            if (picked != null && picked != uIDocument.rootVisualElement)
            {
                var current = picked;
                while (current != null && current != uIDocument.rootVisualElement)
                {
                    float alpha = current.resolvedStyle.backgroundColor.a;
                    var bg = current.resolvedStyle.backgroundImage;
                    bool hasImage = bg.texture != null || bg.sprite != null || bg.vectorImage != null || bg.renderTexture != null;

                    if (alpha > 0.01f || hasImage) return true;
                    if (current == picked && current is TextElement) return true;

                    current = current.parent;
                }
                return false;
            }
        }

        if (EventSystem.current != null)
        {
            if (EventSystem.current.IsPointerOverGameObject()) return true;
            if (Pointer.current != null && EventSystem.current.IsPointerOverGameObject(Pointer.current.deviceId)) return true;
        }

        return false;
    }

    private Transform GetInspectedObject()
    {
        if (InspectedObjectController.Instance != null)
        {
            var inspectedObj = InspectedObjectController.Instance.GetInspectedObject();
            if (inspectedObj != null) return inspectedObj.transform;
        }
        return GameObject.FindGameObjectWithTag("InspectedObject")?.transform;
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
        originalObject = null;
        alreadyPressed = false;
    }
}