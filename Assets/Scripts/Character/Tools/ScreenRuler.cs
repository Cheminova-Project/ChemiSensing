using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// Herramienta de regla que permite medir distancias en la pantalla
/// mediante clics del usuario. Utiliza raycasting para determinar los puntos
/// en el espacio 3D basándose en la posición del puntero.
/// </summary>
public class ScreenRuler : Ruler
{
    private Transform originalObject;
    
    private Vector2 inputPointerPosition;
    private bool alreadyPressed;
    private Camera raycastCamera;

    /// <summary>
    /// Capa utilizada para el raycasting al medir distancias.
    /// </summary>
    [SerializeField] private LayerMask layerMask;
    /// <summary>
    /// Documento UI que contiene la ventana de la herramienta.
    /// </summary>
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
        if (originalObject == null)
            return;

        if (IsPointerOverUI())
        {
            Debug.LogWarning("BLOQUEADO POR UI: El toque se ha detectado, pero Unity cree que estás tocando un botón/panel invisible.");
            return;
        }

        Ray ray = raycastCamera.ScreenPointToRay(inputPointerPosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            VisualizationModeController visController = FindFirstObjectByType<VisualizationModeController>();
            TextureManager texManager = FindFirstObjectByType<TextureManager>();

            if (visController != null && texManager != null && texManager.GetModelRenderer() != null)
            {
                Material mat = texManager.GetModelRenderer().sharedMaterial;

                if (mat != null && mat.HasProperty("_CutPosition") && mat.HasProperty("_CutNormal") && 
                    mat.shader != null && mat.shader.name.Contains("sectionPlaneShader"))
                {
                    Vector3 planePosition = mat.GetVector("_CutPosition");
                    Vector3 planeNormal = mat.GetVector("_CutNormal");

                    Vector3 localP = originalObject.InverseTransformPoint(hit.point);
                    Vector3 toPoint = localP - planePosition;

                    float projection = Vector3.Dot(toPoint, planeNormal);

                    bool isVisible = (projection <= 0f && !visController.runtimeReverse) || 
                                     (projection >= 0f && visController.runtimeReverse);

                    if (!isVisible)
                    {
                        Debug.LogWarning("[ScreenRuler] El punto cayó en la mitad invisible del Section Plane. Se ignora.");
                        return;
                    }
                }
            }

            RequestAddPoint(hit.point);
        }
        else
            Debug.LogWarning("[Físicas] El rayo 3D ha atravesado la escena y no ha tocado nada.");
    }
    
    /// <summary>
    /// Comprueba de forma robusta si el puntero está sobre cualquier elemento de la interfaz interactiva.
    /// </summary>
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

                    if (alpha > 0.01f || hasImage)
                    {
                        return true;
                    }

                    if (current == picked && current is TextElement)
                    {
                        return true;
                    }

                    current = current.parent;
                }
                return false;
            }
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

    /// <summary>
    /// Obtiene el objeto actualmente inspeccionado en la escena.
    /// </summary>
    private Transform GetInspectedObject()
    {
        if (InspectedObjectController.Instance != null)
        {
            var inspectedObj = InspectedObjectController.Instance.GetInspectedObject();
            if (inspectedObj != null) 
                return inspectedObj.transform;
        }
        
        // 2. Fallback (solo por si haces pruebas en escenas aisladas sin el Controller)
        return GameObject.FindGameObjectWithTag("InspectedObject")?.transform;
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
        originalObject = null;
        alreadyPressed = false;
    }
}
