using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Unity.Netcode;

public class SelectPointAnnotation : MonoBehaviour
{
    public LayerMask interestLayer;
    private Vector3 alterationPoint;
    private bool _isSelecting;
    private Camera mainCamera;
    
    [Header("VR Input")]
    public XRInputActionReferences xRInputActionReferences;

    private Transform rightControllerTransform;
    private XRRayInteractor rightXrRayInteractor;
    private Transform leftControllerTransform;
    private XRRayInteractor leftXrRayInteractor;
    private Transform activeControllerTransform;
    private XRRayInteractor activeXrRayInteractor;

    private Vector2 startInputPosition;
    private Vector3 startControllerPosition;
    private bool isDragging;
    private bool startedOnUI;
    private const float dragThreshold2D = 15f;
    private const float dragThresholdVR = 0.1f;

    private bool wasPressed;
    private Vector2 lastPointerPosition;

    private void Start()
    {
        EnsureMainCamera();
        StartCoroutine(EnsureVRInputEnabled());
    }
    
    private IEnumerator EnsureVRInputEnabled()
    {
        yield return new WaitForEndOfFrame();
        if (xRInputActionReferences != null)
        {
            if (xRInputActionReferences.rightTriggerPressed != null && !xRInputActionReferences.rightTriggerPressed.action.enabled)
                xRInputActionReferences.rightTriggerPressed.action.Enable();

            if (xRInputActionReferences.leftTriggerPressed != null && !xRInputActionReferences.leftTriggerPressed.action.enabled)
                xRInputActionReferences.leftTriggerPressed.action.Enable();
        }
    }

    private void EnsureVRReferences()
    {
        if (rightControllerTransform == null || leftControllerTransform == null)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
            {
                GameObject localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
                Transform[] allChildren = localPlayer.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in allChildren)
                {
                    if (child.CompareTag("ControllerR")) rightControllerTransform = child;
                    else if (child.CompareTag("ControllerL")) leftControllerTransform = child;
                }
            }
            else
            {
                rightControllerTransform = GameObject.FindGameObjectWithTag("ControllerR")?.transform;
                leftControllerTransform = GameObject.FindGameObjectWithTag("ControllerL")?.transform;
            }
            
            if (rightControllerTransform != null)
            {
                rightXrRayInteractor = rightControllerTransform.GetComponent<XRRayInteractor>();
                if (rightXrRayInteractor == null) rightXrRayInteractor = rightControllerTransform.GetComponentInChildren<XRRayInteractor>();
            }

            if (leftControllerTransform != null)
            {
                leftXrRayInteractor = leftControllerTransform.GetComponent<XRRayInteractor>();
                if (leftXrRayInteractor == null) leftXrRayInteractor = leftControllerTransform.GetComponentInChildren<XRRayInteractor>();
            }
        }
    }

    void Update()
    {
        if (!_isSelecting) return;
        
        if (FindAnyObjectByType<AnnotationManager>() == null)
        {
            EnableSelectingPoint(false);
            return;
        }

        EnsureVRReferences();

        bool rightPressed = false;
        bool leftPressed = false;

        if (xRInputActionReferences != null)
        {
            if (xRInputActionReferences.rightTriggerPressed != null && rightControllerTransform != null && rightControllerTransform.gameObject.activeInHierarchy)
                rightPressed = xRInputActionReferences.rightTriggerPressed.action.IsPressed();

            if (xRInputActionReferences.leftTriggerPressed != null && leftControllerTransform != null && leftControllerTransform.gameObject.activeInHierarchy)
                leftPressed = xRInputActionReferences.leftTriggerPressed.action.IsPressed();
        }

        bool isVRPressed = rightPressed || leftPressed;
        
        bool isVR = isVRPressed || activeControllerTransform != null;

        bool isPressed = false;
        Ray ray = default;
        Vector2 currentPosition = Vector2.zero;

        if (isVR)
        {
            isPressed = isVRPressed;

            if (isPressed && !wasPressed) 
            {
                if (rightPressed) { activeControllerTransform = rightControllerTransform; activeXrRayInteractor = rightXrRayInteractor; }
                else if (leftPressed) { activeControllerTransform = leftControllerTransform; activeXrRayInteractor = leftXrRayInteractor; }
            }

            if (activeControllerTransform != null)
                ray = new Ray(activeControllerTransform.position, activeControllerTransform.forward);
        }
        else
        {
            int activeTouches = 0;
            if (Mouse.current != null)
            {
                isPressed = Mouse.current.leftButton.isPressed;
                currentPosition = Mouse.current.position.ReadValue();
            }

            if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
            {
                bool touchDetected = false;
                foreach (var touch in Touchscreen.current.touches)
                {
                    var phase = touch.phase.ReadValue();
                    if (phase == UnityEngine.InputSystem.TouchPhase.Began || phase == UnityEngine.InputSystem.TouchPhase.Moved || phase == UnityEngine.InputSystem.TouchPhase.Stationary)
                    {
                        if (!touchDetected) { isPressed = true; currentPosition = touch.position.ReadValue(); touchDetected = true; }
                    }
                }
            }

            if (isPressed)
                lastPointerPosition = currentPosition;
            
            EnsureMainCamera();
            
            if (mainCamera != null && !float.IsInfinity(lastPointerPosition.x) && !float.IsNaN(lastPointerPosition.x))
                ray = mainCamera.ScreenPointToRay(lastPointerPosition);
        }

        bool pressedThisFrame = isPressed && !wasPressed;
        bool releasedThisFrame = !isPressed && wasPressed;

        if (pressedThisFrame)
        {
            if (IsPointerOverUI(isVR)) startedOnUI = true;
            else
            {
                startedOnUI = false;
                isDragging = false;

                if (isVR && activeControllerTransform != null) startControllerPosition = activeControllerTransform.position;
                else startInputPosition = lastPointerPosition;
            }
        }
        else if (releasedThisFrame)
        {
            if (!startedOnUI)
            {
                if (isVR)
                {
                    if (activeControllerTransform != null)
                    {
                        float distance = Vector3.Distance(startControllerPosition, activeControllerTransform.position);
                        if (distance > dragThresholdVR) isDragging = true;
                    }
                }
                else
                {
                    float distance = Vector2.Distance(startInputPosition, lastPointerPosition);
                    if (distance > dragThreshold2D) isDragging = true;
                }

                if (!isDragging) CreatePoint(ray);
            }

            activeControllerTransform = null;
            activeXrRayInteractor = null;
        }

        wasPressed = isPressed;
    }

    private void CreatePoint(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interestLayer))
        {
            Vector3 point = hit.point;
            alterationPoint = point;
            
            AnnotationManager am = FindAnyObjectByType<AnnotationManager>();
            if (am != null)
            {
                am.CreatePointReference(alterationPoint);
            }
        }
    }
    
    public void EnableSelectingPoint(bool enable)
    {
        _isSelecting = enable;
    }
    
    private bool IsPointerOverUI(bool isVR)
    {
        if (isVR && activeXrRayInteractor != null)
        {
            if (activeXrRayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult uiResult))
                if (uiResult.gameObject != null) return true;
            return false;
        }

        if (EventSystem.current != null)
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return true;
            if (Pointer.current != null && EventSystem.current.IsPointerOverGameObject(Pointer.current.deviceId))
                return true;
        }
        return false;
    }
    
    private void EnsureMainCamera()
    {
        if (mainCamera != null && mainCamera.gameObject.activeInHierarchy)
            return;

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            Camera[] localCameras = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponentsInChildren<Camera>(true);
            foreach (var cam in localCameras)
            {
                if (cam.isActiveAndEnabled)
                {
                    mainCamera = cam;
                    return;
                }
            }
        }
        
        mainCamera = Camera.main;
    }
}