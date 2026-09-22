using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

// Model visualization types
public enum VisualizationMode
{
    Standard,
    Split,
    Spot,
    Ring,
    SectionPlane
}

// Possible cut modes
public enum CutMode
{
    EjeX,
    EjeY,
    EjeZ,
    Free
}

[System.Serializable]
// Main camera
public class MainCameraSlot
{
    [HideInInspector] public Camera camera;
}

[System.Serializable]
// Preview cameras with assigned texture
public class PreviewCameraSlot
{
    [HideInInspector] public int id;
    [HideInInspector] public GameObject obj;
    [HideInInspector] public Camera camera;
}

public class VisualizationModeController : NetworkBehaviour
{
    [Header("Development")]
    public InspectedObjectController inspectedObjectController;
    
    [Header("VR Input")]
    public XRInputActionReferences xRInputActionReferences;
    private Transform rightControllerTransform;
    private XRRayInteractor rightXrRayInteractor;
    private Transform leftControllerTransform;
    private XRRayInteractor leftXrRayInteractor;
    private Transform activeControllerTransform;
    private XRRayInteractor activeXrRayInteractor;
    private Vector3 dragStartControllerPos;

    [Header("Model")]
    public VisualizationMode visualizationMode = VisualizationMode.Standard;
    
    [Header("Shaders")]
    public Shader standardShader;
    public Shader splitShader;
    public Shader spotShader;
    public Shader ringShader;
    public Shader sectionPlaneShader;

    [Header("Cameras")]
    public MainCameraSlot mainCamera;
    public PreviewCameraSlot[] previewCamera;

    [Header("Spot and Ring Control")]
    public float runtimeRadius = 3f;
    public float radiusMultiplier = 0.1f;
    public float radiusStrength = 1f;
    public Color radiusColor = Color.red;
    public float softness = 1f;

    [Header("Cameras Control")]
    public int maxCameras;
    public float minFov = 0.1f;
    public float maxFov = 20f;
    
    private bool isIsolated = false;

    private bool isVisualizationModeManager;
    private TextureManager textureManager;
    private Transform targetObject;
    public Vector3 localPoint;
    public Vector3 localNormal;
    private Vector3 localUp;
    private bool hasTarget;
    private bool initialCutSet;
    private bool isDragging;
    private bool isCutting;
    private Vector3 lastMousePosition;
    private Vector3 dragStartMouse;
    private RaycastHit dragStartHit;
    public Vector3 dragStartLocalPos;
    public Vector3 dragVector = Vector3.zero;

    public Material currentMat;
    private Dictionary<string, Texture2D> runtimeTextures = new();
    private Shader runtimeShader;
    public bool runtimeReverse;
    public CutMode runtimeCutMode = CutMode.EjeX;
    public Renderer modelRenderer;
    private bool clicked;

    private bool wasPointerPressed;
    private Vector2 lastPointerPos = Vector2.zero;
    
    public List<int> finalTextures = new();
    
    public NetworkVariable<ulong> visToolLockOwner = new NetworkVariable<ulong>(ulong.MaxValue);
    public NetworkVariable<VisualizationMode> netVisualizationMode = new NetworkVariable<VisualizationMode>(VisualizationMode.Standard);
    public NetworkVariable<Vector3> netCutPosition = new NetworkVariable<Vector3>(Vector3.zero);
    public NetworkVariable<Vector3> netCutNormal = new NetworkVariable<Vector3>(Vector3.up);
    public NetworkVariable<Vector3> netCenter = new NetworkVariable<Vector3>(Vector3.zero);
    public NetworkVariable<bool> netUseTexIni = new NetworkVariable<bool>(false);
    public NetworkVariable<float> netRadius = new NetworkVariable<float>(3f);
    public NetworkVariable<bool> netReverse = new NetworkVariable<bool>(false);
    public NetworkVariable<int> netMainTextureId = new NetworkVariable<int>(-1);
    public NetworkVariable<int> netSecondaryTextureId = new NetworkVariable<int>(-1);
    public NetworkVariable<CutMode> netCutMode = new NetworkVariable<CutMode>(CutMode.EjeX);

    private float lastRpcTime = 0f;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        }

        netMainTextureId.OnValueChanged += (oldVal, newVal) => ApplyNetworkTexture(0, newVal);
        netSecondaryTextureId.OnValueChanged += (oldVal, newVal) => ApplyNetworkTexture(1, newVal);
        netCutMode.OnValueChanged += (oldVal, newVal) => 
        {
            if (!isIsolated && !HasToolLock()) 
                runtimeCutMode = newVal;
        };
        netVisualizationMode.OnValueChanged += (oldVal, newVal) => 
        {
            if (isIsolated)
                return; 
            
            if (newVal == VisualizationMode.Standard || !HasToolLock()) 
                ApplyModeLocal(newVal);
        };
        netReverse.OnValueChanged += (oldVal, newVal) => 
        {
            if (!isIsolated && !HasToolLock()) 
            {
                runtimeReverse = newVal;
                if (currentMat != null && currentMat.HasProperty("_Reverse"))
                    currentMat.SetFloat("_Reverse", newVal ? 1 : 0);
            }
        };
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (visToolLockOwner.Value == clientId)
        {
            visToolLockOwner.Value = ulong.MaxValue;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestVisToolLockServerRpc(ulong clientId)
    {
        if (visToolLockOwner.Value == ulong.MaxValue)
            visToolLockOwner.Value = clientId;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReleaseVisToolLockServerRpc(ulong clientId)
    {
        if (visToolLockOwner.Value == clientId)
        {
            visToolLockOwner.Value = ulong.MaxValue;
        }
    }

    public bool HasToolLock()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient)
            return true;
        
        return visToolLockOwner.Value == NetworkManager.Singleton.LocalClientId;
    }
    
    public bool IsSomeoneElseControlling()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient)
            return false;

        return visToolLockOwner.Value != ulong.MaxValue && visToolLockOwner.Value != NetworkManager.Singleton.LocalClientId;
    }

    void Start()
    {
        InitializeMainCamera();
        StartCoroutine(InitializeInit());
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
                    else if (child.CompareTag("ControllerL")) leftControllerTransform = child; // Asegúrate de tener este Tag en tu mando izquierdo
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

    IEnumerator InitializeInit()
    {
        do
        {
            Init();
            yield return new WaitForSeconds(0.1f);
        } while (textureManager == null || modelRenderer == null);
        
        if (!isIsolated && IsClient && netVisualizationMode.Value != VisualizationMode.Standard)
        {
            ApplyModeLocal(netVisualizationMode.Value);
            RestoreCutStateFromNetwork();
            ApplyNetworkTexture(0, netMainTextureId.Value, true);
            ApplyNetworkTexture(1, netSecondaryTextureId.Value, true);
        }
    }

    public void Init()
    {
        textureManager = FindFirstObjectByType<TextureManager>();

        if (textureManager == null)
            return;

        modelRenderer = textureManager.GetModelRenderer();

        if (modelRenderer != null && modelRenderer.sharedMaterial != null)
        {
            var albedo = modelRenderer.sharedMaterial.GetTexture("_BaseMap") as Texture2D;
            var normal = modelRenderer.sharedMaterial.GetTexture("_BumpMap") as Texture2D;
            var occlusion = modelRenderer.sharedMaterial.GetTexture("_OcclusionMap") as Texture2D;

            if (albedo != null) 
            {
                SetTexture("_BaseMap", albedo);
                SetTexture("_FirstTex", albedo);
            }
            if (normal != null) SetTexture("_BumpMap", normal);
            if (occlusion != null) SetTexture("_OcclusionMap", occlusion);
        }
    }

    void Update()
    {
        if (HasToolLock() || netVisualizationMode.Value != VisualizationMode.Standard)
        {
            if (HasToolLock())
                HandleInput();
            
            UpdateShaderProperties();
        }
    }
    
    /// <summary>
    /// Lanza un rayo y devuelve verdadero SOLO si golpea nuestro modelo 3D.
    /// Ignora avatares, jugadores y el entorno.
    /// </summary>
    private bool RaycastToModel(Ray ray, out RaycastHit validHit)
    {
        validHit = new RaycastHit();
        
        if (inspectedObjectController == null)
        {
            inspectedObjectController = InspectedObjectController.Instance;
            if (inspectedObjectController == null)
                return false;
        }

        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);

        foreach (var h in hits)
        {
            if (h.transform == inspectedObjectController.transform || h.transform.IsChildOf(inspectedObjectController.transform))
            {
                validHit = h;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Maneja la interacción con el modelo
    /// </summary>
    void HandleInput()
    {
        mainCamera.camera = Camera.main;
        
        if (mainCamera.camera == null || visualizationMode == VisualizationMode.Standard)
            return;

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
        Vector2 pointerPos = lastPointerPos;

        if (isVR)
        {
            isPressed = isVRPressed;

            if (isPressed && !wasPointerPressed) 
            {
                if (rightPressed)
                {
                    activeControllerTransform = rightControllerTransform;
                    activeXrRayInteractor = rightXrRayInteractor;
                }
                else
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
                pointerPos = Mouse.current.position.ReadValue();
                isPressed = Mouse.current.leftButton.isPressed;
            }
            if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
            {
                foreach (var touch in Touchscreen.current.touches)
                {
                    var phase = touch.phase.ReadValue();
                    if (phase == UnityEngine.InputSystem.TouchPhase.Began || phase == UnityEngine.InputSystem.TouchPhase.Moved || phase == UnityEngine.InputSystem.TouchPhase.Stationary)
                        activeTouches++;
                }
                if (activeTouches > 0)
                {
                    isPressed = Touchscreen.current.primaryTouch.press.isPressed;
                    pointerPos = Touchscreen.current.primaryTouch.position.ReadValue();
                }
            }
            if (float.IsInfinity(pointerPos.x) || float.IsInfinity(pointerPos.y) || float.IsNaN(pointerPos.x) || float.IsNaN(pointerPos.y)) return;
            ray = mainCamera.camera.ScreenPointToRay(pointerPos);
        }

        bool pointerDown = isPressed && !wasPointerPressed;
        bool pointerHeld = isPressed;
        bool pointerUp = !isPressed && wasPointerPressed;

        wasPointerPressed = isPressed;
        if (!isVR) lastPointerPos = pointerPos;
        
        if (pointerDown)
        {
            if (IsPointerOverUI(isVR)) return;
            
            if (RaycastToModel(ray, out RaycastHit hit))
            {
                clicked = true;
                initialCutSet = false;
                ApplyMode(visualizationMode);
                isCutting = true;
                isDragging = true;

                if (isVR) dragStartControllerPos = activeControllerTransform.position;
                else dragStartMouse = pointerPos;

                dragStartHit = hit;
                dragStartLocalPos = hit.transform.InverseTransformPoint(hit.point);
            }
        }

        if (pointerHeld && isDragging)
        {
            if (isVR) dragVector = activeControllerTransform.position - dragStartControllerPos;
            else
            {
                lastMousePosition = pointerPos;
                dragVector = lastMousePosition - dragStartMouse;
            }

            bool isHit = RaycastToModel(ray, out RaycastHit hit);

            if (visualizationMode == VisualizationMode.Split || visualizationMode == VisualizationMode.SectionPlane)
            {
                if (runtimeCutMode == CutMode.Free) 
                {
                    UpdateFreeCutDrag(isVR);
                }
                else 
                {
                    if (!isHit)
                    {
                        Plane virtualPlane = new Plane(mainCamera.camera.transform.forward, dragStartHit.point);
                        if (virtualPlane.Raycast(ray, out float enter)) hit.point = ray.GetPoint(enter);
                        else hit.point = dragStartHit.point;
                    }
                    UpdateAxisCutDrag(hit, isVR);
                }
            }
            
            if (visualizationMode == VisualizationMode.Spot || visualizationMode == VisualizationMode.Ring)
            {
                if (!isHit)
                {
                    Plane virtualPlane = new Plane(mainCamera.camera.transform.forward, dragStartHit.point);
                    if (virtualPlane.Raycast(ray, out float enter)) hit.point = ray.GetPoint(enter);
                    else hit.point = dragStartHit.point;
                    hit.normal = dragStartHit.normal;
                }
                UpdateCircleDrag(hit, isVR);
            }
            
            if (Time.time - lastRpcTime > 0.05f)
            {
                SyncCurrentCutState();
                lastRpcTime = Time.time;
            }
        }

        if (pointerUp)
        {
            isCutting = false;
            isDragging = false;
            activeControllerTransform = null;
            activeXrRayInteractor = null;
            SyncCurrentCutState();
        }
    }
    
    private void SyncCurrentCutState()
    {
        if (isIsolated || !IsClient || currentMat == null)
            return;
        
        Vector3 cPos = currentMat.HasProperty("_CutPosition") ? currentMat.GetVector("_CutPosition") : localPoint;
        Vector3 cNorm = currentMat.HasProperty("_CutNormal") ? currentMat.GetVector("_CutNormal") : localNormal;
        Vector3 center = currentMat.HasProperty("_Center") ? currentMat.GetVector("_Center") : localPoint;
        bool useTexIni = currentMat.HasProperty("_UseTexIni") && currentMat.GetFloat("_UseTexIni") > 0.5f;
        SyncShaderParamsServerRpc(cPos, cNorm, center, useTexIni);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SyncShaderParamsServerRpc(Vector3 cPos, Vector3 cNorm, Vector3 center, bool useTex)
    {
        netCutPosition.Value = cPos;
        netCutNormal.Value = cNorm;
        netCenter.Value = center;
        netUseTexIni.Value = useTex;
    }

    /// <summary>
    /// Aplicar modos de visualización
    /// </summary>
    public void ApplyMode(VisualizationMode mode)
    {
        visualizationMode = mode;
        if (!isIsolated && NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            SetVisualizationModeServerRpc(mode);
        }
        ApplyModeLocal(mode);
    }
    
    private void ApplyModeLocal(VisualizationMode mode)
    {
        visualizationMode = mode;
        Shader shaderToUse = standardShader;

        // Seleccionamos el shader automáticamente según el modo
        switch (mode)
        {
            case VisualizationMode.Standard:
                shaderToUse = standardShader;
                clicked = false;
                break;
            case VisualizationMode.Split:
                shaderToUse = splitShader;
                break;
            case VisualizationMode.Spot:
                shaderToUse = spotShader;
                break;
            case VisualizationMode.Ring:
                shaderToUse = ringShader;
                break;
            case VisualizationMode.SectionPlane:
                shaderToUse = sectionPlaneShader;
                break;
        }

        runtimeShader = shaderToUse;
        ApplyShader(shaderToUse);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetVisualizationModeServerRpc(VisualizationMode mode)
    {
        netVisualizationMode.Value = mode;
    }

    /// <summary>
    /// Crear y aplicar materiales con el shader seleccionado
    /// </summary>
    public void ApplyShader(Shader shader)
    {
        if (modelRenderer == null || shader == null)
            return;

        Material newMat = new Material(shader);

        int texturasAplicadas = 0;
        foreach (var pair in runtimeTextures)
        {
            if (newMat.HasProperty(pair.Key))
            {
                newMat.SetTexture(pair.Key, pair.Value);
                texturasAplicadas++;
            }
        }

        if (clicked)
            newMat.SetFloat("_UseTexIni", 1);
        else
            newMat.SetFloat("_UseTexIni", 0);
        
        if (newMat.HasProperty("_CutPosition"))
            newMat.SetVector("_CutPosition", localPoint);
        if (newMat.HasProperty("_CutNormal"))
            newMat.SetVector("_CutNormal", localNormal);
        if (newMat.HasProperty("_Center"))
            newMat.SetVector("_Center", localPoint);

        clicked = false;
        
        Material[] newMats = new Material[modelRenderer.sharedMaterials.Length];
        for (int i = 0; i < newMats.Length; i++)
        {
            newMats[i] = newMat;
        }
        modelRenderer.sharedMaterials = newMats;
        currentMat = newMat;
    }

    void UpdateShaderProperties()
    {
        if (currentMat == null)
            return;

        if (!isIsolated && IsSomeoneElseControlling())
        {
            if (currentMat.HasProperty("_CutPosition"))
                currentMat.SetVector("_CutPosition", netCutPosition.Value);
            if (currentMat.HasProperty("_CutNormal"))
                currentMat.SetVector("_CutNormal", netCutNormal.Value);
            if (currentMat.HasProperty("_Center"))
                currentMat.SetVector("_Center", netCenter.Value);
            if (currentMat.HasProperty("_UseTexIni"))
                currentMat.SetFloat("_UseTexIni", netUseTexIni.Value ? 1 : 0);
            
            runtimeRadius = netRadius.Value;
            runtimeReverse = netReverse.Value;
        }
        
        if (currentMat.HasProperty("_Radius"))
            currentMat.SetFloat("_Radius", runtimeRadius * radiusMultiplier);

        if (visualizationMode == VisualizationMode.Ring && isVisualizationModeManager)
            RenderRingPreviews();

        if (currentMat.HasProperty("_RadiusStrength"))
            currentMat.SetFloat("_RadiusStrength", radiusStrength / 1000);
        if (currentMat.HasProperty("_RadiusColor"))
            currentMat.SetColor("_RadiusColor", radiusColor);
        if (currentMat.HasProperty("_Softness"))
            currentMat.SetFloat("_Softness", softness / 1000);
        if (currentMat.HasProperty("_Reverse"))
            currentMat.SetFloat("_Reverse", runtimeReverse ? 1 : 0);
    }

    /// <summary>
    /// Actualizar el corte libre al arrastrar
    /// </summary>
    void UpdateFreeCutDrag(bool isVR)
    {
        if (currentMat == null || dragStartHit.transform == null) return;

        float threshold = isVR ? 0.0005f : 0.1f;

        if (dragVector.sqrMagnitude < threshold)
        {
            localNormal = dragStartHit.transform.InverseTransformDirection(mainCamera.camera.transform.up).normalized;
            if (currentMat.HasProperty("_CutPosition")) currentMat.SetVector("_CutPosition", dragStartLocalPos);
            if (currentMat.HasProperty("_CutNormal")) currentMat.SetVector("_CutNormal", localNormal);
            return;
        }

        Vector3 planeNormalWorld;
        
        if (isVR)
        {
            Vector3 dragDirWorld = dragVector.normalized;
            Vector3 stableForward = Vector3.ProjectOnPlane(mainCamera.camera.transform.forward, Vector3.up).normalized;
            if (stableForward == Vector3.zero) stableForward = mainCamera.camera.transform.forward;

            planeNormalWorld = Vector3.Cross(stableForward, dragDirWorld).normalized;
        }
        else
        {
            Vector3 dragDirScreen = dragVector.normalized;
            Transform camTransform = mainCamera.camera.transform;
            Vector3 dragDirWorld = ((camTransform.right * dragDirScreen.x) + (camTransform.up * dragDirScreen.y)).normalized;
            planeNormalWorld = Vector3.Cross(camTransform.forward, dragDirWorld).normalized;
        }

        localNormal = dragStartHit.transform.InverseTransformDirection(planeNormalWorld).normalized;

        if (currentMat.HasProperty("_CutPosition"))
            currentMat.SetVector("_CutPosition", dragStartLocalPos);
        
        if (currentMat.HasProperty("_CutNormal"))
            currentMat.SetVector("_CutNormal", localNormal); 
    }

    /// <summary>
    /// Actualizar el corte de los ejes al arrastrar
    /// </summary>
    void UpdateAxisCutDrag(RaycastHit hit, bool isVR)
    {
        if (currentMat == null || dragStartHit.transform == null) return;

        Vector3 axis = GetStableAxis(runtimeCutMode, isVR);

        float threshold = isVR ? 0.0005f : 0.1f;

        if (dragVector.sqrMagnitude < threshold)
        {
            localNormal = dragStartHit.transform.InverseTransformDirection(axis).normalized;
        
            if (currentMat.HasProperty("_CutPosition"))
                currentMat.SetVector("_CutPosition", dragStartLocalPos);
            
            if (currentMat.HasProperty("_CutNormal"))
                currentMat.SetVector("_CutNormal", localNormal);
            
            return;
        }

        Transform target = dragStartHit.transform;
        localPoint = target.InverseTransformPoint(hit.point);
        localNormal = target.InverseTransformDirection(axis).normalized;

        if (currentMat.HasProperty("_CutPosition"))
            currentMat.SetVector("_CutPosition", localPoint);
        
        if (currentMat.HasProperty("_CutNormal"))
            currentMat.SetVector("_CutNormal", localNormal);
    }

    /// <summary>
    /// Actualizar el corte del circulo al arrastrar
    /// </summary>
    void UpdateCircleDrag(RaycastHit hit, bool isVR)
    {
        if (currentMat == null || dragStartHit.transform == null) return;

        float threshold = isVR ? 0.0005f : 0.1f;

        if (dragVector.sqrMagnitude < threshold)
        {
            if (currentMat.HasProperty("_Center"))
                currentMat.SetVector("_Center", dragStartLocalPos);
        }
        else
        {
            Transform target = dragStartHit.transform;
            localPoint = target.InverseTransformPoint(hit.point);
        
            if (currentMat.HasProperty("_Center"))
                currentMat.SetVector("_Center", localPoint);
        }

        if (visualizationMode == VisualizationMode.Ring)
        {
            Vector3 worldNormal = hit.normal;
            Vector3 tangent = Vector3.Cross(worldNormal, Vector3.up);

            if (tangent.sqrMagnitude < 0.001f)
                tangent = Vector3.Cross(worldNormal, Vector3.right);

            localNormal = targetObject.InverseTransformDirection(worldNormal);
            localUp = targetObject.InverseTransformDirection(Vector3.Cross(worldNormal, tangent));

            hasTarget = true;
            RenderRingPreviews();
        }
    }

    void ApplyRingCamera(Camera cam, Vector3 pos, Vector3 forward, Vector3 up)
    {
        cam.transform.position = pos;
        cam.transform.rotation = Quaternion.LookRotation(-forward, -up);
    }

    /// <summary>
    /// Renderizar las preview camera (Layers) para mostrar las diferentes texturas
    /// </summary>
    public void RenderRingPreviews()
    {
        if (!hasTarget || targetObject == null || currentMat == null)
            return;
        
        if (inspectedObjectController == null)
        {
            inspectedObjectController = InspectedObjectController.Instance;
            if (inspectedObjectController == null)
                return;
        }
        
        if (true)//downloader.viewerMode == ViewerMode.basic)
        {
            Vector3 worldPoint;

            if (dragVector.sqrMagnitude < 0.1f)
                if (initialCutSet)
                    worldPoint = targetObject.TransformPoint(localPoint);
                else
                    worldPoint = targetObject.TransformPoint(dragStartLocalPos);
            else
                worldPoint = targetObject.TransformPoint(localPoint);
            
            Vector3 worldNormal = targetObject.TransformDirection(localNormal);
            Vector3 worldUp = targetObject.TransformDirection(localUp);

            Vector3 camPos = worldPoint + worldNormal;

            float radius = currentMat.GetFloat("_Radius");
            float scale = Mathf.Max(targetObject.lossyScale.x, 0.0001f);
            float effectiveRadius = radius * scale;

            float distance = Vector3.Distance(camPos, worldPoint);

            float fov = 2f * Mathf.Atan(effectiveRadius / distance) * Mathf.Rad2Deg;

            float finalFov = Mathf.Clamp(fov, minFov, maxFov);

            foreach (var slot in previewCamera)
            {
                if (slot.camera == null) continue;

                ApplyRingCamera(slot.camera, camPos, worldNormal, worldUp);

                slot.camera.fieldOfView = finalFov;

                currentMat.SetFloat("_UseTex", slot.id);

                slot.camera.enabled = true;
                slot.camera.Render();
                slot.camera.enabled = false;
            }

            currentMat.SetFloat("_UseTex", 0);
        }
        else
        {
            if (mainCamera.camera == null) return;

            Vector3 worldPoint;

            if (dragVector.sqrMagnitude < 0.1f)
                if (initialCutSet)
                    worldPoint = targetObject.TransformPoint(localPoint);
                else
                    worldPoint = targetObject.TransformPoint(dragStartLocalPos);
            else
                worldPoint = targetObject.TransformPoint(localPoint);

            Vector3 worldNormal = targetObject.TransformDirection(localNormal);
            Vector3 worldUp = targetObject.TransformDirection(localUp);

            float previewDistance = 0.15f;
            Vector3 camPos = worldPoint + worldNormal * previewDistance;

            float radius = currentMat.GetFloat("_Radius");
            float scale = Mathf.Max(targetObject.lossyScale.x, 0.0001f);
            float effectiveRadius = radius * scale;

            float distance = Vector3.Distance(camPos, worldPoint);

            distance = Mathf.Max(distance, 0.01f);

            float fov = 2f * Mathf.Atan(effectiveRadius / distance) * Mathf.Rad2Deg;
            float finalFov = Mathf.Clamp(fov, minFov, 10 * maxFov);

            foreach (var slot in previewCamera)
            {
                if (slot.camera == null) continue;

                ApplyRingCamera(slot.camera, camPos, worldNormal, worldUp);

                slot.camera.fieldOfView = finalFov;

                currentMat.SetFloat("_UseTex", slot.id);

                slot.camera.enabled = true;
                slot.camera.Render();
                slot.camera.enabled = false;
            }

            currentMat.SetFloat("_UseTex", 0);
        }
    }

    /// <summary>
    /// Restaurar al original
    /// </summary>
    [ContextMenu("ResetModelVisualization")]
    public void ResetModelVisualization()
    {
        clicked = false;
        ApplyShader(standardShader);
    }

    /// <summary>
    /// Inicializar la cámara main
    /// </summary>
    [ContextMenu("Initialize Main Camera")]
    public void InitializeMainCamera()
    {
        if (mainCamera == null) mainCamera = new MainCameraSlot();
        mainCamera.camera = Camera.main;
        
        if (mainCamera.camera == null)
            mainCamera.camera = FindAnyObjectByType<Camera>();
    }

    /// <summary>
    /// Crear una nueva preview camera (Layer) y devuelve el render texture que se mostrará
    /// </summary>
    [ContextMenu("Initialize Preview Cameras")]
    public RenderTexture InitializePreviewCamera(int id)
    {
        if (mainCamera.camera == null) return null;

        int currentCount = 0;

        if (previewCamera != null)
            currentCount = previewCamera.Length;

        PreviewCameraSlot[] newPreviewArray = new PreviewCameraSlot[currentCount + 1];

        for (int i = 0; i < currentCount; i++)
            newPreviewArray[i] = previewCamera[i];

        int newIndex = currentCount;

        GameObject camGO = new GameObject($"PreviewCamera_{newIndex + 1}");
        camGO.transform.SetParent(transform);

        Camera newCam = camGO.AddComponent<Camera>();
        newCam.CopyFrom(mainCamera.camera);
        newCam.ResetProjectionMatrix();
        newCam.targetDisplay = 0;
        // newCam.enabled = false;

        RenderTexture rt = new(485, 485, 16)
        {
            name = $"PreviewRT_{newIndex + 1}"
        };
        rt.Create();

        newCam.targetTexture = rt;

        PreviewCameraSlot slot = new()
        {
            id = id,
            obj = camGO,
            camera = newCam
        };

        newPreviewArray[newIndex] = slot;
        previewCamera = newPreviewArray;

        return rt;
    }

    public void RemovePreviewCameraAt(int index)
    {
        if (previewCamera == null || previewCamera.Length == 0) return;
        if (index < 0 || index >= previewCamera.Length) return;

        int lastIndex = previewCamera.Length - 1;

        PreviewCameraSlot slot = previewCamera[index];

        if (slot != null)
        {
            if (slot.camera != null && slot.camera.targetTexture != null)
            {
                RenderTexture rt = slot.camera.targetTexture;
                slot.camera.targetTexture = null;
                rt.Release();
                Destroy(rt);
            }

            if (slot.camera != null)
                Destroy(slot.camera.gameObject);
        }

        PreviewCameraSlot[] newPreviewArray = new PreviewCameraSlot[previewCamera.Length - 1];

        int newIndex = 0;

        for (int i = 0; i < previewCamera.Length; i++)
        {
            if (i == index) continue;

            newPreviewArray[newIndex] = previewCamera[i];
            newIndex++;
        }

        previewCamera = newPreviewArray;

        if (index != lastIndex)
        {
            for (int i = 0; i < previewCamera.Length; i++)
            {
                PreviewCameraSlot currentSlot = previewCamera[i];

                if (currentSlot == null) continue;

                if (currentSlot.camera != null)
                    currentSlot.camera.gameObject.name = $"PreviewCamera_{i + 1}";

                if (currentSlot.camera.targetTexture != null)
                    currentSlot.camera.targetTexture.name = $"PreviewRT_{i + 1}";

                if (currentSlot.id > 0)
                    currentSlot.id = currentSlot.id - 1;
            }
        }
    }
    
    public void SetIsolatedMode(bool isolated)
    {
        isIsolated = isolated;
        
        if (!isolated && NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            ForceRestoreLocalState();
        }
    }

    /// <summary>
    /// Elimina todas las preview cameras (Layers)
    /// </summary>
    public void RemoveAllPreviewCameras()
    {
        if (previewCamera == null)
            return;
        
        for (int i = 0; i < previewCamera.Length; i++)
        {
            PreviewCameraSlot slot = previewCamera[i];

            if (slot == null) continue;

            if (slot.camera != null && slot.camera.targetTexture != null)
            {
                RenderTexture rt = slot.camera.targetTexture;
                slot.camera.targetTexture = null;
                rt.Release();
                Destroy(rt);
            }

            if (slot.camera != null)
                Destroy(slot.camera.gameObject);
        }

        previewCamera = new PreviewCameraSlot[0];
    }

    public void SetShader(Shader shader)
    {
        runtimeShader = shader;
    }

    public void SetTexture(string propertyName, Texture2D texture)
    {
        runtimeTextures[propertyName] = texture;

        if (currentMat != null && currentMat.HasProperty(propertyName))
            currentMat.SetTexture(propertyName, texture);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void SetNetworkTexturesServerRpc(int mainTexId, int secTexId)
    {
        netMainTextureId.Value = mainTexId;
        netSecondaryTextureId.Value = secTexId;
    }

    private void ApplyNetworkTexture(int index, int texID, bool forceRestore = false) 
    {
        if (isIsolated && !forceRestore) return; 
        if (!forceRestore && HasToolLock()) return;
        
        string propertyName = index == 0 ? "_FirstTex" : "_SecondTex";

        if (texID <= 0 || textureManager == null) 
        {
            SetTexture(propertyName, null);
            return;
        }

        if (textureManager.IsTextureDownloaded(texID))
        {
            var data = textureManager.GetLocalTextureData(texID);
            SetTexture(propertyName, data.localTexture2D);
        }
        else
        {
            textureManager.StartDownload(texID, () => {
                var data = textureManager.GetLocalTextureData(texID);
                SetTexture(propertyName, data.localTexture2D);
            }, false);
        }
    }
    
    public void ForceRestoreLocalState()
    {
        ApplyModeLocal(netVisualizationMode.Value);
        RestoreCutStateFromNetwork();
        
        if (textureManager != null)
        {
            foreach (var tex in textureManager.GetAllRemoteTextures())
            {
                if (textureManager.IsTextureDownloaded(tex.id) && 
                    tex.id != netMainTextureId.Value && 
                    tex.id != netSecondaryTextureId.Value)
                {
                    textureManager.RemoveTexture(tex.id);
                }
            }
        }

        ApplyNetworkTexture(0, netMainTextureId.Value, true);
        ApplyNetworkTexture(1, netSecondaryTextureId.Value, true);
    }

    public void SetReverse(bool reverse)
    {
        runtimeReverse = reverse;

        if (currentMat != null && currentMat.HasProperty("_Reverse"))
            currentMat.SetFloat("_Reverse", reverse ? 1 : 0);
        
        if (!isIsolated && isVisualizationModeManager && NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
            SetReverseServerRpc(reverse);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetReverseServerRpc(bool rev)
    {
        netReverse.Value = rev;
    }

    public void SetCutMode(CutMode cutMode)
    {
        runtimeCutMode = cutMode;
        
        if (hasTarget && targetObject != null && mainCamera.camera != null)
        {
            bool isVR = activeControllerTransform != null;
            Vector3 axis = GetStableAxis(runtimeCutMode, isVR);
            localNormal = targetObject.InverseTransformDirection(axis).normalized;
            
            if (currentMat != null && currentMat.HasProperty("_CutNormal"))
                currentMat.SetVector("_CutNormal", localNormal);
                
            SyncCurrentCutState();
        }

        if (!isIsolated && isVisualizationModeManager && NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
            SetCutModeServerRpc(cutMode);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetCutModeServerRpc(CutMode cutMode)
    {
        netCutMode.Value = cutMode;
    }

    public void SetRadius(float rad)
    {
        runtimeRadius = rad;

        if (currentMat != null && currentMat.HasProperty("_Radius"))
            currentMat.SetFloat("_Radius", rad * radiusMultiplier);

        if (visualizationMode == VisualizationMode.Ring && isVisualizationModeManager)
            RenderRingPreviews();
        
        if (!isIsolated && isVisualizationModeManager && NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
            SetRadiusServerRpc(rad);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetRadiusServerRpc(float rad)
    {
        netRadius.Value = rad;
    }

    public bool isClicked()
    {
        return clicked;
    }
    
    public bool IsInteracting()
    {
        return isDragging || isCutting;
    }
    
    /// <summary>
    /// Al iniciar el modo de visualización se aplica un punto y normal inicial
    /// </summary>
    public void SetInitialCut()
    {
        if (currentMat == null || mainCamera.camera == null || modelRenderer == null)
        {
            if (mainCamera.camera == null)
                InitializeMainCamera();
            
            if (mainCamera.camera == null || currentMat == null || modelRenderer == null)
                return;
        }

        Bounds bounds = modelRenderer.bounds;
        Vector3 worldCenter = bounds.center;
        Vector3 dir = (worldCenter - mainCamera.camera.transform.position).normalized;
        Ray ray = new Ray(mainCamera.camera.transform.position, dir);
        
        targetObject = modelRenderer.transform;
        Vector3 worldNormal = -dir;
        
        bool isHit = RaycastToModel(ray, out RaycastHit hit);

        if (visualizationMode == VisualizationMode.Split || visualizationMode == VisualizationMode.SectionPlane)
        {
            localPoint = targetObject.InverseTransformPoint(worldCenter);
            
            if (isHit)
                worldNormal = hit.normal;
        }
        else
        {
            if (isHit)
            {
                localPoint = targetObject.InverseTransformPoint(hit.point);
                worldNormal = hit.normal;
            }
            else
            {
                localPoint = targetObject.InverseTransformPoint(worldCenter);
            }
        }

        bool isVR = activeControllerTransform != null; // Saber si el click vino de VR
        Vector3 axis = GetStableAxis(runtimeCutMode, isVR); // Usamos nuestra nueva función estabilizada

        Vector3 tangent = Vector3.Cross(worldNormal, Vector3.up);
        if (tangent.sqrMagnitude < 0.001f) 
            tangent = Vector3.Cross(worldNormal, Vector3.right);

        if (visualizationMode == VisualizationMode.Ring) 
            localNormal = targetObject.InverseTransformDirection(worldNormal);
        else 
            localNormal = targetObject.InverseTransformDirection(axis).normalized;

        if (tangent.sqrMagnitude > 0.001f)
            localUp = targetObject.InverseTransformDirection(Vector3.Cross(worldNormal, tangent));
        else
            localUp = targetObject.InverseTransformDirection(Vector3.up);

        if (currentMat.HasProperty("_CutPosition"))
            currentMat.SetVector("_CutPosition", localPoint);
        
        if (currentMat.HasProperty("_Center"))
            currentMat.SetVector("_Center", localPoint);
        
        if (currentMat.HasProperty("_CutNormal"))
            currentMat.SetVector("_CutNormal", localNormal);
        
        if (currentMat.HasProperty("_UseTexIni"))
            currentMat.SetFloat("_UseTexIni", 1);

        hasTarget = true;
        initialCutSet = true;

        if (visualizationMode == VisualizationMode.Ring && HasToolLock())
            RenderRingPreviews();
        
        SyncCurrentCutState();
    }

    /// <summary>
    /// Al cambiar la texturar en un modo de visualización se aplica el punto y normal ya asignado
    /// </summary>
    public void SetCut()
    {
        if (currentMat == null || mainCamera.camera == null || modelRenderer == null)
        {
            if (mainCamera.camera == null)
                InitializeMainCamera();
            
            if (mainCamera.camera == null || currentMat == null || modelRenderer == null)
                return;
        }

        if (!initialCutSet)
        {
            SetInitialCut();
            return; 
        }

        if (runtimeCutMode == CutMode.Free)
        {
            if (currentMat.HasProperty("_Center"))
                currentMat.SetVector("_Center", dragStartLocalPos);
            if (currentMat.HasProperty("_CutPosition"))
                currentMat.SetVector("_CutPosition", dragStartLocalPos);
            if (currentMat.HasProperty("_CutNormal"))
                currentMat.SetVector("_CutNormal", localNormal);
        }
        else
        {
            Vector3 targetPoint = (dragVector.sqrMagnitude < 0.1f && !initialCutSet) ? dragStartLocalPos : localPoint;
            
            if (currentMat.HasProperty("_Center"))
                currentMat.SetVector("_Center", targetPoint);
            if (currentMat.HasProperty("_CutPosition"))
                currentMat.SetVector("_CutPosition", targetPoint);
            if (currentMat.HasProperty("_CutNormal"))
                currentMat.SetVector("_CutNormal", localNormal);
        }

        if (currentMat.HasProperty("_UseTexIni")) currentMat.SetFloat("_UseTexIni", 1);

        if (visualizationMode == VisualizationMode.Ring && HasToolLock())
            RenderRingPreviews();
        
        SyncCurrentCutState();
    }

    /// <summary>
    /// Al cargar el material de las anotaciones se aplica el punto y normal
    /// </summary>
    public void SetCut(Vector3 point, Vector3 normal)
    {
        if (currentMat == null || mainCamera.camera == null || modelRenderer == null)
            return;

        localPoint = point;
        localNormal = normal;
        
        runtimeCutMode = CutMode.Free;

        currentMat.SetVector("_Center", localPoint);
        currentMat.SetVector("_CutPosition", localPoint);
        currentMat.SetVector("_CutNormal", localNormal);

        currentMat.SetFloat("_UseTexIni", 1);

        if (visualizationMode == VisualizationMode.Ring)
            RenderRingPreviews();
    }

    public void IsVisualizationModeManager(bool vmm)
    {
        isVisualizationModeManager = vmm;
    }
    
    /// <summary>
    /// Llamado desde el menú de texturas para forzar el reseteo a modo estándar en toda la red
    /// </summary>
    public void ForceResetToStandard()
    {
        ApplyModeLocal(VisualizationMode.Standard);
        
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
            ForceResetToStandardServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ForceResetToStandardServerRpc()
    {
        netVisualizationMode.Value = VisualizationMode.Standard;
        visToolLockOwner.Value = ulong.MaxValue;
    }
    
    public void RestoreCutStateFromNetwork()
    {
        if (currentMat != null)
        {
            currentMat.SetVector("_CutPosition", netCutPosition.Value);
            currentMat.SetVector("_CutNormal", netCutNormal.Value);
            currentMat.SetVector("_Center", netCenter.Value);
            currentMat.SetFloat("_UseTexIni", netUseTexIni.Value ? 1 : 0);
            
            runtimeRadius = netRadius.Value;
            runtimeReverse = netReverse.Value;
            runtimeCutMode = netCutMode.Value;
            if (visualizationMode == VisualizationMode.Spot || visualizationMode == VisualizationMode.Ring)
                localPoint = netCenter.Value;
            else
                localPoint = netCutPosition.Value;
            localNormal = netCutNormal.Value;
            
            initialCutSet = true;
            hasTarget = true;
            
            currentMat.SetFloat("_Radius", runtimeRadius * radiusMultiplier);
            currentMat.SetFloat("_Reverse", runtimeReverse ? 1 : 0);
        }
    }
    
    public void ForceAnnotationStateLocal(VisualizationMode mode, Vector3 pos, Vector3 normal, float radius, bool reverse, int tex1Id, int tex2Id)
    {
        ApplyModeLocal(mode);

        localPoint = pos;
        localNormal = normal;
        runtimeRadius = radius;
        runtimeReverse = reverse;
        runtimeCutMode = CutMode.Free;
        
        if (currentMat != null)
        {
            currentMat.SetVector("_CutPosition", localPoint);
            currentMat.SetVector("_CutNormal", localNormal);
            currentMat.SetVector("_Center", localPoint);
            currentMat.SetFloat("_Radius", runtimeRadius * radiusMultiplier);
            currentMat.SetFloat("_Reverse", runtimeReverse ? 1 : 0);
            currentMat.SetFloat("_UseTexIni", 1);
        }

        ApplyNetworkTexture(0, tex1Id, true);
        ApplyNetworkTexture(1, tex2Id, true);
        
        initialCutSet = true;
        hasTarget = true;
        
        if (mode == VisualizationMode.Ring)
            RenderRingPreviews();
    }
    
    private Vector3 GetStableAxis(CutMode mode, bool isVR)
    {
        if (isVR)
        {
            if (mode == CutMode.EjeX) 
                return Vector3.up;
            
            if (mode == CutMode.EjeY)
            {
                Vector3 flatRight = Vector3.ProjectOnPlane(mainCamera.camera.transform.right, Vector3.up).normalized;
                return flatRight == Vector3.zero ? Vector3.right : flatRight;
            }
            
            return Vector3.up;
        }
        
        if (mode == CutMode.EjeX)
            return mainCamera.camera.transform.up;
        
        if (mode == CutMode.EjeY)
            return mainCamera.camera.transform.right;
        
        return mainCamera.camera.transform.up;
    }
    
    private bool IsPointerOverUI(bool isVR)
    {
        if (isVR && activeXrRayInteractor != null)
        {
            if (activeXrRayInteractor.TryGetCurrentUIRaycastResult(out RaycastResult uiResult))
                if (uiResult.gameObject != null)
                    return true;
            
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
}