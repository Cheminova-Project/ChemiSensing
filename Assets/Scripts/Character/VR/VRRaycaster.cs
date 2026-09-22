using System;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using VInspector;

/// <summary>
/// Permite realizar raycasts desde la cámara en entornos de Realidad Virtual (VR).
/// Utilizado para interacción con objetos mediante controladores VR.
/// </summary>
public class VRRaycaster : MonoBehaviour
{
    [Header("Raycasting")]
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Transform raycastOrigin;
    [SerializeField] private NetworkedLineRenderer raycastLineRenderer;

    private Transform selectedObject;
    
    [SerializeField]private bool laserEnabled = true;
    
    private VisualizationModeController _visController;
    private TextureManager _texManager;
    
    protected virtual void OnEnable()
    {
        InitListeners();
        laserEnabled = false;
    }

    private void OnDisable()
    {
        DestroyListeners();
    }

    protected virtual void DestroyListeners()
    {
        
    }

    protected virtual void InitListeners()
    {
        
    }

    public void EnableLaser(bool enableL)
    {
        if(enableL)
            EnableLaser();
        else
            DisableLaser();
    }
    public void EnableLaser()
    {
        laserEnabled = true;
        raycastLineRenderer.SetPoints(new List<Vector3>());
    }
    
    public void DisableLaser()
    {
        laserEnabled = false;
        raycastLineRenderer.SetPoints(new List<Vector3>());
    }

    public virtual void EnableRaycaster(bool b)
    {
        laserEnabled = b;
    }

    private void FixedUpdate()
    {
        if (!laserEnabled)
        {
            raycastLineRenderer.SetPoints(new List<Vector3>());
            return;
        }
        
        Physics.Raycast(raycastOrigin.position, raycastOrigin.forward, out RaycastHit hit, Mathf.Infinity, layerMask);
        bool hitSomething = hit.collider != null;
        
        if (hitSomething)
        {
            Transform inspected = GetInspectedObject();
            
            if (inspected != null && (hit.transform == inspected || hit.transform.IsChildOf(inspected)))
            {
                if (!IsPointVisibleOnSectionPlane(hit.point, inspected))
                {
                    hitSomething = false;
                }
            }
        }
        
        if (!hitSomething)
        {
            raycastLineRenderer.SetPoints(new List<Vector3>());
            return;
        }
        List<Vector3> points = new List<Vector3>
        {
            raycastOrigin.position,
            hit.point
        };

        raycastLineRenderer.SetPoints(points);
        VisualPointer.Instance.SetMarkerPosition(hit.point);
    }

    /// <summary>
    /// Cámara desde la que se lanzan los raycasts en VR.
    /// </summary>
    public Camera raycastCamera;

    /// <summary>
    /// Realiza un raycast desde la posición del controlador y devuelve el objeto impactado.
    /// </summary>
    /// <param name="controllerPosition">Posición del controlador VR.</param>
    /// <returns>Objeto impactado o null.</returns>
    public GameObject RaycastFromController(Vector3 controllerPosition)
    {
        Ray ray = new Ray(controllerPosition, raycastCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Transform inspected = GetInspectedObject();
            if (inspected != null && (hit.transform == inspected || hit.transform.IsChildOf(inspected)))
            {
                if (!IsPointVisibleOnSectionPlane(hit.point, inspected))
                    return null;
            }
            return hit.collider.gameObject;
        }
        return null;
    }
    
    public void SetLaserColor(Color newColor)
    {
        if (raycastLineRenderer != null)
            raycastLineRenderer.SetColor(newColor);
    }
    
    private Transform GetInspectedObject()
    {
        if (InspectedObjectController.Instance != null)
        {
            var obj = InspectedObjectController.Instance.GetInspectedObject();
            if (obj != null) return obj.transform;
        }
        return null;
    }

    private bool IsPointVisibleOnSectionPlane(Vector3 hitPoint, Transform objTransform)
    {
        if (objTransform == null) return true;

        if (_visController == null) _visController = FindFirstObjectByType<VisualizationModeController>();
        if (_texManager == null) _texManager = FindFirstObjectByType<TextureManager>();

        if (_visController != null && _texManager != null && _texManager.GetModelRenderer() != null)
        {
            Material mat = _texManager.GetModelRenderer().sharedMaterial;

            if (mat != null && mat.HasProperty("_CutPosition") && mat.HasProperty("_CutNormal") && 
                mat.shader != null && mat.shader.name.Contains("sectionPlaneShader"))
            {
                Vector3 planePosition = mat.GetVector("_CutPosition");
                Vector3 planeNormal = mat.GetVector("_CutNormal");

                Vector3 localP = objTransform.InverseTransformPoint(hitPoint);
                Vector3 toPoint = localP - planePosition;

                float projection = Vector3.Dot(toPoint, planeNormal);

                bool isVisible = (projection <= 0f && !_visController.runtimeReverse) || 
                                 (projection >= 0f && _visController.runtimeReverse);

                return isVisible;
            }
        }
        return true;
    }
}
