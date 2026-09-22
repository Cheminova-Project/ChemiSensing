using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class DrawLineAnnotation : MonoBehaviour
{
    [Header("Drawing Settings (General)")]
    public Color penColor = Color.blue;
    public Material lineMaterial;
    public LayerMask interestLayer;

    [Header("3D Mode Settings")]
    public float penWidth3D = 0.1f;
    public float zOffset3D = 0.1f;

    [Header("AR Mode Settings")]
    public float penWidthAR = 0.005f;
    public float zOffsetAR = 0.002f;

    private LineRenderer tempLineRenderer;
    private Camera mainCamera;
    private float activePenWidth;
    private float activeZOffset;
    
    private GameObject targetModel;
    private List<Vector3> localPoints = new List<Vector3>();
    private bool isDrawing;
    private bool drawingEnabled;
    
    private GameObject currentDrawingLineObj;
    private LineRenderer currentDrawingLineRenderer;

    public static EventHandler<bool> OnStartDrawing;

    private bool wasPressed;
    private VisualizationModeController controller;

    void Start()
    {
        mainCamera = Camera.main;
        
        controller = FindFirstObjectByType<VisualizationModeController>();
        
        tempLineRenderer = GetComponent<LineRenderer>();
        if (tempLineRenderer != null) 
            tempLineRenderer.enabled = false;
        
        ViewerMode currentMode = ViewerMode.basic;
        
        var baseDownloader = FindAnyObjectByType<InspectedObjectController>();
        
        if (baseDownloader != null)
            currentMode = baseDownloader.viewerMode;
    
        if (currentMode == ViewerMode.vr)
        {
            activePenWidth = penWidthAR;
            activeZOffset = zOffsetAR;
        }
        else
        {
            activePenWidth = penWidth3D;
            activeZOffset = zOffset3D;
        }
    }

    void Update()
    {
        if (!drawingEnabled || mainCamera == null)
            return;
        
        if (FindAnyObjectByType<AnnotationManager>() == null)
        {
            EnableDrawing(false);
            return;
        }

        bool isPressed = false;
        Vector2 currentPosition = Vector2.zero;
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

        bool pressedThisFrame = isPressed && !wasPressed;
        bool releasedThisFrame = !isPressed && wasPressed;

        if (pressedThisFrame)
        {
            if (IsPointerOverUI())
            {
                wasPressed = isPressed;
                return;
            }

            if (targetModel == null)
            {
                targetModel = GetTargetModelSafe();
                
                if (targetModel == null)
                {
                    wasPressed = isPressed;
                    return;
                }
            }

            Ray startRay = mainCamera.ScreenPointToRay(currentPosition);
            
            if (!Physics.Raycast(startRay, Mathf.Infinity, interestLayer))
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
                    currentDrawingLineObj = null;
                    currentDrawingLineRenderer = null;
                }
                else
                {
                    if (currentDrawingLineObj != null)
                        Destroy(currentDrawingLineObj);
                }
            }
        }

        if (isDrawing && currentDrawingLineRenderer != null && isPressed)
        {
            if (IsPointerOverUI())
            {
                wasPressed = isPressed;
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(currentPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interestLayer))
            {
                Vector3 worldPoint = hit.point;
                
                AnnotationManager am = FindAnyObjectByType<AnnotationManager>();
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
                            else
                            {
                                if (currentDrawingLineObj != null)
                                    Destroy(currentDrawingLineObj);
                            }

                            return;
                        }
                    }
                }

                Vector3 directionToCamera = (mainCamera.transform.position - worldPoint).normalized;
                worldPoint += directionToCamera * activeZOffset;
                Vector3 localPoint = targetModel.transform.InverseTransformPoint(worldPoint);

                if (localPoints.Count == 0 || Vector3.Distance(localPoints[localPoints.Count - 1], localPoint) > 0.001f)
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
                    currentDrawingLineObj = null;
                    currentDrawingLineRenderer = null;
                }
                else
                {
                    if (currentDrawingLineObj != null)
                        Destroy(currentDrawingLineObj);
                }
            }
        }

        wasPressed = isPressed;
    }
    
    private GameObject GetTargetModelSafe()
    {
        var baseDownloader = FindAnyObjectByType<InspectedObjectController>();
        if (baseDownloader != null)
        {
            return baseDownloader.activeModelInstance;
        }

        return null;
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
}