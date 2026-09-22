using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

public class VRAngleRuler : AngleRuler
{
    [Header("VR Input")]
    public XRInputActionReferences xRInputActionReferences;

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask layerMask;

    private RaycastHit lastHit;
    private bool hasHit;
    
    // VR References
    private Transform rightControllerTransform;
    private XRRayInteractor rightXrRayInteractor;
    private Transform leftControllerTransform;
    private XRRayInteractor leftXrRayInteractor;
    private Transform activeControllerTransform;
    private XRRayInteractor activeXrRayInteractor;

    private bool wasRightPressed;
    private bool wasLeftPressed;

    void Start()
    {
        StartCoroutine(EnsureInputEnabled());
        EnsureVRReferences();
    }

    private IEnumerator EnsureInputEnabled()
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

    protected override void Update()
    {
        base.Update();
        
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

        if (rightPressed && !wasRightPressed) 
        {
            activeControllerTransform = rightControllerTransform;
            activeXrRayInteractor = rightXrRayInteractor;
        }
        else if (leftPressed && !wasLeftPressed) 
        {
            activeControllerTransform = leftControllerTransform;
            activeXrRayInteractor = leftXrRayInteractor;
        }

        if (activeControllerTransform == null)
        {
            if (rightControllerTransform != null)
            {
                activeControllerTransform = rightControllerTransform;
                activeXrRayInteractor = rightXrRayInteractor;
            }
            else if (leftControllerTransform != null)
            {
                activeControllerTransform = leftControllerTransform;
                activeXrRayInteractor = leftXrRayInteractor;
            }
            else return; 
        }

        Vector3 controllerPosition = activeControllerTransform.position;
        Quaternion controllerRotation = activeControllerTransform.rotation;
        Ray ray = new Ray(controllerPosition, controllerRotation * Vector3.forward);

        hasHit = Physics.Raycast(ray, out lastHit, Mathf.Infinity, layerMask);
        UpdatePointerVisualization();

        bool clickedRight = rightPressed && !wasRightPressed;
        bool clickedLeft = leftPressed && !wasLeftPressed;

        if ((clickedRight || clickedLeft) && hasHit)
        {
            if (!IsPointerOverUI())
            {
                if (measuredObject == null) 
                    SetMeasuredObject(GetInspectedObject());

                VisualizationModeController visController = FindFirstObjectByType<VisualizationModeController>();
                TextureManager texManager = FindFirstObjectByType<TextureManager>();

                bool isValidPoint = true;

                if (visController != null && texManager != null && texManager.GetModelRenderer() != null)
                {
                    Material mat = texManager.GetModelRenderer().sharedMaterial;

                    if (mat != null && mat.HasProperty("_CutPosition") && mat.HasProperty("_CutNormal") && 
                        mat.shader != null && mat.shader.name.Contains("sectionPlaneShader"))
                    {
                        Vector3 planePosition = mat.GetVector("_CutPosition");
                        Vector3 planeNormal = mat.GetVector("_CutNormal");

                        Vector3 localP = measuredObject.InverseTransformPoint(lastHit.point);
                        Vector3 toPoint = localP - planePosition;

                        float projection = Vector3.Dot(toPoint, planeNormal);

                        bool isVisible = (projection <= 0f && !visController.runtimeReverse) || 
                                         (projection >= 0f && visController.runtimeReverse);

                        if (!isVisible)
                        {
                            Debug.LogWarning("[VRAngleRuler] El punto cayó en la mitad invisible del Section Plane. Se ignora.");
                            isValidPoint = false;
                        }
                    }
                }

                if (isValidPoint)
                {
                    RequestAddPoint(lastHit.point);
                }
            }
        }

        wasRightPressed = rightPressed;
        wasLeftPressed = leftPressed;
    }
    
    private bool IsPointerOverUI()
    {
        if (activeXrRayInteractor != null && activeXrRayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult uiResult))
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
}