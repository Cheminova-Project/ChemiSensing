using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.UIElements;

public class VRAngleRuler : AngleRuler
{
    [Header("VR Input")]
    public XRInputActionReferences xRInputActionReferences;

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
        {
            rightControllerTransform = GameObject.FindGameObjectWithTag("ControllerR")?.transform;
        }
        
        if (rightControllerTransform != null)
        {
            xrRayInteractor = rightControllerTransform.GetComponent<XRRayInteractor>();
            if (xrRayInteractor == null)
            {
                xrRayInteractor = rightControllerTransform.GetComponentInChildren<XRRayInteractor>();
            }
        }
    }

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
        
        if (rightControllerTransform == null) return;

        Vector3 controllerPosition = rightControllerTransform.position;
        Quaternion controllerRotation = rightControllerTransform.rotation;

        Ray ray = new Ray(controllerPosition, controllerRotation * Vector3.forward);
        hasHit = Physics.Raycast(ray, out lastHit, Mathf.Infinity, layerMask);

        UpdatePointerVisualization();

        if (triggerPressed && hasHit)
        {
            if (!IsPointerOverUI())
            {
                if (measuredObject == null) SetMeasuredObject(GetInspectedObject());
                RequestAddPoint(lastHit.point);
            }
        }
        triggerPressed = false;
    }
    
    private bool IsPointerOverUI()
    {
        if (xrRayInteractor != null && xrRayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult uiResult))
        {
            if (uiResult.gameObject == null || uIDocument == null || uIDocument.rootVisualElement == null) return false;

            Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(uIDocument.rootVisualElement.panel, uiResult.screenPosition);
            var picked = uIDocument.rootVisualElement.panel.Pick(panelPos);
        
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
        return false;
    }
    
    private void UpdatePointerVisualization()
    {
        if (VisualPointer.Instance == null) return;
        if (hasHit) VisualPointer.Instance.SetMarkerPosition(lastHit.point);
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