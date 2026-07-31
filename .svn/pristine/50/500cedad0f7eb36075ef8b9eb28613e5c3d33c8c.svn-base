using System;
using System.Collections;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

/// <summary>
/// Clase que implementa una regla (ruler) en VR utilizando raycasting desde el controlador derecho.
/// Permite medir distancias en el entorno VR al apuntar y seleccionar puntos con el trigger
public class VRRuler : Ruler
{
    /// <summary>
    /// Referencias a las acciones de entrada XR.
    /// </summary>
    [Header("VR Input")]
    public XRInputActionReferences xRInputActionReferences;

    /// <summary>
    /// Objeto a medir.
    /// </summary>
    [Header("Raycast Settings")]
    [SerializeField] private LayerMask layerMask;

    private bool triggerPressed;
    private RaycastHit lastHit;
    private bool hasHit;
    private Transform rightControllerTransform;
    private XRRayInteractor xrRayInteractor;

    void Start()
    {
        xRInputActionReferences.rightTriggerPressed.action.performed += OnTriggerPressed;
        StartCoroutine(EnsureInputEnabled());

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            GameObject localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
            
            Transform[] allChildren = localPlayer.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allChildren)
            {
                if (child.CompareTag("ControllerR"))
                {
                    rightControllerTransform = child;
                    break;
                }
            }
        }
        else
            rightControllerTransform = GameObject.FindGameObjectWithTag("ControllerR")?.transform;
        
        if (rightControllerTransform != null)
        {
            xrRayInteractor = rightControllerTransform.GetComponent<XRRayInteractor>();
            if (xrRayInteractor == null)
            {
                xrRayInteractor = rightControllerTransform.GetComponentInChildren<XRRayInteractor>();
            }
        }
    }

    /// <summary>
    /// Maneja el evento cuando se presiona el trigger derecho.
    /// </summary>
    private void OnTriggerPressed(InputAction.CallbackContext ctx)
    {
        triggerPressed = true;
    }


    protected override void Update()
    {
        base.Update();
        
        if (xrRayInteractor == null && rightControllerTransform != null)
        {
            xrRayInteractor = rightControllerTransform.GetComponentInChildren<XRRayInteractor>();
        }
        
        if (rightControllerTransform == null)
            return;

        // Obtener posición y rotación del controlador derecho
        Vector3 controllerPosition = rightControllerTransform.position;
        Quaternion controllerRotation = rightControllerTransform.rotation;

        // Lanzar rayo desde la posición del controlador en dirección forward
        Ray ray = new Ray(controllerPosition, controllerRotation * Vector3.forward);

        // Realizar raycast
        hasHit = Physics.Raycast(ray, out lastHit, Mathf.Infinity, layerMask);

        // Actualizar visualización del marcador de hit mediante VRPointer
        UpdatePointerVisualization();

        // Si el trigger está presionado y golpea el objeto objetivo
        if (triggerPressed && hasHit)
        {
            if (!IsPointerOverUI())
            {
                if (measuredObject == null)
                    SetMeasuredObject(GetInspectedObject());

                RequestAddPoint(lastHit.point);
            }
        }
        triggerPressed = false; // Resetear el estado del trigger
    }
    
    /// <summary>
    /// Comprueba si el rayo del controlador VR está apuntando a un elemento de UI.
    /// </summary>
    private bool IsPointerOverUI()
    {
        if (xrRayInteractor != null && xrRayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult uiResult))
        {
            // Si el resultado no tiene gameObject, no estamos apuntando a la UI
            if (uiResult.gameObject == null || uIDocument == null || uIDocument.rootVisualElement == null)
                return false;

            // Convertimos la posición simulada de VR a coordenadas del panel
            Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(
                uIDocument.rootVisualElement.panel, 
                uiResult.screenPosition
            );
        
            var picked = uIDocument.rootVisualElement.panel.Pick(panelPos);
        
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
                        return true; // Ha tocado un elemento visual real
                    }

                    if (current == picked && current is UnityEngine.UIElements.TextElement)
                    {
                        return true; // Ha tocado texto
                    }

                    current = current.parent;
                }
                // Si llega aquí, es que todo lo que hay debajo del puntero es transparente
                return false;
            }
        }
        return false;
    }
    
    /// <summary>
    /// Actualiza la visualización del puntero VR con la posición del último hit.
    /// </summary>
    private void UpdatePointerVisualization()
    {
        if (VisualPointer.Instance == null)
            return;

        if (hasHit)
        {
            VisualPointer.Instance.SetMarkerPosition(lastHit.point);
        }
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
        
        return GameObject.FindGameObjectWithTag("InspectedObject")?.transform;
    }

    private void OnDestroy()
    {
        xRInputActionReferences.rightTriggerPressed.action.performed -= OnTriggerPressed;
    }
    
    private IEnumerator EnsureInputEnabled()
    {
        yield return new WaitForEndOfFrame();
        if (!xRInputActionReferences.rightTriggerPressed.action.enabled)
        {
            xRInputActionReferences.rightTriggerPressed.action.Enable();
        }
    }
}