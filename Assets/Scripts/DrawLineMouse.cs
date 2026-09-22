using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(LineRenderer))]
public class DrawLineMouse : MonoBehaviour
{
    [Header("Drawing Settings (General)")]
    public Color penColor = Color.blue;
    public Material lineMaterial;
    public LayerMask interestLayer;

    [Header("3D Mode Settings")]
    public float penWidth3D = 0.1f;
    public float zOffset3D = 0.1f;

    [Header("VR Mode Settings")]
    public float penWidthVR = 0.005f;
    public float zOffsetVR = 0.002f;
    
    [Header("VR Input")]
    public XRInputActionReferences xRInputActionReferences;
    private Transform rightControllerTransform;
    private XRRayInteractor rightXrRayInteractor;
    private Transform leftControllerTransform;
    private XRRayInteractor leftXrRayInteractor;
    private Transform activeControllerTransform;
    private XRRayInteractor activeXrRayInteractor;

    private LineRenderer tempLineRenderer;
    private Camera mainCamera;
    private float activePenWidth;
    private float activeZOffset;
    
    private GameObject targetModel;
    private List<Vector3> localPoints = new List<Vector3>();
    private bool isDrawing = false;
    private bool drawingEnabled = false;
    
    private GameObject currentDrawingLineObj;
    private LineRenderer currentDrawingLineRenderer;

    public static EventHandler<bool> OnStartDrawing;

    private bool wasPressed = false;

    private VisualizationModeController controller;

    void Start()
    {
        EnsureMainCamera();
        
        tempLineRenderer = GetComponent<LineRenderer>();
        if (tempLineRenderer != null)
            tempLineRenderer.enabled = false;
        
        controller = FindFirstObjectByType<VisualizationModeController>();

        ViewerMode currentMode = FindAnyObjectByType<InspectedObjectController>().viewerMode;
    
        if (currentMode == ViewerMode.vr)
        {
            activePenWidth = penWidthVR;
            activeZOffset = zOffsetVR;

            StartCoroutine(EnsureVRInputEnabled());
        }
        else
        {
            activePenWidth = penWidth3D;
            activeZOffset = zOffset3D;
        }
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
                    if (child.CompareTag("ControllerR"))
                        rightControllerTransform = child;
                    else if (child.CompareTag("ControllerL"))
                        leftControllerTransform = child;
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
                if (rightXrRayInteractor == null)
                    rightXrRayInteractor = rightControllerTransform.GetComponentInChildren<XRRayInteractor>();
            }

            if (leftControllerTransform != null)
            {
                leftXrRayInteractor = leftControllerTransform.GetComponent<XRRayInteractor>();
                if (leftXrRayInteractor == null)
                    leftXrRayInteractor = leftControllerTransform.GetComponentInChildren<XRRayInteractor>();
            }
        }
    }

    void Update()
    {
        EnsureMainCamera();

        if (!drawingEnabled || mainCamera == null)
            return;
        
        if (FindAnyObjectByType<AnnotationManager>() == null)
        {
            EnableDrawing(false);
            return;
        }

        EnsureVRReferences();

        bool isPressed = false;
        bool isVR = false;
        Ray ray = default;
        Vector2 currentPosition = Vector2.zero;

        bool rightPressed = false;
        bool leftPressed = false;

        if (xRInputActionReferences != null)
        {
            if (xRInputActionReferences.rightTriggerPressed != null && rightControllerTransform != null && rightControllerTransform.gameObject.activeInHierarchy)
                rightPressed = xRInputActionReferences.rightTriggerPressed.action.IsPressed();

            if (xRInputActionReferences.leftTriggerPressed != null && leftControllerTransform != null && leftControllerTransform.gameObject.activeInHierarchy)
                leftPressed = xRInputActionReferences.leftTriggerPressed.action.IsPressed();
        }

        if (rightPressed || leftPressed)
        {
            isVR = true;
            isPressed = true;

            if (!isDrawing) 
            {
                if (rightPressed)
                {
                    activeControllerTransform = rightControllerTransform;
                    activeXrRayInteractor = rightXrRayInteractor;
                }
                else if (leftPressed)
                {
                    activeControllerTransform = leftControllerTransform;
                    activeXrRayInteractor = leftXrRayInteractor;
                }
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
                    if (phase == UnityEngine.InputSystem.TouchPhase.Began ||
                        phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                        phase == UnityEngine.InputSystem.TouchPhase.Stationary)
                    {
                        activeTouches++;
                        if (!touchDetected)
                        {
                            isPressed = true;
                            currentPosition = touch.position.ReadValue();
                            touchDetected = true;
                        }
                    }
                }
            }

            if (activeTouches > 1)
            {
                if (isDrawing)
                {
                    isDrawing = false;
                    if (currentDrawingLineObj != null) Destroy(currentDrawingLineObj);
                    OnStartDrawing?.Invoke(this, false);
                }
                wasPressed = isPressed;
                return;
            }

            if (isPressed)
            {
                if (float.IsInfinity(currentPosition.x) || float.IsInfinity(currentPosition.y) || float.IsNaN(currentPosition.x) || float.IsNaN(currentPosition.y)) return;
                ray = mainCamera.ScreenPointToRay(currentPosition);
            }
        }

        bool pressedThisFrame = isPressed && !wasPressed;
        bool releasedThisFrame = !isPressed && wasPressed;

        if (pressedThisFrame)
        {
            if (IsPointerOverUI(isVR))
            {
                wasPressed = isPressed;
                return;
            }

            if (targetModel == null)
            {
                var inspectedController = FindAnyObjectByType<InspectedObjectController>();
                targetModel = inspectedController != null ? inspectedController.activeModelInstance : null;
                
                if (targetModel == null)
                {
                    Debug.LogWarning("[DrawLineMouse] No se encontró el targetModel activo. ¿Está el modelo cargado?");
                    wasPressed = isPressed;
                    return;
                }
            }

            if (!Physics.Raycast(ray, Mathf.Infinity, interestLayer))
            {
                wasPressed = isPressed;
                return;
            }

            isDrawing = true;
            localPoints.Clear();

            currentDrawingLineObj = new GameObject("DrawnLine");
            currentDrawingLineObj.transform.SetParent(targetModel.transform, false);
            currentDrawingLineObj.transform.localPosition = Vector3.zero;
            currentDrawingLineObj.transform.localRotation = Quaternion.identity;
            currentDrawingLineObj.transform.localScale = Vector3.one;

            currentDrawingLineRenderer = currentDrawingLineObj.AddComponent<LineRenderer>();
            currentDrawingLineRenderer.material = lineMaterial;
            currentDrawingLineRenderer.startColor = penColor;
            currentDrawingLineRenderer.endColor = penColor;
            currentDrawingLineRenderer.useWorldSpace = false;
            currentDrawingLineRenderer.positionCount = 0;
            
            OnStartDrawing?.Invoke(this, true);
        }
        else if (releasedThisFrame)
        {
            if (isDrawing)
            {
                isDrawing = false;
                OnStartDrawing?.Invoke(this, false);
                
                if (localPoints.Count > 1)
                {
                    FindAnyObjectByType<AnnotationManager>()?.CreateLineReference(currentDrawingLineObj, localPoints, false);
                    currentDrawingLineObj = null;
                    currentDrawingLineRenderer = null;
                }
                else
                {
                    if (currentDrawingLineObj != null)
                        Destroy(currentDrawingLineObj);
                }
            }

            activeControllerTransform = null;
            activeXrRayInteractor = null;
        }

        if (isDrawing && currentDrawingLineRenderer != null && isPressed)
        {
            if (IsPointerOverUI(isVR))
            {
                wasPressed = isPressed;
                return;
            }

            AnnotationManager am = FindAnyObjectByType<AnnotationManager>();
            if (am != null)
                am.KeepBalancedLineScale(currentDrawingLineRenderer);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interestLayer))
            {
                Vector3 worldPoint = hit.point;

                if (am != null && am.rendererModel != null && controller != null && am.rendererModel.sharedMaterial.shader == am.sectionShader)
                {
                    Material material = am.rendererModel.sharedMaterial;

                    if (material.HasProperty("_CutPosition") && material.HasProperty("_CutNormal"))
                    {
                        Vector3 planePosition = material.GetVector("_CutPosition");
                        Vector3 planeNormal = material.GetVector("_CutNormal");

                        Vector3 localP = targetModel.transform.InverseTransformPoint(worldPoint);
                        Vector3 toPoint = localP - planePosition;

                        float projection = Vector3.Dot(toPoint, planeNormal);

                        if ((projection > 0f && controller.runtimeReverse == false) || (projection < 0f && controller.runtimeReverse == true))
                        {
                            isDrawing = false;
                            OnStartDrawing?.Invoke(this, false);

                            if (localPoints.Count > 1)
                            {
                                am.CreateLineReference(currentDrawingLineObj, localPoints, false);
                                currentDrawingLineObj = null;
                                currentDrawingLineRenderer = null;
                            }
                            else if (currentDrawingLineObj != null)
                                Destroy(currentDrawingLineObj);

                            activeControllerTransform = null;
                            activeXrRayInteractor = null;
                            return;
                        }
                    }
                }

                Vector3 directionToCamera;
                if (isVR && activeControllerTransform != null)
                    directionToCamera = (activeControllerTransform.position - worldPoint).normalized;
                else
                    directionToCamera = (mainCamera.transform.position - worldPoint).normalized;

                worldPoint += directionToCamera * activeZOffset;
                Vector3 localPoint = targetModel.transform.InverseTransformPoint(worldPoint);

                if (localPoints.Count == 0 || Vector3.Distance(localPoints[^1], localPoint) > 0.001f)
                {
                    localPoints.Add(localPoint);
                    currentDrawingLineRenderer.positionCount = localPoints.Count;
                    currentDrawingLineRenderer.SetPosition(localPoints.Count - 1, localPoint);
                }
            }
            else
            {
                isDrawing = false;
                OnStartDrawing?.Invoke(this, false);
                
                if (localPoints.Count > 1)
                {
                    FindAnyObjectByType<AnnotationManager>()?.CreateLineReference(currentDrawingLineObj, localPoints, false);
                    currentDrawingLineObj = null;
                    currentDrawingLineRenderer = null;
                }
                else
                {
                    if (currentDrawingLineObj != null)
                        Destroy(currentDrawingLineObj);
                }
                
                activeControllerTransform = null;
                activeXrRayInteractor = null;
            }
        }

        wasPressed = isPressed;
    }

    public void EnableDrawing(bool enable)
    {
        drawingEnabled = enable;
        
        if (!drawingEnabled)
        {
            if (currentDrawingLineObj != null && isDrawing)
            {
                Destroy(currentDrawingLineObj);
            }
            isDrawing = false;
            OnStartDrawing?.Invoke(this, false);
        }
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
    
    private void OnDisable()
    {
        CleanUpUnfinishedLine();
    }

    private void OnDestroy()
    {
        CleanUpUnfinishedLine();
    }

    private void CleanUpUnfinishedLine()
    {
        if (isDrawing)
        {
            if (currentDrawingLineObj != null) 
                Destroy(currentDrawingLineObj);
            
            isDrawing = false;
            OnStartDrawing?.Invoke(this, false);
        }
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