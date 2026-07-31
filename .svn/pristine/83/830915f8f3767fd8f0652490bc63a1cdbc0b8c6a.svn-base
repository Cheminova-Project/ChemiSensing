using System;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using VInspector;

/// <summary>
/// Permite realizar raycasts desde la pantalla para interacción en desktop y mobile.
/// Utilizado para detectar objetos bajo el cursor o el toque.
/// </summary>
public class ScreenRaycaster : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] PointerInputActionReferences pointerInputActionReferences;
    
    [Header("Raycasting")]
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Camera _camera;
    
    [Header("Reticle")]
    [SerializeField] private Transform reticle;
    [SerializeField] private ReticleMaterialManager reticleMaterialManager;
    
    [Header("Reticle control")]
    protected bool isMovingCamera = true;
    [SerializeField] private float positionLerpSpeed = 100f;
    [SerializeField] private float rotationLerpSpeed = 100f;
    [SerializeField] float baseReticleScale = 0.0025f;
    
    [Header("Laser options")]
    [SerializeField] private bool useLaser = false;
    [SerializeField] private NetworkedLineRenderer laserLineRenderer;
    
    private InspectedObjectController inspectedObjectController;
    
    private float baseDistance;
    private Vector2 inputPointerPosition;
    private bool pointerDown;

    public UnityAction OnMovingPointerEnabled;
    public UnityAction OnMovingPointerDisabled;
    
    public UnityAction<Transform> onSelect;
    public UnityAction<Transform> onDeselect;
    
    private Transform inspectedObject;
    
    protected virtual void OnEnable()
    {
        InitListeners();
        reticle.localScale = Vector3.one * baseReticleScale;
        pointerDown = false;
        baseDistance = _camera.nearClipPlane * 2f;
        if(InspectedObjectController.Instance == null)
            InspectedObjectController.OnInspectedObjectControllerInitialized += InitInspectedObjectController;
        else
            InitInspectedObjectController();
    }

    private void InitInspectedObjectController()
    {
        inspectedObjectController = InspectedObjectController.Instance;
        inspectedObjectController.OnInspectedObjectLoaded += FindInspectedObject;
    }
    
    private void FindInspectedObject()
    {
        inspectedObject = inspectedObjectController.GetInspectedObject()?.transform;
    }

    private void OnDisable()
    {
        DestroyListeners();
    }

    protected virtual void DestroyListeners()
    {
        pointerInputActionReferences.PointerPress.action.performed -= OnPointerPress;
        pointerInputActionReferences.PointerPress.action.canceled -= OnPointerRelease;
        
        InspectedObjectController.OnInspectedObjectControllerInitialized -= InitInspectedObjectController;
        if (inspectedObjectController != null)
            inspectedObjectController.OnInspectedObjectLoaded -= FindInspectedObject;
    }

    protected virtual void InitListeners()
    {
        pointerInputActionReferences.PointerPress.action.Enable();
        pointerInputActionReferences.PointerPosition.action.Enable();
        pointerInputActionReferences.PointerPress.action.performed += OnPointerPress;
        pointerInputActionReferences.PointerPress.action.canceled += OnPointerRelease;
    }
    
    public void EnableLaser(bool b)
    {
        useLaser = b;
    }
    
    protected void OnCameraMoveStateChanged(bool enable)
    {
        isMovingCamera = enable;
        
        if (!isMovingCamera)
            OnMovingPointerDisabled?.Invoke();
        else
            OnMovingPointerEnabled?.Invoke();
    }
    private void OnPointerPress(InputAction.CallbackContext pointerPressEvent)
    {
        if (pointerPressEvent.performed)
            pointerDown = true;
        else if (pointerPressEvent.canceled)
            pointerDown = false;
    }
    
    private void OnPointerRelease(InputAction.CallbackContext pointerReleaseEvent)
    {
        pointerDown = false;
    }

    private void FixedUpdate()
    {
        Vector2 rawInputPos = Vector2.zero;
        
        if (Mouse.current != null)
            rawInputPos = Mouse.current.position.ReadValue();
        else
            rawInputPos = pointerInputActionReferences.PointerPosition.action.ReadValue<Vector2>();
        
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
                        rawInputPos = touch.position.ReadValue();
                        touchDetected = true;
                    }
                }
            }
        }
        
        inputPointerPosition = new Vector2(
            Mathf.Clamp(rawInputPos.x, 0, Screen.width),
            Mathf.Clamp(rawInputPos.y, 0, Screen.height)
        );
        
        Ray ray = _camera.ScreenPointToRay(inputPointerPosition);
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask);
        
        Vector3 targetPosition = hitSomething
            ? hit.point + hit.normal * 0.00001f
            : _camera.ScreenToWorldPoint(new Vector3(inputPointerPosition.x, inputPointerPosition.y, baseDistance));

        Quaternion targetRotation = hitSomething
            ? Quaternion.LookRotation(-hit.normal)
            : Quaternion.LookRotation(_camera.transform.forward);

        reticle.position = Vector3.Lerp(reticle.position, targetPosition, Time.deltaTime * positionLerpSpeed);
        
        if (!hitSomething || isMovingCamera)
        {
            reticle.gameObject.SetActive(false);
            reticleMaterialManager.SetDefaultColor();
            laserLineRenderer.SetPoints(new List<Vector3>());
        }
        else
        {
            reticle.gameObject.SetActive(true);
            float actualDistance = Vector3.Dot(reticle.position - _camera.transform.position, _camera.transform.forward);
            float scaleFactor = actualDistance / baseDistance;
            reticle.rotation = Quaternion.Slerp(reticle.rotation, targetRotation, Time.deltaTime * rotationLerpSpeed);
            reticle.localScale = Vector3.one * baseReticleScale * scaleFactor;
            reticleMaterialManager.SetHighlightColor();
            
            if(useLaser)
            {
                List<Vector3> laserPoints = new List<Vector3>
                {
                    _camera.transform.position,
                    hit.point
                };
                laserLineRenderer.SetPoints(laserPoints);
                reticleMaterialManager.SetHighlightColor();
            }
            else
            {
                laserLineRenderer.SetPoints(new List<Vector3>());
                reticleMaterialManager.SetDefaultColor();
            }
        }
    }

    public void SelectInspectedObject()
    {
        if (inspectedObject)
            onSelect?.Invoke(inspectedObject);
        else
        {
            Debug.LogError("No inspected object to select");
        }
    }

    public void DeselectCurrentTarget()
    {
        if (inspectedObject)
            onDeselect?.Invoke(inspectedObject);
        else
        {
            Debug.LogError("No inspected object to deselect");
        }
    }
    
    public void SetLaserColor(Color newColor)
    {
        if (laserLineRenderer != null)
        {
            // Ahora usamos el método sincronizado por red
            laserLineRenderer.SetColor(newColor);
        }
    }
}
