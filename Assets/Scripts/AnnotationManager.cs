using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using Button = UnityEngine.UIElements.Button;
using Toggle = UnityEngine.UIElements.Toggle;

#if UNITY_ANDROID || UNITY_IOS
using UnityEngine.Android; 
#endif

public class AnnotationManager : ToolComponent
{
    [Header("Templates & Prefabs")] 
    public VisualTreeAsset annotationItemTemplate;
    public VisualTreeAsset materialToggleTemplate;
    public VisualTreeAsset alterationFormTemplate;
    public VisualTreeAsset alterationTemplate;
    public VisualTreeAsset pointItemTemplate;
    public GameObject annotationMarkerPrefab;

    [Header("Scale Settings")] 
    public float scaleFactor = 0.025f;
    [Range(0.01f, 1f)] public float vrScaleMultiplier = 0.5f;
    [Range(0f, 1f)] public float scaleAdaptationFactor = 0.5f;
   
    private VisualElement listViewContainer;
    private ScrollView formViewContainer;
    private ScrollView annotationsListContainer;
    private VisualElement selectPointViewContainer;
    private ScrollView pointsListContainer;

    private Label emptyListText;
    private Label formInfoText;
    private Label instructionLabel;
    private Label selectAnnotationLabel;
    private TextField inputTitleField;
    private TextField inputDescriptionField;
    private DropdownField categoryDropdown;
    private DropdownField filterCategoryDropdown;

    private VisualElement materialsFoldoutRoot;
    private VisualElement alterationFormsFoldoutRoot;
    private Foldout materialInputFoldout;
    private Foldout alterationInputFoldout;
    
    private Vector3 _frozenPosition;
    private Quaternion _frozenRotation;
    private Vector3 _frozenScale;
    private bool _isModelFrozen = false;
    private Transform rootModelTransform;

    // Inputs
    private string inputTitleName = "input-name-annotation";
    private string inputDescriptionName = "input-description-annotation";
    private string inputMaterialsName = "input-materials";
    private string inputAlterationsName = "input-alterations";

    // Other UI
    private string materialsTogglesContainerName = "material-container";
    private string materialToggleName = "material-toggle";
    private string alterationToggleName = "alteration-toggle";
    private string alterationFormFoldoutName = "alteration-form-foldout";
    private string alterationFormScrollViewName = "alteration-scrollview";
    private string alterationFormsContainerName = "alteration-container";

    // Text fields
    private string annotationTextName = "annotation-text";
    private string annotationNameName = "annotation-name";
    private string annotationCategoryName = "annotation-category";
    private string annotationDateName = "annotation-date";

    // Buttons
    private Button btnAddNewAnnotation;
    private Button selectPointButton;
    private Button returnAdditionButton;
    private Button btnCancelSelectPoint;
    private Button createAnnotationButton;
    private Button drawLineButton;
    private Button addPointButton;
    private Button btnDeleteAnnotation;

    // Toggle
    private Toggle toggleShaderAnnotation;
    private Toggle togglePOVAnnotation;
    private Toggle shareToggle;
    
    private VisualElement recordAudioContainer;
    private Button btnAddRecordAudio;
    private Button btnRecordAudio;
    private Button btnReturnRecord;
    private ProgressBar progressBarRecord;

    private AudioClip recordedAudioClip;
    private string microphoneDevice;
    private bool isRecording = false;
    private float elapsedTimeRecording = 0;
    private Color originalRecordBtnColor;
    private Coroutine recordingTimerCoroutine;
    private const float MAX_RECORDING_TIME = 90f;
    
    private AnnotationData currentViewedAnnotation;
    private VisualElement rootElement;
    private InspectedObjectController inspectedObjectController;
    private Transform modelTransform;
    private bool viewMode = false;
    private CHElementData chElementDataGlobal;
    private Vector3 desiredAnnotationScale;
    
    private List<Toggle> togglesMaterials = new List<Toggle>();
    private List<Toggle> togglesAlterations = new List<Toggle>();
    private List<int> annotationsIds = new List<int>();
    private List<AnnotationData> cachedAnnotations = new List<AnnotationData>();
    
    private Vector3 savedPlayerPosition;
    private Quaternion savedPlayerRotation;
    private Quaternion savedCameraRotation;
    private bool hasSavedPosition = false;
    private Vector3 savedModelPrePOVPos;
    private Quaternion savedModelPrePOVRot;
    private Vector3 savedModelPrePOVScale;
    private Vector3 savedChildModelPos;
    private Quaternion savedChildModelRot;
    private Vector3 savedChildModelScale;
    
    public bool isGuestMode = false;
    private VisualElement invitePopup;

    public static int PendingAnnotationId = -1;
    public static ulong PendingHostId = ulong.MaxValue;
    private int annotationID = -1;
    
    private VisualizationMode snapshotMode;
    private Vector3 snapshotPos;
    private Vector3 snapshotNormal;
    private Vector3 snapshotCenter;
    private float snapshotRadius;
    private bool snapshotReverse;
    private CutMode snapshotCutMode;
    private int snapshotTex1Id = -1;
    private int snapshotTex2Id = -1;
    private int snapshotStandardTexId = -1;
    private bool snapshotUseTexIni = false;

    private readonly Dictionary<string, AnnotationCategory> categoryMap = new Dictionary<string, AnnotationCategory>()
    {
        { "Information", AnnotationCategory.info },
        { "Alteration", AnnotationCategory.damage },
        //{ "Multimedia data", AnnotationCategory.data }
    };
    
    [Serializable]
    public class LineReference
    {
        public GameObject lineObject;
        public List<Vector3> points = new List<Vector3>();
        public bool isClosed;
    }
    
    private List<object> drawnItems = new List<object>();
    private bool isDrawMode = false;
    
    [Header("Multimedia Data")]
    public VisualTreeAsset documentChipTemplate;
    public FileUploader fileUploader;
    public Sprite documentImage;
    public Sprite imageImage;
    public Sprite videoImage;
    public Sprite audioImage;

    // Elementos UI de Multimedia
    private VisualElement adMultimediaViewContainer;
    private VisualElement multimediaDocsList;
    private Button btnAddMultimediaDataForm;
    //private Button btnSelectMedia;
    private Button btnCancelMultimedia;
    //private Button btnOpenCameraPhoto;
    private Button btnTakeScreenshot;
    private VisualElement viewMultimediaContainer;
    private ScrollView viewMultimediaList;

    // Variables de control de archivos
    private int fileCounter = 0;
    private List<string> filePathsValue = new List<string>();
    private List<byte[]> fileBinariesValue = new List<byte[]>();
    private List<string> fileChecksumsValue = new List<string>();
    private List<Action> fileActionsValue = new List<Action>();
    private List<int> idAnnexDocs = new List<int>();

    // Variables de estado de subida
    private bool uploadingFile;
    private int currentUploadID;
    
    [Header("Multimedia Viewer")]
    public GameObject multimediaCanvas;
    public RawImage annexDataImage;
    public AudioSource audioSource;

    private VisualizationModeController controller;
    private TextureManager textureManager;
    public Renderer rendererModel;
    private AnnotationVisualizationType annotationVisualizationType;
    private ViewerMode viewerMode;

    public Shader standardShader;
    public Shader splitShader;
    public Shader spotShader;
    public Shader ringShader;
    public Shader sectionShader;

    protected override void OnToolActivatedInternal()
    {
        base.OnToolActivatedInternal();
        
        var baseDownloader = FindAnyObjectByType<InspectedObjectController>();
        if (baseDownloader != null)
            viewerMode = baseDownloader.viewerMode;
        
        InitializeUI();
        
        if (controller != null)
        {
            controller.SetIsolatedMode(true);
            snapshotMode = controller.visualizationMode;
            snapshotPos = controller.netCutPosition.Value;
            snapshotNormal = controller.netCutNormal.Value;
            snapshotCenter = controller.netCenter.Value;
            snapshotRadius = controller.runtimeRadius;
            snapshotReverse = controller.runtimeReverse;
            snapshotCutMode = controller.runtimeCutMode;
            snapshotTex1Id = controller.netMainTextureId.Value;
            snapshotTex2Id = controller.netSecondaryTextureId.Value;
            snapshotUseTexIni = controller.currentMat != null && controller.currentMat.HasProperty("_UseTexIni") && controller.currentMat.GetFloat("_UseTexIni") > 0.5f;
        }
        
        if (textureManager != null)
        {
            textureManager.SetIsolatedMode(true);
            snapshotStandardTexId = -1;
            foreach (var tex in textureManager.GetAllRemoteTextures())
            {
                if (tex.isVisible)
                {
                    snapshotStandardTexId = tex.id;
                    break;
                }
            }
        }
        
        FreezeModelMovement(true);
        
        if (PendingAnnotationId != -1)
        {
            ProcessPendingInvite(PendingHostId, PendingAnnotationId);
            PendingAnnotationId = -1;
            PendingHostId = ulong.MaxValue;
        }
    }

    protected override void OnToolDeactivatedInternal()
    {
        base.OnToolDeactivatedInternal();
        ClearAllMarkers();
        StopAllCoroutines();
        
        var pointSelector = FindAnyObjectByType<SelectPointAnnotation>();
        if (pointSelector != null)
            pointSelector.EnableSelectingPoint(false);
        
        if (controller != null)
            controller.SetIsolatedMode(false);
        
        if (textureManager != null)
            textureManager.SetIsolatedMode(false);
        
        RestoreOriginalPosition();
        FreezeModelMovement(false);
        EnablePlayerMovement(true);
    }
    
    private void FreezeModelMovement(bool freeze)
    {
        GetModelTransform();
        if (modelTransform != null)
        {
            var manipulableObj = modelTransform.GetComponentInParent<ManipulableObject>();
            var netTransform = modelTransform.GetComponentInParent<Unity.Netcode.Components.NetworkTransform>();

            rootModelTransform = manipulableObj != null ? manipulableObj.transform : modelTransform;

            if (netTransform != null)
            {
                netTransform.enabled = !freeze; 
            }

            if (freeze)
            {
                _frozenPosition = rootModelTransform.localPosition;
                _frozenRotation = rootModelTransform.localRotation;
                _frozenScale = rootModelTransform.localScale;
                _isModelFrozen = true;
            }
            else
            {
                _isModelFrozen = false;
                
                if (manipulableObj != null)
                {
                    manipulableObj.RequestSyncTransform();
                }
            }
        }
    }
    
    private void KeepBalancedScale(Transform marker)
    {
        Vector3 baseLocalScale = desiredAnnotationScale;
        if (baseLocalScale.x == 0) 
            return;

        float unzoomedLossyScaleAvg = scaleFactor / baseLocalScale.x;
        Vector3 parentLossy = marker.parent != null ? marker.parent.lossyScale : Vector3.one;
        float currentLossyScaleAvg = (parentLossy.x + parentLossy.y + parentLossy.z) / 3f;
        float zoomLevel = currentLossyScaleAvg / unzoomedLossyScaleAvg;
        float compensation = Mathf.Pow(zoomLevel, scaleAdaptationFactor);

        Vector3 finalScale = baseLocalScale / compensation;
        bool isVrMode = viewerMode == ViewerMode.vr;

        if (isVrMode) 
            finalScale *= vrScaleMultiplier;

        marker.localScale = finalScale;
    }
    
    public void KeepBalancedLineScale(LineRenderer lr)
    {
        if (lr == null || modelTransform == null) return;

        Vector3 baseLocalScale = desiredAnnotationScale;
        if (baseLocalScale.x == 0) return;

        float unzoomedLossyScaleAvg = scaleFactor / baseLocalScale.x;
        Vector3 parentLossy = modelTransform.lossyScale;
        float currentLossyScaleAvg = Mathf.Max((parentLossy.x + parentLossy.y + parentLossy.z) / 3f, 0.001f);
        
        float zoomLevel = currentLossyScaleAvg / unzoomedLossyScaleAvg;
        float compensation = Mathf.Pow(zoomLevel, scaleAdaptationFactor);

        DrawLineMouse drawMouse = FindAnyObjectByType<DrawLineMouse>();
        if (drawMouse == null)
            return;

        bool isVrMode = viewerMode == ViewerMode.vr;

        float activePenWidth = isVrMode ? drawMouse.penWidthVR : drawMouse.penWidth3D;
        float finalWidth = (activePenWidth * zoomLevel) / compensation;

        if (isVrMode) 
            finalWidth *= vrScaleMultiplier;

        if (finalWidth > 0f)
        {
            lr.startWidth = finalWidth;
            lr.endWidth = finalWidth;
            lr.widthMultiplier = 1f;
        }
    }

    public void KeepBalancedWorldScale(Transform child, Vector3 baseDesiredScale)
    {
        Vector3 parentLossy = child.parent != null ? child.parent.lossyScale : Vector3.one;
        float parentScaleAvg = (parentLossy.x + parentLossy.y + parentLossy.z) / 3f;
        float adjustmentMultiplier = Mathf.Pow(parentScaleAvg, scaleAdaptationFactor);

        Vector3 targetGlobalScale = baseDesiredScale * adjustmentMultiplier;

        child.localScale = new Vector3(
            targetGlobalScale.x / parentLossy.x,
            targetGlobalScale.y / parentLossy.y,
            targetGlobalScale.z / parentLossy.z
        );
    }

    public static void KeepConstantWorldScale(Transform child, Vector3 desiredWorldScale)
    {
        Vector3 parentLossy = child.parent != null ? child.parent.lossyScale : Vector3.one;

        child.localScale = new Vector3(
            desiredWorldScale.x / parentLossy.x,
            desiredWorldScale.y / parentLossy.y,
            desiredWorldScale.z / parentLossy.z
        );
    }

    private Vector3 ComputeDamageScale()
    {
        Vector3 parentLossyScale = modelTransform != null ? modelTransform.lossyScale : Vector3.one;
        float parentScaleFactor = (parentLossyScale.x + parentLossyScale.y + parentLossyScale.z) / 3f;
        
        if (parentScaleFactor < 0.001f) 
            parentScaleFactor = 1f;
        
        return Vector3.one * (scaleFactor / parentScaleFactor);
    }

    /// <summary>
    /// Crear puntos si no es la parte invisble del modo section
    /// </summary>
    public void CreatePointReference(Vector3 referencePoint)
    {
        GetModelTransform();

        if (rendererModel != null && rendererModel.sharedMaterial.shader == sectionShader)
        {
            Material material = rendererModel.sharedMaterial;
            if (material.HasProperty("_CutPosition") && material.HasProperty("_CutNormal"))
            {
                Vector3 planePosition = material.GetVector("_CutPosition");
                Vector3 planeNormal = material.GetVector("_CutNormal");

                Transform targetObject = rendererModel.transform;
                Vector3 localPoint = targetObject.InverseTransformPoint(referencePoint);

                Vector3 toPoint = localPoint - planePosition;

                float projection = Vector3.Dot(toPoint, planeNormal);

                if (projection > 0f && controller.runtimeReverse == false) return;
                if (projection < 0f && controller.runtimeReverse == true) return;
            }
        }

        GameObject newMarker = Instantiate(annotationMarkerPrefab, modelTransform);
        newMarker.transform.position = referencePoint;

        drawnItems.Add(newMarker);

        RefreshPointsAndLinesUI();
    }

    /// <summary>
    /// Crear lineas
    /// </summary>
    public void CreateLineReference(GameObject lineObj, List<Vector3> points, bool isClosed)
    {
        if (points == null || points.Count < 2) return;

        if (lineObj.GetComponent<AnnotationLineMarker>() == null)
            lineObj.AddComponent<AnnotationLineMarker>();

        drawnItems.Add(new LineReference()
        {
            lineObject = lineObj,
            points = new List<Vector3>(points),
            isClosed = isClosed
        });

        RefreshPointsAndLinesUI();
    }

    private void RemoveDrawnItem(int index)
    {
        if (index >= 0 && index < drawnItems.Count)
        {
            var item = drawnItems[index];
            if (item is GameObject marker)
            {
                if (marker != null) Destroy(marker);
            }
            else if (item is LineReference lineRef)
            {
                if (lineRef.lineObject != null) Destroy(lineRef.lineObject);
            }

            drawnItems.RemoveAt(index);
            RefreshPointsAndLinesUI();
        } 
    }

    private void ClearAllMarkers()
    {
        foreach (var item in drawnItems)
        {
            if (item is GameObject marker)
            {
                if (marker != null) Destroy(marker);
            }
            else if (item is LineReference lineRef)
            {
                if (lineRef.lineObject != null) Destroy(lineRef.lineObject);
            }
        }
        drawnItems.Clear();
        if (pointsListContainer != null)
            pointsListContainer.Clear();
        
        if (selectPointButton != null)
        {
            ColorUtility.TryParseHtmlString("#F9DFAA", out Color color);
            selectPointButton.style.backgroundColor = color;
        }
    }

    private void LateUpdate()
    {
        if (_isModelFrozen && rootModelTransform != null)
        {
            rootModelTransform.localPosition = _frozenPosition;
            rootModelTransform.localRotation = _frozenRotation;
            rootModelTransform.localScale = _frozenScale;
        }
        
        foreach (var item in drawnItems)
        {
            if (item is GameObject marker)
            {
                if (marker != null)
                    KeepBalancedScale(marker.transform);
            }
            else if (item is LineReference lineRef)
            {
                if (lineRef != null && lineRef.lineObject != null)
                {
                    LineRenderer lr = lineRef.lineObject.GetComponent<LineRenderer>();
                    if (lr != null) KeepBalancedLineScale(lr);
                }
            }
        }
    }

    private void RefreshPointsAndLinesUI()
    {
        if (pointsListContainer == null) return;
        pointsListContainer.Clear();

        for (int i = 0; i < drawnItems.Count; i++)
        {
            int index = i;
            var itemInstance = pointItemTemplate.CloneTree();

            var pointLabel = itemInstance.Q<Label>("point-label");
            var btnRemove = itemInstance.Q<Button>("btn-remove-point");
            var closedToggle = itemInstance.Q<Toggle>("is-closed-toggle");

            if (drawnItems[i] is GameObject marker)
            {
                if (pointLabel != null) pointLabel.text = $"Point {index + 1}";
                if (closedToggle != null) closedToggle.style.display = DisplayStyle.None;
                
                itemInstance.RegisterCallback<ClickEvent>(evt =>
                {
                    if (evt.target == btnRemove) return;
                    if (drawnItems[index] is GameObject m && m != null)
                    {
                        StopAllBlinks(); 
                        var annotMarker = m.GetComponent<AnnotationMarker>();
                        if (annotMarker != null) 
                            annotMarker.StartBlink();
                    }
                });
            }
            else if (drawnItems[i] is LineReference lineRef)
            {
                if (pointLabel != null) pointLabel.text = $"Line {index + 1}";

                if (closedToggle != null)
                {
                    closedToggle.style.display = DisplayStyle.Flex;
                    closedToggle.SetValueWithoutNotify(lineRef.isClosed); 

                    closedToggle.RegisterValueChangedCallback(evt =>
                    {
                        if (drawnItems[index] is LineReference lrRef)
                        {
                            lrRef.isClosed = evt.newValue;
                            if (lrRef.lineObject != null)
                            {
                                LineRenderer lr = lrRef.lineObject.GetComponent<LineRenderer>();
                                if (lr != null) lr.loop = evt.newValue; 
                            }
                        }
                    });
                }
                
                itemInstance.RegisterCallback<ClickEvent>(evt =>
                {
                    if (evt.target == btnRemove || evt.target == closedToggle) 
                        return;
            
                    if (drawnItems[index] is LineReference lrRef && lrRef.lineObject != null)
                    {
                        StopAllBlinks(); 
                        var annotMarker = lrRef.lineObject.GetComponent<AnnotationLineMarker>();
                        if (annotMarker != null) 
                            annotMarker.StartBlink();
                    }
                });
            }

            if (btnRemove != null) 
                btnRemove.clicked += () => RemoveDrawnItem(index);

            pointsListContainer.Add(itemInstance);
        }
    }
    
    private void RestorePoint(AnnotationGeometry geometry)
    {
        Vector3 point = geometry.points[0];
        GameObject newMarker = Instantiate(annotationMarkerPrefab, modelTransform);

        newMarker.transform.localPosition = point;
        drawnItems.Add(newMarker);
    }

    private void RestoreLine(AnnotationGeometry geometry, bool isVrMode)
    {
        GameObject lineObj = new GameObject("RestoredLine");
        lineObj.transform.SetParent(modelTransform, false);

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lineObj.AddComponent<AnnotationLineMarker>();
        DrawLineMouse drawMouse = FindAnyObjectByType<DrawLineMouse>();
        lr.material = drawMouse.lineMaterial;
        
        lr.startColor = geometry.color;
        lr.endColor = geometry.color;

        float activeWidth = isVrMode ? drawMouse.penWidthVR : drawMouse.penWidth3D;

        lr.startWidth = activeWidth;
        lr.endWidth = activeWidth;
        lr.widthMultiplier = 1f;
        lr.useWorldSpace = false;

        lr.positionCount = geometry.is_closed ? geometry.points.Count + 1 : geometry.points.Count;

        for (int i = 0; i < geometry.points.Count; i++)
        {
            lr.SetPosition(i, geometry.points[i]);
        }

        if (geometry.is_closed)
            lr.SetPosition(geometry.points.Count, geometry.points[0]);

        drawnItems.Add(new LineReference()
        {
            lineObject = lineObj,
            points = new List<Vector3>(geometry.points),
            isClosed = geometry.is_closed
        });
    }
    
    /// <summary>
    /// Restaurar material cargado de la anotación usando el controlador central
    /// </summary>
    private void RestoreMaterial()
    {
        if (controller == null || currentViewedAnnotation == null)
            return;

        VisualizationMode mode = VisualizationMode.Standard;
        string visTypeStr = string.IsNullOrEmpty(currentViewedAnnotation.vizualization_type) 
            ? "default" 
            : currentViewedAnnotation.vizualization_type.ToLower();

        if (visTypeStr.Contains("split"))
            mode = VisualizationMode.Split;
        else if (visTypeStr == "spot")
            mode = VisualizationMode.Spot;
        else if (visTypeStr == "ring")
            mode = VisualizationMode.Ring;
        else if (visTypeStr == "section_plane")
            mode = VisualizationMode.SectionPlane;

        Vector3 refPoint = currentViewedAnnotation.reference_point ?? Vector3.zero;
        Vector3 refNormal = currentViewedAnnotation.reference_normal ?? Vector3.up;
        float radius = currentViewedAnnotation.radius ?? 3f;
        bool isReverse = currentViewedAnnotation.reverse_mode ?? false;

        int tex1Id = -1;
        int tex2Id = -1;
        
        if (textureManager != null && currentViewedAnnotation.texture_layers != null)
        {
            var remoteTextures = textureManager.GetAllRemoteTextures();
            
            if (currentViewedAnnotation.texture_layers.Count > 0)
            {
                string tex1Name = currentViewedAnnotation.texture_layers[0].name;
                foreach (var tex in remoteTextures)
                {
                    if (tex.name.ToString() == tex1Name)
                    {
                        tex1Id = tex.id;
                        break;
                    }
                }
            }
            
            if (currentViewedAnnotation.texture_layers.Count > 1)
            {
                string tex2Name = currentViewedAnnotation.texture_layers[1].name;
                foreach (var tex in remoteTextures)
                {
                    if (tex.name.ToString() == tex2Name)
                    {
                        tex2Id = tex.id;
                        break;
                    }
                }
            }
        }

        BasicTexturesModel(currentViewedAnnotation.vizualization_type);
        if (mode == VisualizationMode.Split || mode == VisualizationMode.Spot)
            controller.ForceAnnotationStateLocal(mode, refPoint, refNormal, radius, isReverse, tex2Id, tex1Id); // Invertidas
        else
            controller.ForceAnnotationStateLocal(mode, refPoint, refNormal, radius, isReverse, tex1Id, tex2Id); // Normales
        
        if (mode == VisualizationMode.Standard && tex1Id != -1 && textureManager != null)
        {
            controller.SetTexture("_AlphaLayer", null);
            if (textureManager.IsTextureDownloaded(tex1Id))
            {
                var data = textureManager.GetLocalTextureData(tex1Id);
                if (data != null && data.localTexture2D != null)
                    controller.SetTexture("_AlphaLayer", data.localTexture2D);
            }
            else
            {
                textureManager.StartDownload(tex1Id, () => {
                    var data = textureManager.GetLocalTextureData(tex1Id);
                    if (data != null && data.localTexture2D != null)
                        controller.SetTexture("_AlphaLayer", data.localTexture2D);
                }, false);
            }
        }
    }

    /// <summary>
    /// Aplicar propiedades al cambiar shader
    /// </summary>
    private void MakeShader(Shader selectedShader, VisualizationMode visualizationMode)
    {
        controller.SetShader(selectedShader);
        controller.ApplyMode(visualizationMode);

        if (visualizationMode != VisualizationMode.Standard)
        {
            Vector3 refPoint = currentViewedAnnotation.reference_point.HasValue 
                ? currentViewedAnnotation.reference_point.Value 
                : Vector3.zero;
                
            Vector3 refNormal = currentViewedAnnotation.reference_normal.HasValue 
                ? currentViewedAnnotation.reference_normal.Value 
                : Vector3.up;
                
            float radius = currentViewedAnnotation.radius.HasValue 
                ? currentViewedAnnotation.radius.Value 
                : 3f;
                
            bool reverseMode = currentViewedAnnotation.reverse_mode.HasValue 
                ? currentViewedAnnotation.reverse_mode.Value 
                : false;

            controller.SetCut(refPoint, refNormal);
            controller.SetRadius(radius);
            controller.SetReverse(reverseMode);
        }
    }

    /// <summary>
    /// Obtener texturas básicas 
    /// </summary>
    private void BasicTexturesModel(string visualizationType)
    {
        if (rendererModel != null && rendererModel.sharedMaterial != null)
        {
            Texture2D modelAlbedoTexture = rendererModel.sharedMaterial.GetTexture("_BaseMap") as Texture2D;
            Texture2D modelNormalTexture = rendererModel.sharedMaterial.GetTexture("_BumpMap") as Texture2D;
            Texture2D modelOcclusionTexture = rendererModel.sharedMaterial.GetTexture("_OcclusionMap") as Texture2D;

            if (modelAlbedoTexture != null)
            {
                controller.SetTexture("_BaseMap", modelAlbedoTexture);

                if (visualizationType != HelpFunctionsConditionReport.AnnotationVisualizationTypeToString(AnnotationVisualizationType.def))
                    controller.SetTexture("_FirstTex", modelAlbedoTexture);
            }
            else
                Debug.LogWarning("[AnnotationManager] Model has no _BaseMap texture.");

            if (modelNormalTexture != null)
            {
                controller.SetTexture("_BumpMap", modelNormalTexture);
            }
            else
                Debug.LogWarning("[AnnotationManager] Model has no _BumpMap texture.");

            if (modelOcclusionTexture != null)
            {
                controller.SetTexture("_OcclusionMap", modelOcclusionTexture);
            }
            else
                Debug.LogWarning("[AnnotationManager] Model has no _OcclusionMap texture.");
        }
    }

    /// <summary>
    /// Comprobar texturas descargadas
    /// </summary>
    private void EnsureMemoryLimit(int newTextureId)
    {
        while (GetDownloadedCount() >= textureManager.GetMaxDownloadedTextures())
        {
            // Borrar cualquier textura descargada que no sea la nueva
            foreach (var tex in textureManager.GetAllRemoteTextures())
            {
                if (tex.id == newTextureId) continue;
                if (!textureManager.IsTextureDownloaded(tex.id)) continue;

                textureManager.RemoveTexture(tex.id);

                Debug.Log($"[AnnotationManager] Non-runtime texture removed: {tex.name}.");

                break;
            }
        }
    }
    
    private string GetUserFolderPath()
    {
        string currentUsername = GlobalManagement.Instance.username;
        if (string.IsNullOrEmpty(currentUsername)) 
            currentUsername = "offline_user";
        
        string path = Path.Combine(Application.persistentDataPath, currentUsername);
        if (!Directory.Exists(path)) 
            Directory.CreateDirectory(path);
            
        return path;
    }

    private int GetDownloadedCount()
    {
        int count = 0;

        foreach (var tex in textureManager.GetAllRemoteTextures())
            if (textureManager.IsTextureDownloaded(tex.id))
                count++;

        return count;
    }
    
    private void InitializeUI()
    {
        rootElement = uIDocument.rootVisualElement;
        
        listViewContainer = rootElement.Q<VisualElement>("list-view-container");
        formViewContainer = rootElement.Q<ScrollView>("form-view-container");
        annotationsListContainer = rootElement.Q<ScrollView>("annotations-list-container");
        selectPointViewContainer = rootElement.Q<VisualElement>("select-point-view-container");
        pointsListContainer = rootElement.Q<ScrollView>("points-list-container");

        emptyListText = rootElement.Q<Label>("empty-list-text");
        formInfoText = rootElement.Q<Label>("form-info-text");
        inputTitleField = rootElement.Q<TextField>("input-name-textfield");
        inputDescriptionField = rootElement.Q<TextField>("input-description-textfield");
        
        categoryDropdown = rootElement.Q<DropdownField>("input-category-dropdown");
        filterCategoryDropdown = rootElement.Q<DropdownField>("filter-category-dropdown");

        if (categoryDropdown != null)
            categoryDropdown.RegisterValueChangedCallback(evt => UpdateCategoryFieldsVisibility(evt.newValue));
        
        if (filterCategoryDropdown != null)
        {
            filterCategoryDropdown.SetValueWithoutNotify("All");
            filterCategoryDropdown.RegisterValueChangedCallback(evt => FilterAnnotationsList(evt.newValue));
        }

        materialsFoldoutRoot = rootElement.Q<VisualElement>("material-container");
        alterationFormsFoldoutRoot = rootElement.Q<VisualElement>("alteration-container");
        materialInputFoldout = rootElement.Q<Foldout>("material-input");
        alterationInputFoldout = rootElement.Q<Foldout>("alteration-input");

        btnAddNewAnnotation = rootElement.Q<Button>("btn-add-annotation");
        selectPointButton = rootElement.Q<Button>("select-point-button");
        returnAdditionButton = rootElement.Q<Button>("return-addition-button");
        createAnnotationButton = rootElement.Q<Button>("input-button-annotation");
        btnCancelSelectPoint = rootElement.Q<Button>("cancel-select-point-button");
        drawLineButton = rootElement.Q<Button>("draw-line-button");
        addPointButton = rootElement.Q<Button>("add-point-button");
        instructionLabel = rootElement.Q<Label>("instruction-label");
        selectAnnotationLabel = rootElement.Q<Label>("select-annotation-label");
        viewMultimediaContainer = rootElement.Q<VisualElement>("multimedia-data");
        viewMultimediaList = rootElement.Q<ScrollView>("multimedia-container-list");
        btnDeleteAnnotation = rootElement.Q<Button>("btn-delete-annotation");
        toggleShaderAnnotation = rootElement.Q<Toggle>("toggle-shader-annotation");
        togglePOVAnnotation = rootElement.Q<Toggle>("toggle-pov-annotation");
        shareToggle = rootElement.Q<Toggle>("share-toggle");

        if (btnAddNewAnnotation != null) 
            btnAddNewAnnotation.clicked += OpenAddMode;
        if (returnAdditionButton != null) 
            returnAdditionButton.clicked += OpenListView;
        if (selectPointButton != null) 
            selectPointButton.clicked += () => ShowSelectPointView(true);
        if (createAnnotationButton != null) 
            createAnnotationButton.clicked += CreateAnnotation;
        if (addPointButton != null) 
            addPointButton.clicked += () => SetInputMode(false);
        if (drawLineButton != null) 
            drawLineButton.clicked += () => SetInputMode(true);
        if (btnDeleteAnnotation != null) 
            btnDeleteAnnotation.clicked += OnClickDeleteButton;

        toggleShaderAnnotation?.RegisterValueChangedCallback(OnChangeShader);
        togglePOVAnnotation?.RegisterValueChangedCallback(OnChangePOV);

        if (btnCancelSelectPoint != null)
        {
            btnCancelSelectPoint.clicked -= null; 
            btnCancelSelectPoint.clicked += () => ShowSelectPointView(false);
        }
        
        adMultimediaViewContainer = rootElement.Q<VisualElement>("ad-multimedia-view-container");
        multimediaDocsList = rootElement.Q<VisualElement>("multimedia-doc-list");
        btnAddMultimediaDataForm = rootElement.Q<Button>("ad-multimedia-data");
        //btnSelectMedia = rootElement.Q<Button>("btn-select-media");
        btnCancelMultimedia = rootElement.Q<Button>("btn-cancel-multimedia");
        //btnOpenCameraPhoto = rootElement.Q<Button>("btn-open-camera-photo");
        btnTakeScreenshot = rootElement.Q<Button>("btn-take-screenshot");

        if (btnAddMultimediaDataForm != null) 
            btnAddMultimediaDataForm.clicked += () => ShowMultimediaView(true);
        if (btnCancelMultimedia != null) 
            btnCancelMultimedia.clicked += () => ShowMultimediaView(false);
        /*if (btnSelectMedia != null) 
            btnSelectMedia.clicked += PickFromGallery;
        if (btnOpenCameraPhoto != null)
            btnOpenCameraPhoto.clicked += PickFromCameraPhoto;*/
        if (btnTakeScreenshot != null) 
            btnTakeScreenshot.clicked += () => StartCoroutine(TakeScreenshotRoutine());
        
        recordAudioContainer = rootElement.Q<VisualElement>("record-audio-container");
        btnAddRecordAudio = rootElement.Q<Button>("btn-add-record-audio");
        btnRecordAudio = rootElement.Q<Button>("btn-record-audio");
        btnReturnRecord = rootElement.Q<Button>("btn-cancel-record-audio");
        progressBarRecord = rootElement.Q<ProgressBar>("progress-bar-record");
        
        if (btnAddRecordAudio != null) 
            btnAddRecordAudio.clicked += () => ShowRecordAudioView(true);
        if (btnReturnRecord != null) 
            btnReturnRecord.clicked += () => ShowRecordAudioView(false);

        if (btnRecordAudio != null)
        {
            originalRecordBtnColor = btnRecordAudio.resolvedStyle.unityBackgroundImageTintColor;
            
            btnRecordAudio.RegisterCallback<PointerDownEvent>(evt =>
            {
                btnRecordAudio.style.unityBackgroundImageTintColor = Color.red;
                StartRecordingAudio();
            }, TrickleDown.TrickleDown);

            btnRecordAudio.RegisterCallback<PointerUpEvent>(evt =>
            {
                btnRecordAudio.style.unityBackgroundImageTintColor = originalRecordBtnColor;
                StopRecordingAudioAndReturn();
            }, TrickleDown.TrickleDown);

            btnRecordAudio.RegisterCallback<PointerCaptureOutEvent>(evt =>
            {
                if (isRecording)
                { 
                    btnRecordAudio.style.unityBackgroundImageTintColor = originalRecordBtnColor;
                    StopRecordingAudioAndReturn();
                }
            });
        }

        inspectedObjectController = FindAnyObjectByType<InspectedObjectController>();
        InitializeSession();

        ModelReferences();

        if (AnnotationNetworkSync.Instance != null)
            AnnotationNetworkSync.Instance.OnFormUpdated += SyncGuestForm;

        inputTitleField.RegisterValueChangedCallback(evt => {
            if (!isGuestMode) AnnotationNetworkSync.Instance?.SendFormUpdate(evt.newValue, inputDescriptionField.value, categoryDropdown.value);
        });
        inputDescriptionField.RegisterValueChangedCallback(evt => {
            if (!isGuestMode) AnnotationNetworkSync.Instance?.SendFormUpdate(inputTitleField.value, evt.newValue, categoryDropdown.value);
        });
        categoryDropdown.RegisterValueChangedCallback(evt => {
            UpdateCategoryFieldsVisibility(evt.newValue);
            if (!isGuestMode) AnnotationNetworkSync.Instance?.SendFormUpdate(inputTitleField.value, inputDescriptionField.value, evt.newValue);
        });
        
        inputTitleField.RegisterCallback<FocusInEvent>(evt => EnablePlayerMovement(false));
        inputTitleField.RegisterCallback<FocusOutEvent>(evt => { if (!viewMode) EnablePlayerMovement(true); });
        
        inputDescriptionField.RegisterCallback<FocusInEvent>(evt => EnablePlayerMovement(false));
        inputDescriptionField.RegisterCallback<FocusOutEvent>(evt => { if (!viewMode) EnablePlayerMovement(true); });
        
        rootElement.RegisterCallback<PointerDownEvent>(evt =>
        {
            if (evt.target is not TextField)
            {
                var focusController = uIDocument.rootVisualElement.focusController;
                if (focusController != null && focusController.focusedElement != null)
                    focusController.focusedElement.Blur();
            }
        });
    }

    private void ModelReferences()
    {
        controller = FindFirstObjectByType<VisualizationModeController>();
        textureManager = FindFirstObjectByType<TextureManager>();

        rendererModel = textureManager.GetModelRenderer();

        // Texture2D albedoTex;

        annotationVisualizationType = controller.visualizationMode switch
        {
            VisualizationMode.Split => AnnotationVisualizationType.split,
            VisualizationMode.Spot => AnnotationVisualizationType.spot,
            VisualizationMode.Ring => AnnotationVisualizationType.ring,
            VisualizationMode.SectionPlane => AnnotationVisualizationType.section_plane,
            _ => AnnotationVisualizationType.def,
        };
    }

    public void InitializeSession()
    {
        GetModelTransform();
        RefreshAnnotationList();
        
        StartCoroutine(CHElementDB.GetCHElementByID(OnChElementReceived, inspectedObjectController.GetChElementID()));
    }
    
    public void OpenListView()
    {
        EnablePlayerMovement(true);
        isGuestMode = false;
        CloseMultimedia();
        viewMode = false;
        
        if (toggleShaderAnnotation != null && toggleShaderAnnotation.value)
        {
            toggleShaderAnnotation.SetValueWithoutNotify(false);
            if (controller != null)
            {
                controller.ForceRestoreLocalState();
            }
        }
        
        listViewContainer.style.display = DisplayStyle.Flex;
        formViewContainer.style.display = DisplayStyle.None;

        ClearAllMarkers();
        FindAnyObjectByType<SelectPointAnnotation>()?.EnableSelectingPoint(false);
        
        if (togglePOVAnnotation != null)
            togglePOVAnnotation.SetValueWithoutNotify(false);
        RestoreOriginalPosition();
    }

    private void OpenAddMode()
    {
        EnablePlayerMovement(true);
        CloseMultimedia();
        viewMode = false;
        listViewContainer.style.display = DisplayStyle.None;
        formViewContainer.style.display = DisplayStyle.Flex;
        formInfoText.text = "Add new annotation";

        inputTitleField.value = "";
        inputDescriptionField.value = "";
        categoryDropdown.value = "Information";
        UpdateCategoryFieldsVisibility(categoryDropdown.value);

        inputTitleField.isReadOnly = false;
        inputDescriptionField.isReadOnly = false;
        categoryDropdown.SetEnabled(true);

        foreach (var t in togglesMaterials)
        {
            t.SetEnabled(true);
            t.SetValueWithoutNotify(false);
        }

        foreach (var t in togglesAlterations)
        {
            t.SetEnabled(true);
            t.SetValueWithoutNotify(false);
        }

        if (alterationFormsFoldoutRoot != null)
        {
            var allFoldouts = alterationFormsFoldoutRoot.Query<Foldout>(alterationFormFoldoutName).ToList();
            foreach (var foldout in allFoldouts)
            {
                foldout.value = false;
            }
        }

        selectPointButton.style.display = DisplayStyle.Flex;
        createAnnotationButton.style.display = DisplayStyle.Flex;
        
        if (btnDeleteAnnotation != null)
            btnDeleteAnnotation.style.display = DisplayStyle.None;
        
        if (toggleShaderAnnotation != null)
            toggleShaderAnnotation.style.display = DisplayStyle.None;
        
        if (togglePOVAnnotation != null)
            togglePOVAnnotation.style.display = DisplayStyle.None;
        
        if (btnAddMultimediaDataForm != null) 
            btnAddMultimediaDataForm.style.display = DisplayStyle.Flex;
        
        if (viewMultimediaContainer != null) 
            viewMultimediaContainer.style.display = DisplayStyle.None;
        
        if (shareToggle != null)
        {
            shareToggle.style.display = DisplayStyle.Flex;
            shareToggle.value = false;
        }

        ClearMultimediaData();
        ClearAllMarkers();
        ApplyGuestLock();
    }

    public void OpenViewMode(AnnotationData annotation, AlterationEventData alterationEvent = null)
    {
        Debug.Log($"[DEBUG Anotaciones] Visualizando anotación ID {annotation.id}:\n{JsonConvert.SerializeObject(annotation, Formatting.Indented)}");
        CloseMultimedia();
        viewMode = true;
        currentViewedAnnotation = annotation;
        listViewContainer.style.display = DisplayStyle.None;
        formViewContainer.style.display = DisplayStyle.Flex;
        formInfoText.text = "Annotation details";

        inputTitleField.value = annotation.name;
        inputDescriptionField.value = annotation.description;

        string cat = string.IsNullOrEmpty(annotation.category)
            ? "Information"
            : HelpFunctionsConditionReport.AnnotationCategoryDatabaseToString(annotation.category);
        
        categoryDropdown.value = cat;
        UpdateCategoryFieldsVisibility(cat);

        inputTitleField.isReadOnly = true;
        inputDescriptionField.isReadOnly = true;
        categoryDropdown.SetEnabled(false);

        if (cat == "Alteration" && alterationEvent != null)
        {
            FillMaterialViewMode(alterationEvent.id_material);
            FillAlterationsViewMode(alterationEvent.alterations);
        }

        selectPointButton.style.display = DisplayStyle.None;
        createAnnotationButton.style.display = DisplayStyle.None;

        if (toggleShaderAnnotation != null)
        {
            if (GlobalManagement.Instance != null)
            {
                toggleShaderAnnotation.style.display = DisplayStyle.Flex;
                
                if (togglePOVAnnotation != null)
                {
                    togglePOVAnnotation.style.display = DisplayStyle.Flex;
                    togglePOVAnnotation.SetValueWithoutNotify(false);
                }
            }
            else
            {
                toggleShaderAnnotation.style.display = DisplayStyle.None;
            }
        }
        
        if (btnDeleteAnnotation != null)
        {
            if (GlobalManagement.Instance != null && !string.IsNullOrEmpty(annotation.username) && 
                     annotation.username == GlobalManagement.Instance.username)
            {
                btnDeleteAnnotation.style.display = DisplayStyle.Flex;
            }
            else
            {
                btnDeleteAnnotation.style.display = DisplayStyle.None;
            }
        }
        
        if (btnAddMultimediaDataForm != null) 
            btnAddMultimediaDataForm.style.display = DisplayStyle.None;
        
        if (shareToggle != null)
            shareToggle.style.display = DisplayStyle.None;
            
        ClearMultimediaData();
        
        if (annotation.annotation_infos != null && annotation.annotation_infos.Count > 0)
        {
            if (viewMultimediaContainer != null) 
                viewMultimediaContainer.style.display = DisplayStyle.Flex;
                
            FillMultimediaReadOnly(annotation.annotation_infos);
        }
        else
        {
            if (viewMultimediaContainer != null) 
                viewMultimediaContainer.style.display = DisplayStyle.None;
        }
        
        RestoreAnnotation(annotation);
        ApplyGuestLock();
    }
    
    private void ClearMultimediaData()
    {
        if (multimediaDocsList != null) 
            multimediaDocsList.Clear();
            
        filePathsValue.Clear();
        fileBinariesValue.Clear();
        fileChecksumsValue.Clear();
        fileActionsValue.Clear();
        idAnnexDocs.Clear();
        fileCounter = 0;
        
        if (btnAddMultimediaDataForm != null)
        {
            ColorUtility.TryParseHtmlString("#F9DFAA", out Color color);
            btnAddMultimediaDataForm.style.backgroundColor = color;
        }
    }

    public void ShowSelectPointView(bool show)
    {
        if (show)
        {
            formViewContainer.style.display = DisplayStyle.None;
            selectPointViewContainer.style.display = DisplayStyle.Flex;
            SetInputMode(false); 
        }
        else
        {
            selectPointViewContainer.style.display = DisplayStyle.None;
            formViewContainer.style.display = DisplayStyle.Flex;
            
            FindAnyObjectByType<DrawLineMouse>()?.EnableDrawing(false);
            FindAnyObjectByType<SelectPointAnnotation>()?.EnableSelectingPoint(false);

            if (pointsListContainer.childCount > 0)
            {
                ColorUtility.TryParseHtmlString("#A3CC70", out Color color);
                selectPointButton.style.backgroundColor = color;
            }
            else
            {
                ColorUtility.TryParseHtmlString("#F9DFAA", out Color color);
                selectPointButton.style.backgroundColor = color;
            }
        }
    }
    
    private void SetInputMode(bool lineMode)
    {
        isDrawMode = lineMode;
        
        FindAnyObjectByType<DrawLineMouse>()?.EnableDrawing(isDrawMode);
        FindAnyObjectByType<SelectPointAnnotation>()?.EnableSelectingPoint(!isDrawMode);
        
        if (selectAnnotationLabel != null)
        {
            selectAnnotationLabel.text = isDrawMode ? 
                "Select Annotation 3D Line" : 
                "Select Annotation 3D Point";
        }

        if (instructionLabel != null)
        {
            instructionLabel.text = isDrawMode ? 
                "Draw a line directly on the 3D model by clicking and dragging." : 
                "Tap or click directly on the 3D model to place the annotation marker.";
        }

        Color activeColor = new Color(249f / 255f, 223f / 255f, 170f / 255f);
        Color inactiveColor = new Color(200f / 255f, 200f / 255f, 200f / 255f);

        if (addPointButton != null)
            addPointButton.style.backgroundColor = isDrawMode ? inactiveColor : activeColor;

        if (drawLineButton != null)
            drawLineButton.style.backgroundColor = isDrawMode ? activeColor : inactiveColor;

        RefreshPointsAndLinesUI();
    }
    
    private void RefreshAnnotationList()
    {
        StartCoroutine(ThreeDInstanceDB.GetAnnotationListFromThreeDInstance(OnAnnotationsListReceived, inspectedObjectController.GetE3DInstanceID(), perPage: 1000));
    }
    
    private void FilterAnnotationsList(string filterValue)
    {
        if (cachedAnnotations == null) return;

        if (filterValue == "All" || string.IsNullOrEmpty(filterValue))
        {
            InsertAnnotationInformation(cachedAnnotations);
        }
        else if (categoryMap.TryGetValue(filterValue, out AnnotationCategory targetCategory))
        {
            string targetCatStr = targetCategory.ToString().ToLower();
            var filteredList = cachedAnnotations.Where(a => 
                !string.IsNullOrEmpty(a.category) && 
                a.category.ToLower() == targetCatStr
            ).ToList();
        
            InsertAnnotationInformation(filteredList);
        }
        else
        {
            InsertAnnotationInformation(cachedAnnotations);
        }
    }

    private void InsertAnnotationInformation(List<AnnotationData> annotations)
    {
        annotationsListContainer.Clear();
        
        if (annotations.Count == 0)
        {
            if (emptyListText != null) emptyListText.style.display = DisplayStyle.Flex;
            return;
        }
        
        if (emptyListText != null) emptyListText.style.display = DisplayStyle.None;

        for (int i = 0; i < annotations.Count; i++)
        {
            var element = annotationItemTemplate.CloneTree();
            var item = annotations[i];

            var labelName = element.Q<Label>(annotationNameName);
            var labelCategory = element.Q<Label>(annotationCategoryName);
            var labelDate = element.Q<Label>(annotationDateName);
            var labelUsername = element.Q<Label>("annotation-username");

            if (labelName != null) labelName.text = item.name;
            
            if (labelCategory != null)
                labelCategory.text = !string.IsNullOrEmpty(item.category) ? "Type: " + HelpFunctionsConditionReport.AnnotationCategoryDatabaseToString(item.category) : "No type";
            
            if (labelUsername != null)
                labelUsername.text = string.IsNullOrEmpty(item.username) ? "Unknown author" : "Author: " + item.username;

            if (labelDate != null)
            {
                if (DateTime.TryParse(item.created_on, out DateTime parsedDate))
                    labelDate.text = parsedDate.ToString("dd/MM/yyyy");
                else
                    labelDate.text = string.IsNullOrEmpty(item.created_on) ? "--/--/----" : item.created_on; 
            }

            element.RegisterCallback<ClickEvent>(ev =>
            {
                if (!string.IsNullOrEmpty(item.category) && item.category.ToLower() == "damage")
                {
                    StartCoroutine(AnnotationDB.GetAlterationEventListFromAnnotation((response, success) => 
                    {
                        if (success && response != null && response.items != null && response.items.Count > 0)
                            OpenViewMode(item, response.items[0]);
                        else
                            OpenViewMode(item, null); 
                    }, item.id));
                }
                else
                {
                    OpenViewMode(item, null);
                }
            });

            annotationsListContainer.Add(element);
        }
    }

    private void UpdateCategoryFieldsVisibility(string category)
    {
        bool isAlteration = !string.IsNullOrEmpty(category) && category == "Alteration";
        DisplayStyle targetDisplay = isAlteration ? DisplayStyle.Flex : DisplayStyle.None;

        if (materialInputFoldout != null) 
            materialInputFoldout.style.display = targetDisplay;
        
        if (alterationInputFoldout != null) 
            alterationInputFoldout.style.display = targetDisplay;
    }

    public void GetModelTransform()
    {
        if (inspectedObjectController.activeModelInstance == null)
        {
            Debug.LogWarning("El modelo 3D aún no ha cargado. Esperando para calcular el transform.");
            return;
        }

        modelTransform = inspectedObjectController.activeModelInstance.transform;
    
        if (desiredAnnotationScale == Vector3.zero || desiredAnnotationScale == Vector3.one)
            desiredAnnotationScale = ComputeDamageScale();
    }

    private void RestoreAnnotation(AnnotationData data)
    {
        ClearAllMarkers();

        toggleShaderAnnotation.value = false;
        if (togglePOVAnnotation != null)
            togglePOVAnnotation.SetValueWithoutNotify(false);
        
        bool isVrMode = viewerMode == ViewerMode.vr;

        if (data.annotation_geometries != null && data.annotation_geometries.Count > 0)
        {
            foreach (var geometry in data.annotation_geometries)
            {
                if (geometry.points == null || geometry.points.Count == 0)
                    continue;

                if (geometry.points.Count == 1)
                    RestorePoint(geometry);
                else
                    RestoreLine(geometry, isVrMode);
            }
        }

        RefreshPointsAndLinesUI();
    }
    
    private void ApplyGuestLock()
    {
        if (!isGuestMode)
            return;

        inputTitleField.isReadOnly = true;
        inputDescriptionField.isReadOnly = true;
        categoryDropdown.SetEnabled(false);

        foreach (var t in togglesMaterials) t.SetEnabled(false);
        foreach (var t in togglesAlterations) t.SetEnabled(false);

        createAnnotationButton.style.display = DisplayStyle.None;
        selectPointButton.style.display = DisplayStyle.None;
        drawLineButton.style.display = DisplayStyle.None;
        addPointButton.style.display = DisplayStyle.None;
        
        if (btnAddRecordAudio != null)
            btnAddRecordAudio.style.display = DisplayStyle.None;
        if (btnAddMultimediaDataForm != null)
            btnAddMultimediaDataForm.style.display = DisplayStyle.None;
        if (btnDeleteAnnotation != null)
            btnDeleteAnnotation.style.display = DisplayStyle.None;

        ToolMessageHandler.Instance?.ShowMessage("You are viewing in Read-Only mode.", 3f, MessageType.Info);
    }
    
    private void CheckAndShareAnnotation(int annotationId)
    {
        if (shareToggle != null && shareToggle.value)
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.SpawnManager != null)
            {
                var localPlayerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                if (localPlayerObj != null)
                {
                    var localController = localPlayerObj.GetComponent<UserControllerPlayer>();
                    if (localController != null)
                    {
                        localController.SendGlobalAnnotationInvitation(annotationId);
                        ToolMessageHandler.Instance?.ShowMessage("Annotation shared with the room!");
                    }
                }
            }
        }
    }
    
    private void CreateAnnotation()
    {
        if (CheckNeccessaryData())
        {
            createAnnotationButton.SetEnabled(false);
            StartCoroutine(UploadFilesAndCreateAnnotation());
        }
    }

    private IEnumerator UploadFilesAndCreateAnnotation()
    {
        idAnnexDocs.Clear();
        List<AnnotationInfo> annotationInfos = new List<AnnotationInfo>();

        if (filePathsValue.Count > 0)
        {
            fileCounter = 0;
            foreach (var path in filePathsValue)
            {
                string fileName = Path.GetFileName(path);
                uploadingFile = true;

                StartCoroutine(UploadDB.PostNewUpload(OnUploadCompleted, fileChecksumsValue[fileCounter], fileName, FileTypeCategory.annex_data, null, 1));
                
                yield return new WaitUntil(() => !uploadingFile);
                fileCounter++;
            }

            for (int i = 0; i < idAnnexDocs.Count; i++)
            {
                string path = filePathsValue[i];
                string ext = Path.GetExtension(path).ToLower();
                string backendType = (ext == ".mp3" || ext == ".wav" || ext == ".ogg") ? "Sound" : "Image";
                
                annotationInfos.Add(new AnnotationInfo(
                    annotationInfoName: Path.GetFileName(path),
                    createdOn: DateTime.Now.ToString("s"),
                    file: idAnnexDocs[i],
                    id: 0, 
                    idAnnotation: 0, 
                    type: backendType
                ));
            }
        }

        FinalizeAnnotationCreation(annotationInfos);
    }
    
    private void FinalizeAnnotationCreation(List<AnnotationInfo> annotationInfos)
    {
        if (!categoryMap.TryGetValue(categoryDropdown.value, out AnnotationCategory selectedCategory))
            Debug.LogWarning("Dropdown value not recognised: " + categoryDropdown.value);
        
        string modelMatrixString = HelpFunctionsConditionReport.Matrix4X4ToString(modelTransform.localToWorldMatrix);

        if (Camera.main != null)
        {
            GameObject camera = Camera.main.gameObject;
            string cameraMatrixString = HelpFunctionsConditionReport.Matrix4X4ToString(camera.transform.localToWorldMatrix);
            
            List<AnnotationGeometry> geometryList = new List<AnnotationGeometry>();
            
            for (int i = 0; i < drawnItems.Count; i++)
            {
                if (drawnItems[i] is GameObject marker)
                {
                    List<Vector3> singlePointList = new List<Vector3>() { marker.transform.localPosition };
                    geometryList.Add(new AnnotationGeometry(Color.red, i, false, singlePointList));
                }
                else if (drawnItems[i] is LineReference line)
                {
                    DrawLineMouse drawMouse = FindAnyObjectByType<DrawLineMouse>();
                    Color lineColor = drawMouse != null ? drawMouse.penColor : Color.red;
                    geometryList.Add(new AnnotationGeometry(lineColor, i, line.isClosed, line.points));
                }
            }
            
            Vector3? point = (snapshotMode == VisualizationMode.Spot || snapshotMode == VisualizationMode.Ring) ? snapshotCenter : snapshotPos;
            Vector3? normal = snapshotNormal;
            List<int> texId = new();

            if (snapshotMode == VisualizationMode.Standard)
            {
                if (snapshotStandardTexId != -1 && textureManager != null)
                {
                    var tex = textureManager.GetRemoteTextureByID(snapshotStandardTexId);
                    if (!string.IsNullOrEmpty(tex.name.ToString()))
                    {
                        int layerId = textureManager.GetTextureIdDB(tex.name.ToString());
                        if (layerId != -1)
                            texId.Add(layerId);
                    }
                }
            }
            else
            {
                if (snapshotTex1Id != -1 && textureManager != null)
                {
                    var tex = textureManager.GetRemoteTextureByID(snapshotTex1Id);
                    if (!string.IsNullOrEmpty(tex.name.ToString()))
                    {
                        int layerId = textureManager.GetTextureIdDB(tex.name.ToString());
                        if (layerId != -1) texId.Add(layerId);
                    }
                }
                if (snapshotTex2Id != -1 && textureManager != null && snapshotMode != VisualizationMode.Ring && snapshotMode != VisualizationMode.SectionPlane)
                {
                    var tex = textureManager.GetRemoteTextureByID(snapshotTex2Id);
                    if (!string.IsNullOrEmpty(tex.name.ToString()))
                    {
                        int layerId = textureManager.GetTextureIdDB(tex.name.ToString());
                        if (layerId != -1) texId.Add(layerId);
                    }
                }
            }
            
            AnnotationVisualizationType currentVisType = AnnotationVisualizationType.def;
            if (controller != null)
            {
                if (snapshotMode == VisualizationMode.Split)
                {
                    if (snapshotCutMode == CutMode.EjeX)
                        currentVisType = AnnotationVisualizationType.horizontal_split;
                    else if (snapshotCutMode == CutMode.EjeY)
                        currentVisType = AnnotationVisualizationType.vertical_split;
                    else
                        currentVisType = AnnotationVisualizationType.split;
                }
                else
                {
                    currentVisType = snapshotMode switch
                    {
                        VisualizationMode.Spot => AnnotationVisualizationType.spot,
                        VisualizationMode.Ring => AnnotationVisualizationType.ring,
                        VisualizationMode.SectionPlane => AnnotationVisualizationType.section_plane,
                        _ => AnnotationVisualizationType.def,
                    };
                }
            }

            StartCoroutine(ThreeDInstanceDB.PostNewAnnotationToThreeDInstance(OnAnnotationCreated,
                inspectedObjectController.GetE3DInstanceID(), inputDescriptionField.value, null,
                geometryList, annotationInfos, selectedCategory,
                Color.blue, null, null, texId.Count != 0 ? texId : null, inputTitleField.value, snapshotRadius,
                normal, point, snapshotReverse, null, modelMatrixString, AnnotationType._3D,
                cameraMatrixString, currentVisType));
        }
    }

    private void OnAnnotationCreated(AnnotationData annotation, bool success)
    {
        createAnnotationButton.SetEnabled(true);
        
        if (!success)
        {
            Debug.LogError("Error al crear la anotación. Revisa si falta algún campo obligatorio o si el formato es erróneo.");
            if(annotation != null) 
                Debug.LogError("Respuesta del servidor: " + annotation.error);
            ToolMessageHandler.Instance.ShowMessage("Error creating the annotation.");
            return;
        }
        
        annotationID = annotation.id;

        if (categoryDropdown.value.Equals("Alteration"))
        {
            List<int> idAlterations = FillAlterations();
            List<int> idAnnexData = new List<int>();
            int idMaterial = GetSelectedMaterial();
            annotationsIds.Add(annotation.id);

            StartCoroutine(AnnotationDB.PostNewAlterationEventToAnnotation(OnAlterationEventCreated, annotation.id,
                    inputDescriptionField.value, idAlterations, idAnnexData, idMaterial, inputTitleField.value));
        }
        else
        {
            CheckAndShareAnnotation(annotationID);
            OpenListView();
            RefreshAnnotationList();
            annotationID = -1;
        }
    }
    
    private void OnAlterationEventCreated(AlterationEventData alterationEvent, bool success)
    {
        if (success)
        {
            CheckAndShareAnnotation(annotationID);
            OpenListView();
            RefreshAnnotationList();
            annotationID = -1;
        }
        else
        {
            Debug.LogWarning("Error creating alteration event.");
        }
    }
    
    private void OnAnnotationsListReceived(AnnotationResponse response, bool success)
    {
        if (success && response != null && response.items != null)
        {
            cachedAnnotations = response.items;
            FilterAnnotationsList(filterCategoryDropdown?.value ?? "All");
            
            if (!viewMode && !isGuestMode)
                OpenListView();
        }
        else
        {
            Debug.LogWarning("Error al refrescar la lista de anotaciones.");
            cachedAnnotations = new List<AnnotationData>();
            FilterAnnotationsList("All");
            
            if (!viewMode && !isGuestMode)
                OpenListView();
        }
    }
    
    private void OnChElementReceived(CHElementData chElementData, bool success)
    {
        if (success)
        {
            chElementDataGlobal = chElementData;
            StartCoroutine(MaterialDB.GetMaterialList(OnMaterialListReceived, perPage: 100));
        }
        else
        {
            Debug.LogWarning("Error when obtaining CH element.");
        }
    }

    private void OnMaterialListReceived(MaterialResponse materials, bool success)
    {
        if (success)
        {
            InsertMaterialToggles(materials);
            StartCoroutine(AlterationFormDB.GetAlterationFormList(OnAlterationFormListReceived));
        }
        else
        {
            Debug.LogWarning("Error when obtaining material list.");
        }
    }
    
    private void OnAlterationFormListReceived(AlterationFormResponse data, bool success)
    {
        if (success)
        {
            CreateFoldouts(data);
        }
        else
        {
            Debug.LogWarning("Error when obtaining alteration forms list.");
        }
    }
    
    private void InsertMaterialToggles(MaterialResponse materials)
    {
        var materialsFoldoutRoot = rootElement.Q<VisualElement>(materialsTogglesContainerName);
        List<MaterialData> listMaterialsChElement = chElementDataGlobal.materials;

        togglesMaterials.Clear();
        bool hasOtherMaterials = false;
        string otherMaterialsText = "Other materials";
        int realOtherMaterialsId = GetOtherMaterialsId(materials);

        foreach (var material in materials.items)
        {
            if (ChElementHasMaterial(material.id, listMaterialsChElement))
            {
                if (material.name.Equals(otherMaterialsText, StringComparison.OrdinalIgnoreCase))
                    hasOtherMaterials = true;

                CreateAndAddToggle(material.name, material.id, materialsFoldoutRoot);
            }
        }

        if (!hasOtherMaterials && realOtherMaterialsId != -1)
        {
            CreateAndAddToggle(otherMaterialsText, realOtherMaterialsId, materialsFoldoutRoot);
        }
    }
    
    public void CreateFoldouts(AlterationFormResponse alterationForms)
    {
        var alterationFormsFoldoutRoot = rootElement.Q<VisualElement>(alterationFormsContainerName);
        togglesAlterations.Clear();
        
        foreach (var alterationForm in alterationForms.items)
        {
            var alterationFormInstance = alterationFormTemplate.CloneTree();
            var alterationFormFoldout = alterationFormInstance.Q<Foldout>(alterationFormFoldoutName);
            alterationFormFoldout.text = alterationForm.name;
            alterationFormFoldout.value = false;
            
            var alterationFormScrollView = alterationFormInstance.Q<ScrollView>(alterationFormScrollViewName);

            foreach (var alteration in alterationForm.alterations)
            {
                var alterationInstance = alterationTemplate.CloneTree();
                var alterationToggle = alterationInstance.Q<Toggle>(alterationToggleName);
                alterationToggle.name = alteration.name + "-" + alteration.id;
                alterationToggle.text = alteration.name;
                alterationToggle.AddToClassList("alteration-toggle");
                alterationToggle.AddToClassList("big-toggle");
                alterationToggle.value = false;
                
                togglesAlterations.Add(alterationToggle);
                alterationFormScrollView.Add(alterationToggle);
            }
            alterationFormsFoldoutRoot.Add(alterationFormInstance);
        }
    }

    private void CreateAndAddToggle(string toggleName, int toggleId, VisualElement container)
    {
        var materialToggleInstance = materialToggleTemplate.CloneTree();
        var materialToggle = materialToggleInstance.Q<Toggle>(materialToggleName);
    
        materialToggle.name = $"{toggleName}-{toggleId}";
        materialToggle.text = toggleName;
        materialToggle.AddToClassList("material-toggle");
        materialToggle.AddToClassList("big-toggle");
        materialToggle.value = false;

        togglesMaterials.Add(materialToggle);

        materialToggle.RegisterValueChangedCallback(evt =>
        {
            if (evt.newValue)
            {
                foreach (var t in togglesMaterials)
                {
                    if (t != materialToggle) t.value = false;
                }
            }
        });

        container.Add(materialToggleInstance);
    }
    
    private int GetOtherMaterialsId(MaterialResponse materials)
    {
        if (materials == null || materials.items == null) 
            return -1;
        
        var otherMaterial = materials.items.FirstOrDefault(m =>
            m.name.Equals("Other materials", StringComparison.OrdinalIgnoreCase));
        
        return otherMaterial != null ? otherMaterial.id : -1;
    }

    public bool ChElementHasMaterial(int id, List<MaterialData> materials)
    {
        return materials.Any(m => m.id == id);
    }
    
    public bool CheckNeccessaryData()
    {
        if (string.IsNullOrEmpty(inputTitleField.value) || inputTitleField.value.Trim().Length < 4)
        {
            ToolMessageHandler.Instance.ShowMessage("Fill the title of the annotation, please (minimum 4 characters).");
            return false;
        }

        if (string.IsNullOrEmpty(inputDescriptionField.value) || inputDescriptionField.value.Length < 4)
        {
            ToolMessageHandler.Instance.ShowMessage("Fill the description of the annotation, please (minimum 4 characters).");
            return false;
        }

        if (drawnItems.Count == 0)
        {
            ToolMessageHandler.Instance.ShowMessage("Select at least one point or line for the annotation, please.");
            return false;
        }
        
        if (categoryDropdown.value == "Alteration")
        {
            if (GetSelectedMaterial().Equals(-1))
            {
                ToolMessageHandler.Instance.ShowMessage("Select a material for the annotation, please.");
                return false;
            }

            if (FillAlterations().Count.Equals(0))
            {
                ToolMessageHandler.Instance.ShowMessage("Select one or more alterations for the annotation, please.");
                return false;
            }
        }

        return true;
    }
    
    public List<int> FillAlterations()
    {
        List<int> alterationIDs = new List<int>();

        foreach (var toggle in togglesAlterations)
        {
            if (toggle.value)
            {
                var parts = toggle.name.Split('-');
                if (parts.Length >= 2) alterationIDs.Add(int.Parse(parts[1]));
            }
        }
        return alterationIDs;
    }
    
    private void FillMultimediaReadOnly(List<AnnotationInfo> infos)
    {
        if (viewMultimediaList == null || documentChipTemplate == null) return;
        
        viewMultimediaList.Clear();

        foreach (var info in infos)
        {
            VisualElement newItem = documentChipTemplate.CloneTree();
            newItem.style.flexGrow = 0;
            newItem.style.flexShrink = 0;
            newItem.style.width = new Length(100, LengthUnit.Percent); 
            newItem.style.flexDirection = FlexDirection.Row;
            
            newItem.RegisterCallback<ClickEvent>(ev => 
            {
                OpenMultimedia(info);
            });
            
            Label label = newItem.Q<Label>("DocumentText");
            VisualElement image = newItem.Q<VisualElement>("DocumentLogo");
            Button button = newItem.Q<Button>("DocumentButton");

            if (label != null) 
                label.text = info.annotation_info_name;

            if (image != null)
            {
                bool isAudio = info.type == "Sound";
                image.style.backgroundImage = isAudio ? audioImage?.texture : imageImage?.texture;
            }

            if (button != null)
            {
                button.style.display = DisplayStyle.None; 
            }

            viewMultimediaList.Add(newItem);
        }
    }
    
    public void FillAlterationsViewMode(List<AlterationData> alterationIDs)
    {
        var ids = alterationIDs.Select(a => a.id.ToString()).ToList();
        foreach (var toggle in togglesAlterations)
        {
            toggle.SetEnabled(false);
            var parts = toggle.name.Split('-');
            string toggleId = parts.Length >= 2 ? parts[1] : "";
            bool shouldBeOn = ids.Contains(toggleId);
            toggle.SetValueWithoutNotify(shouldBeOn);
        }

        if (alterationFormsFoldoutRoot != null)
        {
            var allFoldouts = alterationFormsFoldoutRoot.Query<Foldout>(alterationFormFoldoutName).ToList();
            foreach (var foldout in allFoldouts)
            {
                var togglesInFoldout = foldout.Query<Toggle>().ToList();
                bool hasSelected = togglesInFoldout.Any(t => t.value);
                foldout.value = hasSelected;
            }
        }
    }
    
    public void FillMaterialViewMode(int materialID)
    {
        foreach (var toggle in togglesMaterials)
        {
            toggle.SetEnabled(false);
            if (toggle.name.Contains(materialID.ToString()))
                toggle.SetValueWithoutNotify(true);
        }
    }
    
    public int GetSelectedMaterial()
    {
        foreach (var toggle in togglesMaterials)
        {
            if (toggle.value)
            {
                var parts = toggle.name.Split('-');
                if (parts.Length >= 2) return int.Parse(parts[1]);
            }
        }
        return -1;
    }
    
    private void StopAllBlinks()
    {
        foreach (var item in drawnItems)
        {
            if (item is GameObject markerObj && markerObj != null)
            {
                var marker = markerObj.GetComponent<AnnotationMarker>();
                if (marker != null) marker.ForceStopBlink();
            }
            else if (item is LineReference lineRef && lineRef != null && lineRef.lineObject != null)
            {
                var lineMarker = lineRef.lineObject.GetComponent<AnnotationLineMarker>();
                if (lineMarker != null) lineMarker.ForceStopBlink();
            }
        }
    }

    public void OnClickDeleteButton()
    {
        if (currentViewedAnnotation == null) return;
        if (currentViewedAnnotation.username != GlobalManagement.Instance.username)
        {
            Debug.LogWarning("Acceso denegado: Solo el creador puede borrar esta anotación.");
            return;
        }

        StartCoroutine(DeleteAnnotationAndFiles(currentViewedAnnotation));
    }

    public void OnChangeShader(ChangeEvent<bool> evt)
    {
        if (rendererModel == null || controller == null) return;

        if (evt.newValue)
            RestoreMaterial();
        else
            RestoreSnapshotState();
    }
    
    private void RestoreSnapshotState()
    {
        if (controller == null)
            return;
        
        controller.ApplyMode(snapshotMode);
        
        if (controller.currentMat != null)
        {
            controller.currentMat.SetVector("_CutPosition", snapshotPos);
            controller.currentMat.SetVector("_CutNormal", snapshotNormal);
            controller.currentMat.SetVector("_Center", snapshotCenter);
            controller.currentMat.SetFloat("_Radius", snapshotRadius * controller.radiusMultiplier);
            controller.currentMat.SetFloat("_Reverse", snapshotReverse ? 1 : 0);
            controller.currentMat.SetFloat("_UseTexIni", snapshotUseTexIni ? 1 : 0);
        }
        
        controller.runtimeRadius = snapshotRadius;
        controller.runtimeReverse = snapshotReverse;
        controller.runtimeCutMode = snapshotCutMode;
        controller.localPoint = (snapshotMode == VisualizationMode.Spot || snapshotMode == VisualizationMode.Ring) ? snapshotCenter : snapshotPos;
        controller.localNormal = snapshotNormal;
        
        if (snapshotMode != VisualizationMode.Standard)
        {
            controller.ForceAnnotationStateLocal(snapshotMode, controller.localPoint, controller.localNormal, snapshotRadius, snapshotReverse, snapshotTex1Id, snapshotTex2Id);
        }
        else
        {
            controller.SetTexture("_AlphaLayer", null);
            if (snapshotStandardTexId != -1 && textureManager != null)
            {
                if (textureManager.IsTextureDownloaded(snapshotStandardTexId))
                {
                    var data = textureManager.GetLocalTextureData(snapshotStandardTexId);
                    if (data != null && data.localTexture2D != null)
                        controller.SetTexture("_AlphaLayer", data.localTexture2D);
                }
            }
        }
    }

    public void OnChangePOV(ChangeEvent<bool> evt)
    {
        if (evt.newValue)
        {
            EnablePlayerMovement(false);
            ApplyPointOfView();
        }
        else
        {
            EnablePlayerMovement(true);
            RestoreOriginalPosition();
        }
    }

    private void ApplyPointOfView()
    {
        if (currentViewedAnnotation == null)
            return;
            
        var data = currentViewedAnnotation;

        if (data.transformation_matrix != null && data.user_transformation_matrix != null && Camera.main != null)
        {
            GameObject localPlayer = null;
            NetworkedPlayerCharacter[] allPlayers = FindObjectsOfType<NetworkedPlayerCharacter>();
            foreach (var player in allPlayers)
            {
                if (player.IsOwner)
                {
                    localPlayer = player.gameObject;
                    break;
                }
            }

            if (localPlayer != null)
            {
                if (!hasSavedPosition)
                {
                    savedPlayerPosition = localPlayer.transform.position;
                    savedPlayerRotation = localPlayer.transform.rotation;
                    savedCameraRotation = Camera.main.transform.rotation;

                    savedModelPrePOVPos = _frozenPosition;
                    savedModelPrePOVRot = _frozenRotation;
                    savedModelPrePOVScale = _frozenScale;
                    
                    if (modelTransform != null)
                    {
                        savedChildModelPos = modelTransform.localPosition;
                        savedChildModelRot = modelTransform.localRotation;
                        savedChildModelScale = modelTransform.localScale;
                    }
                    
                    hasSavedPosition = true;
                }

                Matrix4x4 savedModelMatrix = HelpFunctionsConditionReport.StringToMatrix4X4(data.transformation_matrix);
                Matrix4x4 savedCameraMatrix = HelpFunctionsConditionReport.StringToMatrix4X4(data.user_transformation_matrix);

                modelTransform.position = savedModelMatrix.GetColumn(3);
                modelTransform.rotation = Quaternion.LookRotation(savedModelMatrix.GetColumn(2), savedModelMatrix.GetColumn(1));

                if (rootModelTransform != null)
                {
                    _frozenPosition = rootModelTransform.localPosition;
                    _frozenRotation = rootModelTransform.localRotation;
                }

                Vector3 targetCamPos = savedCameraMatrix.GetColumn(3);
                Quaternion targetCamRot = Quaternion.LookRotation(savedCameraMatrix.GetColumn(2), savedCameraMatrix.GetColumn(1));

                Vector3 cameraOffset = Camera.main.transform.position - localPlayer.transform.position;
                localPlayer.transform.position = targetCamPos - cameraOffset;

                float angleDifference = targetCamRot.eulerAngles.y - Camera.main.transform.eulerAngles.y;
                localPlayer.transform.RotateAround(Camera.main.transform.position, Vector3.up, angleDifference);
                
                Camera.main.transform.rotation = targetCamRot;
            }
        }
    }

    private void RestoreOriginalPosition()
    {
        if (hasSavedPosition)
        {
            NetworkedPlayerCharacter[] allPlayers = FindObjectsOfType<NetworkedPlayerCharacter>();
            foreach (var player in allPlayers)
            {
                if (player.IsOwner)
                {
                    player.gameObject.transform.position = savedPlayerPosition;
                    player.gameObject.transform.rotation = savedPlayerRotation;
                    
                    if (Camera.main != null)
                        Camera.main.transform.rotation = savedCameraRotation;
                    
                    _frozenPosition = savedModelPrePOVPos;
                    _frozenRotation = savedModelPrePOVRot;
                    _frozenScale = savedModelPrePOVScale;
                    
                    if (modelTransform != null)
                    {
                        modelTransform.localPosition = savedChildModelPos;
                        modelTransform.localRotation = savedChildModelRot;
                        modelTransform.localScale = savedChildModelScale;
                    }
                    
                    hasSavedPosition = false; 
                    break;
                }
            }
        }
    }

    private IEnumerator DeleteAnnotationAndFiles(AnnotationData annotation)
    {
        if (annotation.annotation_infos != null && annotation.annotation_infos.Count > 0)
        {
            bool hasFileErrors = false;
            foreach (var file in annotation.annotation_infos)
            {
                bool fileRequestFinished = false;

                yield return UploadDB.DeleteUploadFromApi((error, success) =>
                {
                    if (!success) hasFileErrors = true;
                    fileRequestFinished = true;
                }, file.file);

                yield return new WaitUntil(() => fileRequestFinished);
            }
        }

        bool annRequestFinished = false;
        bool annDeleted = false;
    
        yield return AnnotationDB.DeleteAnnotationFromApi((error, success) =>
        {
            annDeleted = success;
            
            if (!success) 
                Debug.LogError($"Error borrando anotación principal: {error?.error}");
            
            annRequestFinished = true;
        }, annotation.id);
    
        yield return new WaitUntil(() => annRequestFinished);

        if (annDeleted)
        {
            OpenListView();
            RefreshAnnotationList();
            ToolMessageHandler.Instance?.ShowMessage("Annotation deleted successfully.");
        }
        else
            ToolMessageHandler.Instance?.ShowMessage("Error while trying to delete the annotation.");
    }
    
    private void OnUploadCompleted(UploadData upload, bool success)
    {
        if (success && upload.id != null)
        {
            currentUploadID = upload.id.Value;
            StartCoroutine(UploadDB.PostNewUploadChunks(OnUploadChunksCompleted, currentUploadID, fileBinariesValue[fileCounter], 0, fileChecksumsValue[fileCounter]));
        }
        else
        {
            uploadingFile = false;
            Debug.LogWarning("Error when uploading data information.");
        }
    }

    private void OnUploadChunksCompleted(UploadData upload, bool success)
    {
        if (success)
            StartCoroutine(UploadDB.PostNewUploadComplete(OnUploadCompleteCompleted, currentUploadID));
        else
        {
            uploadingFile = false;
            Debug.LogWarning("Error when uploading chunk data information.");
        }
    }

    private void OnUploadCompleteCompleted(UploadData upload, bool success)
    {
        if (success)
            idAnnexDocs.Add(currentUploadID);
        else
            Debug.LogWarning("Error when uploading complete data information.");

        uploadingFile = false;
    }
    
    private void ShowMultimediaView(bool show)
    {
        CloseMultimedia();
        if (recordAudioContainer != null)
            recordAudioContainer.style.display = DisplayStyle.None;
        
        if (show)
        {
            formViewContainer.style.display = DisplayStyle.None;
            adMultimediaViewContainer.style.display = DisplayStyle.Flex;
        }
        else
        {
            adMultimediaViewContainer.style.display = DisplayStyle.None;
            formViewContainer.style.display = DisplayStyle.Flex;

            if (multimediaDocsList.childCount > 0)
            {
                ColorUtility.TryParseHtmlString("#A3CC70", out Color color);
                btnAddMultimediaDataForm.style.backgroundColor = color;
            }
            else
            {
                ColorUtility.TryParseHtmlString("#F9DFAA", out Color color);
                btnAddMultimediaDataForm.style.backgroundColor = color;
            }
        }
    }

    public void PickFromGallery()
    {
        CloseMultimedia();
        
        fileUploader.OpenWithGallery((paths) =>
        {
            if (paths == null)
            {
                ToolMessageHandler.Instance.ShowMessage("File not supported.");
                return;
            }

            AddMultimediaItems(paths);
        });
    }
    
    public void PickFromCameraPhoto()
    {
        CloseMultimedia();
        fileUploader.OpenCamera(true, (paths) =>
        {
            if (paths == null)
                return;
            
            AddMultimediaItems(paths);
        });
    }

    /*public void PickFromCameraVideo()
    {
        FindAnyObjectByType<LoadingScreenController>()?.ShowLoadingScreen();
        fileUploader.OpenCamera(false, (paths) =>
        {
            if (paths == null)
            {
                FindAnyObjectByType<LoadingScreenController>()?.HideLoadingScreen();
                return;
            }
            AddMultimediaItems(paths);
            FindAnyObjectByType<LoadingScreenController>()?.HideLoadingScreen();
        });
    }*/

    public void AddMultimediaItems(string[] names)
    {
        if (multimediaDocsList == null || documentChipTemplate == null) return;

        foreach (var n in names)
        {
            string ext = Path.GetExtension(n).ToLower();

            if (ext == ".heic")
            {
                ToolMessageHandler.Instance.ShowMessage("File .heic not supported. Prove with 'Select media'.");
                continue;
            }

            bool isImage = ext == ".png" || ext == ".jpg" || ext == ".jpeg";
            bool isAudio = ext == ".mp3" || ext == ".ogg" || ext == ".wav";

            if (!isImage && !isAudio)
            {
                ToolMessageHandler.Instance.ShowMessage($"Format {ext} not supported. Only images and sounds are allowed.");
                continue;
            }
            
            VisualElement newItem = documentChipTemplate.CloneTree();
            newItem.style.flexGrow = 0;
            newItem.style.flexShrink = 0;
            newItem.style.width = new Length(100, LengthUnit.Percent); 
            newItem.style.flexDirection = FlexDirection.Row;
            
            string localPath = n;
            string originalFileName = fileUploader.GetFileNameFromUrl(n);
            
            newItem.RegisterCallback<ClickEvent>(ev => 
            {
                if (ev.target is Button) 
                    return; 
                
                OpenLocalMultimedia(localPath, isImage, originalFileName);
            });
            
            Label label = newItem.Q<Label>("DocumentText");
            VisualElement image = newItem.Q<VisualElement>("DocumentLogo");
            Button button = newItem.Q<Button>("DocumentButton");

            if (label != null) label.text = fileUploader.GetFileNameFromUrl(n);

            if (image != null)
            {
                if (isImage)
                    image.style.backgroundImage = imageImage?.texture;
                else
                    image.style.backgroundImage = audioImage?.texture;
            }

            if (button != null)
            {
                var file = fileCounter;
                Action action = () => DeleteFileFromList(file);
                fileActionsValue.Add(action);
                button.clicked += action;
            }

            multimediaDocsList.Add(newItem);
            
            if (File.Exists(n))
            {
                byte[] bytes = File.ReadAllBytes(n);
                string checksum = Checksum.CalculateMD5(n);
                fileBinariesValue.Add(bytes);
                filePathsValue.Add(n);
                fileChecksumsValue.Add(checksum);
                fileCounter++;
            }
        }
    }

    public Action DeleteFileFromList(int id)
    {
        if (id < 0 || id >= multimediaDocsList.childCount) return null;

        multimediaDocsList.RemoveAt(id);
        fileBinariesValue.RemoveAt(id);
        filePathsValue.RemoveAt(id);
        fileChecksumsValue.RemoveAt(id);
        fileActionsValue.RemoveAt(id);
        fileCounter--;
        
        for (int i = 0; i < filePathsValue.Count; i++)
        {
            multimediaDocsList[i].Q<Button>("DocumentButton").clicked -= fileActionsValue[i];
            var i1 = i;
            Action action = () => DeleteFileFromList(i1);
            fileActionsValue[i] = action;
            multimediaDocsList[i].Q<Button>("DocumentButton").clicked += action;
        }
        
        return null;
    }

    private void OpenMultimedia(AnnotationInfo info)
    {
        ClearRawImage(annexDataImage, true);
        
        if (info.file < 0) 
        {
            string localPath = Path.Combine(GetUserFolderPath(), $"Offline_Media_{info.file}_{info.annotation_info_name}");
            OpenLocalMultimedia(localPath, info.type == "Image", info.annotation_info_name);
            return;
        }
        
        if (info.type == "Image")
        {
            multimediaCanvas.SetActive(true);
            annexDataImage.gameObject.SetActive(true);
            if (audioSource != null) audioSource.gameObject.SetActive(false);
            
            DownloadAndFillAnnexDataImage(annexDataImage, info.file);
        }
        else if (info.type == "Sound")
        {
            multimediaCanvas.SetActive(true);
            if (audioSource != null) audioSource.gameObject.SetActive(true);
            annexDataImage.gameObject.SetActive(false);
            
            DownloadAndPlayAnnexDataAudio(audioSource, info.annotation_info_name, info.file);
        }
        else
        {
            ToolMessageHandler.Instance.ShowMessage("Formato no soportado para previsualización.");
        }
    }
    
    private void OpenLocalMultimedia(string localPath, bool isImage, string originalName = "")
    {
        if (multimediaCanvas == null) return;
    
        ClearRawImage(annexDataImage, true);
    
        if (isImage)
        {
            multimediaCanvas.SetActive(true);
            if (annexDataImage != null) annexDataImage.gameObject.SetActive(true);
            if (audioSource != null) audioSource.gameObject.SetActive(false);
            
            StartCoroutine(LoadLocalImageCoroutine(localPath));
        }
        else
        {
            multimediaCanvas.SetActive(true);
            if (audioSource != null) audioSource.gameObject.SetActive(true);
            if (annexDataImage != null) annexDataImage.gameObject.SetActive(false);
            
            StartCoroutine(LoadLocalAudio(localPath, originalName));
        }
    }
    
    private IEnumerator LoadLocalImageCoroutine(string localPath)
    {
        yield return null; 
    
        if (File.Exists(localPath))
        {
            byte[] bytes = File.ReadAllBytes(localPath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);
            AdjustRawImageAspect(annexDataImage, (float)texture.width / texture.height);
            annexDataImage.texture = texture;
        }
    }

    private IEnumerator LoadLocalAudio(string path, string originalName = "")
    {
        string uri = "file://" + path;
        
        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.UNKNOWN))
        {
            yield return uwr.SendWebRequest();
            
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                if (audioSource != null)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);
                    clip.name = string.IsNullOrEmpty(originalName) ? Path.GetFileName(path) : originalName; 
                    
                    audioSource.clip = clip;
                    audioSource.Play();
                }
            }
            else
            {
                Debug.LogWarning("Error al cargar audio local: " + uwr.error);
                ToolMessageHandler.Instance.ShowMessage("No se pudo reproducir el archivo de audio local.");
            }
        }
    }

    private void DownloadAndFillAnnexDataImage(RawImage image, int? fileId)
    {
        StartCoroutine(AnnexDataDB.GetAnnexDataImage(fileId, FillImage(image)));
    }

    private UnityEngine.Events.UnityAction<Texture2D, bool> FillImage(RawImage imageToFill)
    {
        return (texture, success) =>
        {
            if (success && texture != null)
            {
                AdjustRawImageAspect(imageToFill, (float)texture.width / texture.height);
                imageToFill.texture = texture;
            }
            else
            {
                Debug.LogWarning("No se pudo cargar la imagen de la anotación.");
                ToolMessageHandler.Instance.ShowMessage("Error al cargar la imagen.");
            }
        };
    }

    private void DownloadAndPlayAnnexDataAudio(AudioSource source, string audioName, int? fileId)
    {
        StartCoroutine(AnnexDataDB.GetAnnexDataAudio(fileId, audioName, FillAudio(source, audioName)));
    }

    private UnityEngine.Events.UnityAction<AudioClip, bool> FillAudio(AudioSource audioSourceComp, string audioName)
    {
        return (audioClip, success) =>
        {
            if (success && audioSourceComp != null)
            {
                audioClip.name = audioName;
                audioSourceComp.clip = audioClip;
                audioSourceComp.Play();
            }
            else
            {
                Debug.LogWarning("No se pudo cargar el audio de la anotación.");
                ToolMessageHandler.Instance.ShowMessage("Error al cargar el audio.");
            }
        };
    }

    private void AdjustRawImageAspect(RawImage image, float aspectRatio)
    {
        var fitter = image.gameObject.GetComponent<AspectRatioFitter>();
        if (fitter != null)
        {
            fitter.aspectRatio = aspectRatio;
        }
    }

    public void CloseMultimedia()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
        
        if (audioSource != null) audioSource.gameObject.SetActive(false);
        
        ClearRawImage(annexDataImage, true);
        if (annexDataImage != null) annexDataImage.gameObject.SetActive(false);
        
        if (multimediaCanvas != null) multimediaCanvas.SetActive(false);
    }

    public void ClearRawImage(RawImage image, bool destroyTexture = false)
    {
        if (image == null) return;

        if (destroyTexture && image.texture != null)
        {
            if (image.texture is RenderTexture rt)
            {
                rt.Release();
                DestroyImmediate(rt);
            }
            else
            {
                DestroyImmediate(image.texture);
            }
        }
        image.texture = null;
    }
    
    // AUDIO
    public void ShowRecordAudioView(bool show)
    {
        StartCoroutine(RequestPermissionAndStartRecording(show));
        elapsedTimeRecording = 0f;
        progressBarRecord.value = 0f;
        progressBarRecord.title = string.Format("00:00");
        
        if (show && btnRecordAudio != null)
            btnRecordAudio.style.unityBackgroundImageTintColor = originalRecordBtnColor;
    }
    
    private IEnumerator TakeScreenshotRoutine()
    {
        if (rootElement != null)
            rootElement.style.display = DisplayStyle.None;

        int originalCullingMask = 0;
        if (Camera.main != null)
        {
            originalCullingMask = Camera.main.cullingMask;
            Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));
        }
        
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        
        if (Camera.main != null)
            Camera.main.cullingMask = originalCullingMask;

        if (rootElement != null)
            rootElement.style.display = DisplayStyle.Flex;

        byte[] bytes = screenshot.EncodeToJPG(85);
        string fileName = $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
        string filePath = Path.Combine(Application.temporaryCachePath, fileName);
        
        File.WriteAllBytes(filePath, bytes);
        
        Destroy(screenshot); 
        AddMultimediaItems(new string[] { filePath });
        
        if (ToolMessageHandler.Instance != null)
            ToolMessageHandler.Instance.ShowMessage("Screenshot captured successfully!");
    }
    
    private IEnumerator RequestPermissionAndStartRecording(bool show)
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
            yield return new WaitForSeconds(0.5f);
            float timer = 0;
            while (!Permission.HasUserAuthorizedPermission(Permission.Microphone) && timer < 5f)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone)) yield return null;
        }
#endif
#if UNITY_IOS
        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            if (!Application.HasUserAuthorization(UserAuthorization.Microphone)) yield return null;
        }
#endif
        if (Microphone.devices.Length != 0)
        {
            CloseMultimedia();
            if (show)
            {
                if (adMultimediaViewContainer != null) adMultimediaViewContainer.style.display = DisplayStyle.None;
                if (recordAudioContainer != null) recordAudioContainer.style.display = DisplayStyle.Flex;
            }
            else
            {
                if (recordAudioContainer != null) recordAudioContainer.style.display = DisplayStyle.None;
                if (adMultimediaViewContainer != null) adMultimediaViewContainer.style.display = DisplayStyle.Flex;
            }
        }
        else
            ToolMessageHandler.Instance?.ShowMessage("No microphone detected. Check Quest app permissions.", 4f, MessageType.Error);
        
        yield return null;
    }
    
    private void StartRecordingAudio()
    {
        if (Microphone.devices.Length == 0) return;

        microphoneDevice = Microphone.devices[0];
        isRecording = true;
        recordedAudioClip = Microphone.Start(microphoneDevice, false, (int)MAX_RECORDING_TIME, 44100);

        if (recordingTimerCoroutine != null) StopCoroutine(recordingTimerCoroutine);
        recordingTimerCoroutine = StartCoroutine(RecordingTimeoutRoutine());
    }

    private IEnumerator RecordingTimeoutRoutine()
    {
        elapsedTimeRecording = 0f;
        progressBarRecord.value = 0f;
        float startTime = Time.time;
        
        while (elapsedTimeRecording < MAX_RECORDING_TIME && isRecording)
        {
            elapsedTimeRecording = Time.time - startTime;
            progressBarRecord.value = elapsedTimeRecording / MAX_RECORDING_TIME;
            TimeSpan time = TimeSpan.FromSeconds(elapsedTimeRecording);
            progressBarRecord.title = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
            progressBarRecord.MarkDirtyRepaint();
            yield return null;
        }

        if (isRecording)
        {
            progressBarRecord.value = 1f;
            StopRecordingAudioAndReturn(isTimeLimitReached: true);
        }
    }

    private void StopRecordingAudioAndReturn(bool isTimeLimitReached = false)
    {
        if (!isRecording) return;
        
        if (recordingTimerCoroutine != null)
        {
            StopCoroutine(recordingTimerCoroutine);
            recordingTimerCoroutine = null;
        }
        
        if (btnRecordAudio != null) btnRecordAudio.style.unityBackgroundImageTintColor = originalRecordBtnColor;
        
        int lastPosition = Microphone.GetPosition(microphoneDevice);
        if (isTimeLimitReached) lastPosition = recordedAudioClip.samples;
        
        Microphone.End(microphoneDevice);
        isRecording = false;
        progressBarRecord.value = 0f;

        if (lastPosition <= 0 || recordedAudioClip == null || elapsedTimeRecording <= 0.5f)
        {
            ShowRecordAudioView(false);
            elapsedTimeRecording = 0;
            return;
        }

        AudioClip trimmedClip = TrimAudioClip(recordedAudioClip, lastPosition);
        string fileName = $"Recorded_Audio_{DateTime.Now:yyyyMMdd_HHmmss}.wav";
        string filePath = Path.Combine(Application.temporaryCachePath, fileName);

        if (SaveAudioClipToWav(trimmedClip, filePath))
            AddMultimediaItems(new string[] { filePath });
        
        elapsedTimeRecording = 0;
        ShowRecordAudioView(false);
    }

    private AudioClip TrimAudioClip(AudioClip clip, int samples)
    {
        float[] data = new float[samples * clip.channels];
        clip.GetData(data, 0);
        AudioClip trimmed = AudioClip.Create(clip.name, samples, clip.channels, clip.frequency, false);
        trimmed.SetData(data, 0);
        return trimmed;
    }

    private bool SaveAudioClipToWav(AudioClip clip, string filePath)
    {
        try
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            using (var writer = new BinaryWriter(fileStream))
            {
                var samples = new float[clip.samples * clip.channels];
                clip.GetData(samples, 0);
                short channels = (short)clip.channels;
                int sampleRate = clip.frequency;
                ushort bitsPerSample = 16;

                writer.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"));
                writer.Write(36 + samples.Length * 2);
                writer.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"));
                writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt "));
                writer.Write(16);
                writer.Write((short)1); 
                writer.Write(channels);
                writer.Write(sampleRate);
                writer.Write(sampleRate * channels * (bitsPerSample / 8));
                writer.Write((short)(channels * (bitsPerSample / 8)));
                writer.Write(bitsPerSample);
                writer.Write(System.Text.Encoding.UTF8.GetBytes("data"));
                writer.Write(samples.Length * 2);

                foreach (var sample in samples)
                {
                    short value = (short)(Mathf.Clamp(sample, -1.0f, 1.0f) * 32767);
                    writer.Write(value);
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error guardando el archivo WAV: {ex.Message}");
            return false;
        }
    }
    
    public void ProcessPendingInvite(ulong hostId, int annotationId)
    {
        isGuestMode = true;

        if (annotationId != -1)
        {
            var ann = cachedAnnotations.FirstOrDefault(a => a.id == annotationId);
            
            if (ann != null) 
            {
                OpenAnnotationWithCheck(ann);
            }
            else 
            {
                ToolMessageHandler.Instance?.ShowMessage("Fetching shared annotation...", 2f, MessageType.Info);
                
                StartCoroutine(ThreeDInstanceDB.GetAnnotationListFromThreeDInstance((response, success) => 
                {
                    if (success && response != null && response.items != null)
                    {
                        cachedAnnotations = response.items;
                        FilterAnnotationsList(filterCategoryDropdown?.value ?? "All");
                        
                        var fetchedAnn = cachedAnnotations.FirstOrDefault(a => a.id == annotationId);
                        if (fetchedAnn != null)
                        {
                            OpenAnnotationWithCheck(fetchedAnn);
                        }
                        else
                        {
                            ToolMessageHandler.Instance?.ShowMessage("Error: The shared annotation could not be found.", 3f, MessageType.Error);
                        }
                    }
                    else
                    {
                        ToolMessageHandler.Instance?.ShowMessage("Error downloading annotation data.", 3f, MessageType.Error);
                    }
                }, inspectedObjectController.GetE3DInstanceID(), perPage: 1000));
            }
        }
        else
        {
            OpenAddMode();
        }
    }

    private void OpenAnnotationWithCheck(AnnotationData item)
    {
        if (!string.IsNullOrEmpty(item.category) && item.category.ToLower() == "damage")
        {
            StartCoroutine(AnnotationDB.GetAlterationEventListFromAnnotation((response, success) => 
            {
                if (success && response != null && response.items != null && response.items.Count > 0)
                    OpenViewMode(item, response.items[0]);
                else
                    OpenViewMode(item, null); 
            }, item.id));
        }
        else
        {
            OpenViewMode(item, null);
        }
    }
    
    private void EnablePlayerMovement(bool enable)
    {
        GameObject localPlayer = null;

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient && NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
        }
        else
        {
            var fpcFallback = FindAnyObjectByType<FirstPersonController>();
            if (fpcFallback != null) localPlayer = fpcFallback.gameObject;
        }

        if (localPlayer != null)
        {
            var fpc = localPlayer.GetComponentInChildren<FirstPersonController>();
            if (fpc != null) fpc.enabled = enable;

            var moveProviders = localPlayer.GetComponentsInChildren<LocomotionProvider>();
            foreach (var provider in moveProviders)
            {
                provider.enabled = enable;
            }
        }
    }

    private void SyncGuestForm(string title, string desc, string category)
    {
        if (!isGuestMode) return;

        inputTitleField.SetValueWithoutNotify(title);
        inputDescriptionField.SetValueWithoutNotify(desc);
        categoryDropdown.SetValueWithoutNotify(category);
        UpdateCategoryFieldsVisibility(category);
    }
}