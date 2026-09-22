using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Netcode;

public class VisualizationModeManager : ToolComponent
{
    [Header("UI")]
    [SerializeField] private VisualTreeAsset labelItemAsset;
    [SerializeField] private VisualTreeAsset sliderItemAsset;
    [SerializeField] private VisualTreeAsset toggleItemAsset;
    [SerializeField] private VisualTreeAsset dropdownItemAsset;
    [SerializeField] private VisualTreeAsset buttonItemAsset;
    [SerializeField] private VisualTreeAsset imageItemAsset;

    private int dropdownCount;
    private string texTop;
    private string texBottom;

    [Header("Shader")]
    [SerializeField] private VisualizationMode visualizationMode;

    private UIDocument uiDocument;
    private TextureManager textureManager;
    private VisualizationModeController controller;

    private VisualElement root;
    private VisualElement container;
    private VisualElement auxContainer;
    private VisualElement hiddenContainer;
    private bool containerHidden = false;
    private Button hideButton;
    private VisualElement containerWindow;
    private VisualElement auxContainerWindow;

    private int initialVisibleTextureId = -1;
    public int mainTextureId = -1;
    public int secondaryTextureId = -1;
    private List<RemoteTexture> dropdownTextures = new();
    private List<DropdownField> textureDropdowns = new();
    private Button addLayerButton;
    private List<LayerUI> layerRows = new();

    private Renderer rendererModel;
    private Texture2D modelAlbedoTexture;
    private Texture2D modelNormalTexture;
    private Texture2D modelOcclusionTexture;
    private int MAX_DOWNLOADED_TEXTURES;
    private HashSet<int> runtimeDownloadedTextures = new();

    private bool reverse;
    private CutMode axis;

    private int idLayer = 1;

    private Dictionary<int, int> dropdownRequestVersion = new();

    private bool closedByBackButton = false;

    private DropdownField layerDropdownAux;
    
    [Header("Timeout Settings")]
    [SerializeField] private float inactivityTimeoutSeconds = 15f;
    private float _lastInteractionTime;
    private bool _isTrackingInactivity = false;

    protected override void OnToolActivatedInternal()
    {
        base.OnToolActivatedInternal();
        textureManager = FindFirstObjectByType<TextureManager>();
        controller = FindFirstObjectByType<VisualizationModeController>();
        
        if (controller != null)
            controller.RemoveAllPreviewCameras();
        
        bool wasAlreadyActive = false;
        VisualizationMode intendedMode = visualizationMode;
        
        if (controller != null && NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
        {
            controller.visToolLockOwner.OnValueChanged += OnVisLockChanged;
            if (textureManager != null)
                textureManager.textureToolLockOwner.OnValueChanged += OnTexLockChanged;
            
            ulong currentOwner = controller.visToolLockOwner.Value;
            ulong texOwner = textureManager != null ? textureManager.textureToolLockOwner.Value : ulong.MaxValue;
            ulong myId = NetworkManager.Singleton.LocalClientId;

            if ((currentOwner != ulong.MaxValue && currentOwner != myId) || 
                (texOwner != ulong.MaxValue && texOwner != myId))
            {
                ulong blockingOwner = (currentOwner != ulong.MaxValue && currentOwner != myId) ? currentOwner : texOwner;
                string ownerName = LocalRegistry.Instance != null ? LocalRegistry.Instance.GetClientUsername(blockingOwner) : "Another user";
                
                if (ToolMessageHandler.Instance != null)
                    ToolMessageHandler.Instance.ShowMessage($"{ownerName} is using Textures or Visualization modes tool.", 4f, MessageType.Error);

                if (ToolMenuController.Instance != null)
                    ToolMenuController.Instance.ForceCloseActiveGroupTool();
                
                return;
            }

            controller.RequestVisToolLockServerRpc(myId);
            _isTrackingInactivity = true;
            ResetActivityTimer();
            
            if (controller.netVisualizationMode.Value != VisualizationMode.Standard)
            {
                if (controller.netVisualizationMode.Value == intendedMode)
                    wasAlreadyActive = true; 
                else
                    wasAlreadyActive = false;
            }
        }
        
        InitializeUI();
        
        if (root != null)
        {
            root.RegisterCallback<PointerMoveEvent>(evt => ResetActivityTimer());
            root.RegisterCallback<PointerDownEvent>(evt => ResetActivityTimer());
            root.RegisterCallback<WheelEvent>(evt => ResetActivityTimer());
        }

        MAX_DOWNLOADED_TEXTURES = textureManager.GetMaxDownloadedTextures();
        controller.maxCameras = MAX_DOWNLOADED_TEXTURES - 1;

        InitialDownloadedTexture();
        RefreshDropdownTextures();
        controller.IsVisualizationModeManager(true);
        CreateTextLabel();

        if (visualizationMode == VisualizationMode.Split || visualizationMode == VisualizationMode.SectionPlane)
            CreateAxisDropdown();
        if (visualizationMode == VisualizationMode.Spot || visualizationMode == VisualizationMode.Ring)
            CreateRadiusSlider();
        if (visualizationMode != VisualizationMode.Ring)
            CreateReverseToggle();

        CreateTextureDropdowns();

        if (visualizationMode == VisualizationMode.Ring)
        {
            CreateLayerButton();
            CreateHideButton();
        }

        controller.ApplyMode(visualizationMode);
        
        if (!wasAlreadyActive)
            controller.SetInitialCut();
        else
            controller.RestoreCutStateFromNetwork();
    }

    protected override void OnToolDeactivatedInternal()
    {
        base.OnToolDeactivatedInternal();
        if (controller != null)
            controller.visToolLockOwner.OnValueChanged -= OnVisLockChanged;
        if (textureManager != null)
            textureManager.textureToolLockOwner.OnValueChanged -= OnTexLockChanged;
        _isTrackingInactivity = false;
        bool holdLock = controller != null && controller.HasToolLock();

        if (holdLock)
        {
            if (textureManager != null)
                VisibleTexture();
            
            if (!closedByBackButton)
                RestoreOriginalMaterial();

            if (controller != null)
            {
                controller.IsVisualizationModeManager(false);
                
                if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
                    controller.ReleaseVisToolLockServerRpc(NetworkManager.Singleton.LocalClientId);
            }
        }

        closedByBackButton = false;

        if (containerWindow != null)
        {
            containerWindow.style.display = DisplayStyle.None;
            containerWindow.pickingMode = PickingMode.Ignore;
        }

        if (auxContainerWindow != null)
        {
            auxContainerWindow.style.display = DisplayStyle.None;
            auxContainerWindow.pickingMode = PickingMode.Ignore;
        }

        if (hiddenContainer != null)
        {
            hiddenContainer.style.display = DisplayStyle.None;
            hiddenContainer.pickingMode = PickingMode.Ignore;
            hiddenContainer.style.backgroundColor = new StyleColor(Color.clear);
        }
    }
    
    private void Update()
    {
        if (_isTrackingInactivity)
        {
            if (controller != null && (controller.isClicked() || controller.IsInteracting()))
                ResetActivityTimer();

            if (Time.time - _lastInteractionTime > inactivityTimeoutSeconds)
                ExitToolDueToTimeout();
        }
    }

    public void ResetActivityTimer()
    {
        _lastInteractionTime = Time.time;
    }

    private void ExitToolDueToTimeout()
    {
        if (ToolMessageHandler.Instance != null)
            ToolMessageHandler.Instance.ShowMessage("Visualization tool closed due to inactivity.", 3f, MessageType.Info);
                
        _isTrackingInactivity = false;
        CloseFromBackButton();

        if (ToolMenuController.Instance != null)
            ToolMenuController.Instance.GoBackToPreviousMenu();
        else
            gameObject.SetActive(false);
    }

    private void InitializeUI()
    {
        uiDocument = transform.parent.GetComponent<UIDocument>();

        root = uiDocument.rootVisualElement;

        container = root.Q<VisualElement>("visualization-container");
        auxContainer = root.Q<VisualElement>("aux-visualization-container");

        containerWindow = root.Q<VisualElement>("tool-window");
        containerWindow.style.display = DisplayStyle.Flex;
        containerWindow.pickingMode = PickingMode.Position;

        auxContainerWindow = root.Q<VisualElement>("aux-window");
        hiddenContainer = root.Q<VisualElement>("hidden-window");
    }

    /// <summary>
    /// Obtener las texturas descargadas visibles e invisibles
    /// </summary>
    private void InitialDownloadedTexture()
    {
        if (controller.modelRenderer.sharedMaterial.HasProperty("_FirstTex"))
            foreach (var tex in textureManager.GetAllRemoteTextures())
                if (controller.currentMat.GetTexture("_FirstTex") as Texture2D == controller.currentMat.GetTexture("_BaseMap") as Texture2D && tex.isVisible)
                {
                    textureManager.ToggleTextureVisibility(tex.id);
                    break;
                }

        foreach (var tex in textureManager.GetAllRemoteTextures())
            if (textureManager.IsTextureDownloaded(tex.id) && tex.isVisible)
            {
                initialVisibleTextureId = tex.id;
                mainTextureId = tex.id;
            }
            else if (textureManager.IsTextureDownloaded(tex.id) && !tex.isVisible)
                secondaryTextureId = tex.id;
    }

    /// <summary>
    /// Obtener texturas básicas 
    /// </summary>
    private void RefreshDropdownTextures()
    {
        dropdownTextures.Clear();

        foreach (var texture in textureManager.GetAllRemoteTextures())
            dropdownTextures.Add(texture);

        rendererModel = textureManager.GetModelRenderer();

        if (rendererModel != null && rendererModel.sharedMaterial != null)
        {
            modelAlbedoTexture = rendererModel.sharedMaterial.GetTexture("_BaseMap") as Texture2D;
            modelNormalTexture = rendererModel.sharedMaterial.GetTexture("_BumpMap") as Texture2D;
            modelOcclusionTexture = rendererModel.sharedMaterial.GetTexture("_OcclusionMap") as Texture2D;

            if (modelAlbedoTexture != null)
                controller.SetTexture("_BaseMap", modelAlbedoTexture);

            if (modelNormalTexture != null)
                controller.SetTexture("_BumpMap", modelNormalTexture);

            if (modelOcclusionTexture != null)
                controller.SetTexture("_OcclusionMap", modelOcclusionTexture);
        }
    }

    /// <summary>
    /// Crear label con un texto
    /// </summary>
    private void CreateTextLabel()
    {
        var element = labelItemAsset.CloneTree();

        Label label = element.Q<Label>("texture-label");

        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.unityTextAlign = TextAnchor.UpperLeft;

        label.text = visualizationMode switch
        {
            VisualizationMode.Split => "Shows different textures side by side to compare surface details.",
            VisualizationMode.Spot => "Displays a circular area with a different texture to analyze a specific region of the model.",
            VisualizationMode.Ring => "Highlight an area of the model to compare that area with other textures layers.",
            VisualizationMode.SectionPlane => "Cuts through the model to reveal internal structure and textures.",
            _ => visualizationMode.ToString(),
        };

        container.Add(element);
    }

    /// <summary>
    /// Crear dropdown para hacer cortes en los diferentes ejes
    /// </summary>
    private void CreateAxisDropdown()
    {
        var element = dropdownItemAsset.CloneTree();
        DropdownField dropdown = element.Q<DropdownField>("texture-dropdown");
        dropdown.label = "Cut style";
        dropdown.choices = new List<string>{"Horizontal", "Vertical", "Free"};
        axis = controller.runtimeCutMode;

        switch (axis)
        {
            case CutMode.EjeX:
                dropdown.SetValueWithoutNotify("Horizontal");
                break;
            case CutMode.EjeY:
                dropdown.SetValueWithoutNotify("Vertical");
                break;
            case CutMode.Free:
                dropdown.SetValueWithoutNotify("Free");
                break;
        }
        
        dropdown.RegisterValueChangedCallback(evt =>
        {
            switch (evt.newValue)
            {
                case "Horizontal":
                    axis = CutMode.EjeX;
                    break;
                case "Vertical":
                    axis = CutMode.EjeY;
                    break;
                case "Free":
                    axis = CutMode.Free;
                    break;
            }

            controller.SetCutMode(axis);
            UpdateDropdownLabels();
        });

        container.Add(dropdown);
    }

    /// <summary>
    /// Crear toggle para hacer reverse en las texturas
    /// </summary>
    private void CreateReverseToggle()
    {
        var element = toggleItemAsset.CloneTree();
        Toggle toggle = element.Q<Toggle>("texture-toggle");
        toggle.label = "Reverse";
        reverse = controller.runtimeReverse;
        toggle.SetValueWithoutNotify(reverse);

        toggle.RegisterValueChangedCallback(evt =>
        {
            controller.SetReverse(evt.newValue);
            reverse = evt.newValue;
        });

        container.Add(toggle);
    }

    /// <summary>
    /// Crear slider para modificar radio del circulo o anillo
    /// </summary>
    private void CreateRadiusSlider()
    {
        if (sliderItemAsset == null)
            return;
                
        var element = sliderItemAsset.CloneTree();
        Slider slider = element.Q<Slider>("texture-slider");
        
        if (slider == null || container == null || controller == null)
            return;
        
        slider.label = "Radius";
        slider.lowValue = 0.1f;
        slider.highValue = 3f;
        float startValue = controller.runtimeRadius > 0 ? controller.runtimeRadius : 1.5f;
        slider.SetValueWithoutNotify(startValue);

        slider.RegisterValueChangedCallback(evt =>
        {
            controller.SetRadius(evt.newValue);
        });

        container.Add(slider);
    }

    /// <summary>
    /// Crear dropdown con las texturas siendo la primer que aparece las descargas o la siguiente a la de arriba
    /// </summary>
    private void CreateTextureDropdowns()
    {
        switch (visualizationMode)
        {
            case VisualizationMode.Split:
                dropdownCount = 2;
                texTop = "Top texture";
                texBottom = "Bottom texture";
                break;
            case VisualizationMode.Spot:
                dropdownCount = 2;
                texTop = "Main texture";
                texBottom = "Spot texture";
                break;
            case VisualizationMode.Ring:
                dropdownCount = 1;
                texTop = "Main texture";
                break;
            case VisualizationMode.SectionPlane:
                dropdownCount = 1;
                texTop = "Main texture";
                break;
        }

        for (int i = 0; i < dropdownCount; i++)
        {
            int textureIndex = i;

            var element = dropdownItemAsset.CloneTree();

            DropdownField dropdown = element.Q<DropdownField>("texture-dropdown");

            if (i == 0)
                dropdown.label = texTop;
            else
                dropdown.label = texBottom;

            List<string> choices = new();

            foreach (var tex in dropdownTextures)
                choices.Add(tex.name.ToString());

            dropdown.choices = choices;

            if (i == 0 && initialVisibleTextureId != -1)
            {
                RemoteTexture firstTex = dropdownTextures.Find
                (
                    t => t.id == initialVisibleTextureId
                );

                dropdown.value = firstTex.name.ToString();

                HandleTextureSelection(firstTex, textureIndex, true);
            }
            else if (i != 0 && initialVisibleTextureId != -1)
            {
                if (secondaryTextureId != -1)
                {
                    RemoteTexture secondTex = dropdownTextures.Find
                    (
                        t => t.id == secondaryTextureId
                    );

                    dropdown.value = secondTex.name.ToString();

                    HandleTextureSelection(secondTex, textureIndex, true);
                }
                else
                {
                    int next = -1;

                    for (int j = 0; j < dropdownTextures.Count; j++)
                        if (dropdownTextures[j].id == initialVisibleTextureId)
                        {
                            if (j + 1 != dropdownTextures.Count)
                                next = dropdownTextures[j + 1].id;
                            else
                                next = dropdownTextures[0].id;

                            break;
                        }

                    RemoteTexture secondTex = dropdownTextures.Find
                    (
                        t => t.id == next
                    );

                    dropdown.value = secondTex.name.ToString();

                    secondaryTextureId = secondTex.id;

                    HandleTextureSelection(secondTex, textureIndex, true);
                }
            }
            else if (i == 0 && initialVisibleTextureId == -1)
            {
                if (choices.Count > 0)
                {
                    dropdown.value = choices[0];
                    int texIndex = dropdownTextures.FindIndex(t => t.name.ToString() == choices[0]);
                    if (texIndex >= 0)
                        mainTextureId = dropdownTextures[texIndex].id;
                }

                ApplyTexture(textureIndex, modelAlbedoTexture);
            }
            else if (i != 0 && initialVisibleTextureId == -1)
            {
                if (secondaryTextureId != -1)
                {
                    RemoteTexture secondTex = dropdownTextures.Find(t => t.id == secondaryTextureId);
                    if (secondTex.id != 0)
                    {
                        dropdown.value = secondTex.name.ToString();
                        HandleTextureSelection(secondTex, textureIndex, true);
                    }
                }
                else
                {
                    if (choices.Count > 1)
                    {
                        RemoteTexture secondTex = dropdownTextures.Find(t => t.name.ToString() == choices[1]);
                        dropdown.value = secondTex.name.ToString();
                        secondaryTextureId = secondTex.id;
                        HandleTextureSelection(secondTex, textureIndex, true);
                    }
                    else if (choices.Count > 0)
                    {
                        RemoteTexture secondTex = dropdownTextures.Find(t => t.name.ToString() == choices[0]);
                        dropdown.value = secondTex.name.ToString();
                        secondaryTextureId = secondTex.id;
                        HandleTextureSelection(secondTex, textureIndex, true);
                    }
                }
            }

            UpdateFinalTextures();

            int separateTextures = i;

            dropdown.RegisterValueChangedCallback(evt =>
            {
                RemoteTexture selected = dropdownTextures.Find
                (
                    t => t.name.ToString() == evt.newValue
                );
                
                if (selected.id == 0)
                    return;

                if (separateTextures == 0)
                    mainTextureId = selected.id;
                else
                    secondaryTextureId = selected.id;

                HandleTextureSelection(selected, textureIndex, true);

                UpdateFinalTextures();
            });

            textureDropdowns.Add(dropdown);
            container.Add(dropdown);
        }
    }

    /// <summary>
    /// Modificar las texturas finales
    /// </summary>
    private void UpdateFinalTextures()
    {
        if (controller.finalTextures.Count == 0)
        {
            controller.finalTextures.Add(mainTextureId);
            controller.finalTextures.Add(secondaryTextureId);
        }
        else
        {
            if (mainTextureId > 0)
                controller.finalTextures[0] = mainTextureId; 
            
            int nextIndex = (controller.finalTextures.Count > 1) ? 1 : 0;
            
            if (secondaryTextureId > 0)
                controller.finalTextures[nextIndex] = secondaryTextureId;
        }

        if (controller != null && NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
            controller.SetNetworkTexturesServerRpc(mainTextureId, secondaryTextureId);
    }

    /// <summary>
    /// Modificar labels de las texturas
    /// </summary>
    private void UpdateDropdownLabels()
    {
        foreach (var dropdown in textureDropdowns)
            if (visualizationMode != VisualizationMode.SectionPlane)
                if (axis == CutMode.EjeY)
                    dropdown.label = dropdown.label.Contains("Top") || dropdown.label.Contains("1") ? "Left texture" : "Right texture";
                else if (axis == CutMode.Free)
                    dropdown.label = dropdown.label.Contains("Left") || dropdown.label.Contains("Top") ? "Texture 1" : "Texture 2";
                else
                    dropdown.label = dropdown.label.Contains("1") || dropdown.label.Contains("Left") ? "Top texture" : "Bottom texture";
    }

    /// <summary>
    /// Desactivar y activar dropdown de las texturas
    /// </summary>
    private void SetDropdownsEnabled(bool enabled)
    {
        foreach (var dropdown in textureDropdowns)
            dropdown?.SetEnabled(enabled);

        addLayerButton?.SetEnabled(enabled);
        layerDropdownAux?.SetEnabled(enabled);
    }

    /// <summary>
    /// Aplicar textura dependiendo la propiedad del shader
    /// </summary>
    private void ApplyTexture(int index, Texture2D texture)
    {
        string property = index switch
        {
            0 => "_FirstTex",
            1 => "_SecondTex",
            2 => "_ThirdTex",
            3 => "_FourthTex",
            _ => "_FirstTex",
        };
        controller.SetTexture(property, texture);
    }

    /// <summary>
    /// Manejar si hay que descargar texturas
    /// </summary>
    private void HandleTextureSelection(RemoteTexture selected, int index, bool msg)
    {
        if (selected.id == 0) return;

        if (!dropdownRequestVersion.ContainsKey(index))
            dropdownRequestVersion[index] = 0;

        dropdownRequestVersion[index]++;

        int requestVersion = dropdownRequestVersion[index];
        VisualizationMode visualizationModeVersion = visualizationMode;

        // Si no está descargada
        if (!textureManager.IsTextureDownloaded(selected.id))
        {
            EnsureMemoryLimit(selected.id);

            SetDropdownsEnabled(false);

            ToolMessageHandler.Instance.ShowMessage($"Downloading {selected.name} texture...");

            textureManager.StartDownload(selected.id, () =>
            {
                SetDropdownsEnabled(true);

                if (dropdownRequestVersion[index] != requestVersion || visualizationModeVersion != visualizationMode)
                {
                    if (textureManager.IsTextureDownloaded(selected.id))
                    {
                        textureManager.RemoveTexture(selected.id);

                        runtimeDownloadedTextures.Remove(selected.id);
                    }

                    return;
                }
                
                runtimeDownloadedTextures.Add(selected.id);
                ApplyDownloadedTexture(selected, index, msg);
            });
        }
        else
        {
            if (dropdownRequestVersion[index] != requestVersion) return;

            SetDropdownsEnabled(true);
            ApplyDownloadedTexture(selected, index, msg);
        }
    }

    /// <summary>
    /// Comprobar texturas descargadas
    /// </summary>
    private void EnsureMemoryLimit(int newTextureId)
    {
        while (GetDownloadedCount() >= MAX_DOWNLOADED_TEXTURES)
        {
            foreach (var tex in textureManager.GetAllRemoteTextures())
            {
                if (tex.id == newTextureId) continue;
                if (!textureManager.IsTextureDownloaded(tex.id)) continue;

                textureManager.RemoveTexture(tex.id);
                runtimeDownloadedTextures.Remove(tex.id);

                break;
            }
        }
    }

    /// <summary>
    /// Aplicar propiedades al descargar texturas
    /// </summary>
    private void ApplyDownloadedTexture(RemoteTexture selected, int index, bool msg)
    {
        LocalTextureData data = textureManager.GetLocalTextureData(selected.id);
        if (data == null || data.localTexture2D == null) return;
        ApplyTexture(index, data.localTexture2D);

        if (msg && ToolMessageHandler.Instance != null)
            ToolMessageHandler.Instance.ShowMessage($"Applied: {selected.name}");
    }

    private int GetDownloadedCount()
    {
        int count = 0;

        foreach (var tex in textureManager.GetAllRemoteTextures())
            if (textureManager.IsTextureDownloaded(tex.id))
                count++;

        return count;
    }

    /// <summary>
    /// Crear botón para añadir layers
    /// </summary>
    private void CreateLayerButton()
    {
        var element = buttonItemAsset.CloneTree();

        Button button = element.Q<Button>("texture-button");

        button.text = $"Add Layer";

        auxContainerWindow.style.display = DisplayStyle.None;
        auxContainerWindow.pickingMode = PickingMode.Ignore;

        button.clicked += () =>
        {
            if (idLayer < MAX_DOWNLOADED_TEXTURES)
            {
                auxContainerWindow.style.display = DisplayStyle.Flex;
                auxContainerWindow.pickingMode = PickingMode.Position;

                RenderTexture rt = controller.InitializePreviewCamera(idLayer);
                
                CreateRingLayerDropdown(idLayer, rt);
                
                idLayer++;
            }
            else if (ToolMessageHandler.Instance != null)
                ToolMessageHandler.Instance.ShowMessage($"Maximum number of preview cameras reached, {MAX_DOWNLOADED_TEXTURES - 1} max.", messageType:MessageType.Error);
        };

        addLayerButton = button;
        container.Add(button);
    }

    /// <summary>
    /// Crear botón para ocultar contenedor
    /// </summary>
    private void CreateHideButton()
    {
        var element = buttonItemAsset.CloneTree();

        hideButton = element.Q<Button>("texture-button");

        hideButton.text = "";
        Texture2D icon = Resources.Load<Texture2D>("Images/menos");
        hideButton.style.backgroundImage = new StyleBackground(icon);
        hideButton.style.marginTop = 10;
        hideButton.style.marginBottom = 10;
        hideButton.style.marginLeft = 10;
        hideButton.style.marginRight = 10;
        hideButton.style.borderTopWidth = 0;
        hideButton.style.borderBottomWidth = 0;
        hideButton.style.borderLeftWidth = 0;
        hideButton.style.borderRightWidth = 0;
        hideButton.style.width = Length.Percent(80);
        hideButton.style.height = Length.Percent(80);

        hideButton.clicked += ToggleContainer;

        hiddenContainer.style.left = Length.Percent(25);
        ColorUtility.TryParseHtmlString("#BDBDBD", out Color color);
        hiddenContainer.style.backgroundColor = color;

        hiddenContainer.Add(hideButton);
    }

    /// <summary>
    /// Modificar botón de ocultar
    /// </summary>
    private void ToggleContainer()
    {
        containerHidden = !containerHidden;

        if (containerHidden)
        {
            containerWindow.style.display = DisplayStyle.None;
            containerWindow.pickingMode = PickingMode.Ignore;

            hiddenContainer.style.left = 0;

            Texture2D icon = Resources.Load<Texture2D>("Images/mas");
            hideButton.style.backgroundImage = new StyleBackground(icon);
        }
        else
        {
            containerWindow.style.display = DisplayStyle.Flex;
            containerWindow.pickingMode = PickingMode.Position;

            hiddenContainer.style.left = Length.Percent(25);

            Texture2D icon = Resources.Load<Texture2D>("Images/menos");
            hideButton.style.backgroundImage = new StyleBackground(icon);
        }
    }

    /// <summary>
    /// Crear dropdown de texturas para los layers siguiendo la logica del otro dropdown de texturas
    /// </summary>
    private void CreateRingLayerDropdown(int id, RenderTexture rt)
    {
        var elementDropdown = dropdownItemAsset.CloneTree();
        var elementImage = imageItemAsset.CloneTree();
        var elementButton = buttonItemAsset.CloneTree();

        layerDropdownAux = elementDropdown.Q<DropdownField>("texture-dropdown");
        Image image = elementImage.Q<Image>("texture-image");
        Button button = elementButton.Q<Button>("texture-button");

        VisualElement rightRow = CreateRowContainer();

        layerDropdownAux.label = $"Layer {id}";

        List<string> choices = new();

        foreach (var tex in dropdownTextures)
            choices.Add(tex.name.ToString());

        layerDropdownAux.choices = choices;

        if (initialVisibleTextureId != -1)
        {
            if (secondaryTextureId != -1)
            {
                int next = -1;

                for (int j = 0; j < dropdownTextures.Count; j++)
                    if (dropdownTextures[j].id == secondaryTextureId)
                    {
                        next = dropdownTextures[j + id - 1].id;
                        break;
                    }

                RemoteTexture secondTex = dropdownTextures.Find
                (
                    t => t.id == next
                );

                layerDropdownAux.value = secondTex.name.ToString();

                secondaryTextureId = secondTex.id;

                HandleTextureSelection(secondTex, id, true);
            }
            else
            {
                int next = -1;

                for (int j = 0; j < dropdownTextures.Count; j++)
                    if (dropdownTextures[j].id == initialVisibleTextureId)
                    {
                        if (j + 1 != dropdownTextures.Count)
                            next = dropdownTextures[j + 1].id;
                        else
                            next = dropdownTextures[0].id;

                        break;
                    }

                RemoteTexture secondTex = dropdownTextures.Find
                (
                    t => t.id == next
                );

                layerDropdownAux.value = secondTex.name.ToString();

                secondaryTextureId = secondTex.id;

                HandleTextureSelection(secondTex, id, true);
            }
        }
        else
        {
            if (secondaryTextureId != -1)
            {
                int next = -1;

                for (int j = 0; j < dropdownTextures.Count; j++)
                    if (dropdownTextures[j].id == secondaryTextureId)
                    {
                        if (j + id - 1 != dropdownTextures.Count)
                            next = dropdownTextures[j + id - 1].id;
                        else
                            next = dropdownTextures[0].id;

                        break;
                    }

                RemoteTexture secondTex = dropdownTextures.Find
                (
                    t => t.id == next
                );

                layerDropdownAux.value = secondTex.name.ToString();

                secondaryTextureId = secondTex.id;

                HandleTextureSelection(secondTex, id, true);
            }
            else
            {
                layerDropdownAux.value = choices[1];

                RemoteTexture secondTex = dropdownTextures.Find
                (
                    t => t.name.ToString() == choices[1]
                );

                layerDropdownAux.value = secondTex.name.ToString();

                secondaryTextureId = secondTex.id;

                HandleTextureSelection(secondTex, id, true);
            }
        }

        layerDropdownAux.RegisterValueChangedCallback(evt =>
        {
            RemoteTexture selected = dropdownTextures.Find
            (
                t => t.name.ToString() == evt.newValue
            );
            HandleTextureSelection(selected, id, true);
        });

        image.image = rt;
        image.scaleMode = ScaleMode.ScaleToFit;
        image.style.position = Position.Relative;
        image.style.width = 637f;
        image.style.height = 637f;
        image.style.marginTop = 35;

        button.style.position = Position.Absolute;
        button.style.top = 0;
        button.style.right = 0;
        button.text = "";
        Texture2D icon = Resources.Load<Texture2D>("Images/cerrar_fondo");
        button.style.backgroundImage = new StyleBackground(icon);
        button.style.backgroundColor = new StyleColor(Color.clear);
        button.style.borderTopWidth = 0;
        button.style.borderBottomWidth = 0;
        button.style.borderLeftWidth = 0;
        button.style.borderRightWidth = 0;
        button.style.width = 60;
        button.style.height = 60;
        button.style.marginTop = 50;

        textureDropdowns.Add(layerDropdownAux);
        container.Add(layerDropdownAux);

        rightRow.Add(image);
        rightRow.Add(button);
        auxContainer.Add(rightRow);

        LayerUI layer = new()
        {
            rightRow = rightRow,
            dropdown = layerDropdownAux,
            image = image,
            button = button
        };

        layerRows.Add(layer);

        controller.RenderRingPreviews();

        button.clicked += () =>
        {
            RemoveLayer(layer);
        };
    }

    /// <summary>
    /// Crear auxiliar del dropdown de texturas para los layers
    /// </summary>
    private void AuxCreateRingLayerDropdown(int id, RenderTexture selectedValueRT, string selectedValue)
    {
        var elementDropdown = dropdownItemAsset.CloneTree();
        var elementImage = imageItemAsset.CloneTree();
        var elementButton = buttonItemAsset.CloneTree();

        layerDropdownAux = elementDropdown.Q<DropdownField>("texture-dropdown");
        Image image = elementImage.Q<Image>("texture-image");
        Button button = elementButton.Q<Button>("texture-button");

        VisualElement rightRow = CreateRowContainer();

        layerDropdownAux.label = $"Layer {id}";

        List<string> choices = new();

        foreach (var tex in dropdownTextures)
            choices.Add(tex.name.ToString());

        layerDropdownAux.choices = choices;

        layerDropdownAux.value = selectedValue;
        
        RemoteTexture selected = dropdownTextures.Find
        (
            t => t.name.ToString() == selectedValue
        );

        secondaryTextureId = selected.id;
        
        HandleTextureSelection(selected, id, false);

        layerDropdownAux.RegisterValueChangedCallback(evt =>
        {
            RemoteTexture selected = dropdownTextures.Find
            (
                t => t.name.ToString() == evt.newValue
            );
            HandleTextureSelection(selected, id, true);
        });

        image.image = selectedValueRT;
        image.scaleMode = ScaleMode.ScaleToFit;
        image.style.position = Position.Relative;
        image.style.width = 637f;
        image.style.height = 637f;
        image.style.marginTop = 35;

        button.style.position = Position.Absolute;
        button.style.top = 0;
        button.style.right = 0;
        button.text = "";
        Texture2D icon = Resources.Load<Texture2D>("Images/cerrar_fondo");
        button.style.backgroundImage = new StyleBackground(icon);
        button.style.backgroundColor = new StyleColor(Color.clear);
        button.style.borderTopWidth = 0;
        button.style.borderBottomWidth = 0;
        button.style.borderLeftWidth = 0;
        button.style.borderRightWidth = 0;
        button.style.width = 60;
        button.style.height = 60;
        button.style.marginTop = 50;

        textureDropdowns.Add(layerDropdownAux);
        container.Add(layerDropdownAux);

        rightRow.Add(image);
        rightRow.Add(button);
        auxContainer.Add(rightRow);

        LayerUI layer = new()
        {
            rightRow = rightRow,
            dropdown = layerDropdownAux,
            image = image,
            button = button
        };

        layerRows.Add(layer);

        controller.RenderRingPreviews();

        button.clicked += () =>
        {
            RemoveLayer(layer);
        };
    }

    private VisualElement CreateRowContainer()
    {
        var row = new VisualElement();

        row.style.flexDirection = FlexDirection.Row;
        row.style.justifyContent = Justify.FlexStart;

        return row;
    }

    /// <summary>
    /// Eliminar layer
    /// </summary>
    private void RemoveLayer(LayerUI layer)
    {
        idLayer--;

        layerRows.Remove(layer);

        container.Remove(layer.dropdown);
        auxContainer.Remove(layer.rightRow);

        controller.RemoveAllPreviewCameras();

        RefreshLayers();
    }

    /// <summary>
    /// Actualizar layers existentes
    /// </summary>
    private void RefreshLayers()
    {
        List<string> layers = new();

        for (int i = 0; i < layerRows.Count; i++)
            if (layerRows[i].dropdown != null)
                layers.Add(layerRows[i].dropdown.value);

        foreach (var layer in layerRows)
        {
            container.Remove(layer.dropdown);
            auxContainer.Remove(layer.rightRow);
        }

        layerRows.Clear();

        for (int i = 0; i < layers.Count; i++)
            AuxCreateRingLayerDropdown(i + 1, controller.InitializePreviewCamera(i + 1), layers[i]);

        if (auxContainer.childCount == 0)
        {
            auxContainerWindow.style.display = DisplayStyle.None;
            auxContainerWindow.pickingMode = PickingMode.Ignore;
        }
    }

    private void RestoreOriginalMaterial()
    {
        // Protegemos la función para que no crashee si falta algo
        if (rendererModel == null || controller == null || textureManager == null) 
            return;

        controller.ApplyMode(VisualizationMode.Standard);

        bool visible = true;

        foreach (var tex in textureManager.GetAllRemoteTextures())
            if (tex.isVisible)
            {
                visible = false;

                LocalTextureData data = textureManager.GetLocalTextureData(tex.id);
                if (data != null && data.localTexture2D != null)
                    controller.SetTexture("_AlphaLayer", data.localTexture2D);

                break;
            }

        if (visible)
            controller.SetTexture("_AlphaLayer", null);
    }
    
    private void OnVisLockChanged(ulong oldVal, ulong newVal)
    {
        ulong myId = NetworkManager.Singleton.LocalClientId;
        if (newVal != ulong.MaxValue && newVal != myId)
        {
            if (ToolMessageHandler.Instance != null)
                ToolMessageHandler.Instance.ShowMessage("Another user has taken control of the Visualization modes.", 4f, MessageType.Error);
            if (ToolMenuController.Instance != null)
                ToolMenuController.Instance.GoBackToPreviousMenu();
        }
    }

    private void OnTexLockChanged(ulong oldVal, ulong newVal)
    {
        ulong myId = NetworkManager.Singleton.LocalClientId;
        if (newVal != ulong.MaxValue && newVal != myId)
        {
            if (ToolMessageHandler.Instance != null)
                ToolMessageHandler.Instance.ShowMessage("Another user is using the Textures tool.", 4f, MessageType.Error);
            if (ToolMenuController.Instance != null)
                ToolMenuController.Instance.GoBackToPreviousMenu();
        }
    }

    /// <summary>
    /// Añadir visibilidad a la textura principal, guardar el secundario y eliminar de descargas el resto
    /// </summary>
    private void VisibleTexture()
    {
        int firstTex = -1;
        int secondTex = -1;
        List<int> otherTex = new();

        foreach (var tex in textureManager.GetAllRemoteTextures())
        {
            if (textureManager.IsTextureDownloaded(tex.id) && !tex.isVisible && tex.id == mainTextureId)
                firstTex = tex.id;
            if (textureManager.IsTextureDownloaded(tex.id) && tex.isVisible && tex.id != mainTextureId && tex.id == secondaryTextureId)
                secondTex = tex.id;
            if (textureManager.IsTextureDownloaded(tex.id) && tex.id != mainTextureId && tex.id != secondaryTextureId)
                otherTex.Add(tex.id);
        }

        if (firstTex != -1)
            textureManager.ToggleTextureVisibility(firstTex);
        if (secondTex != -1 && firstTex == -1)
            textureManager.ToggleTextureVisibility(secondTex);

        foreach (var id in otherTex)
            if (id != secondTex)
                textureManager.RemoveTexture(id);
    }

    private class LayerUI
    {
        public VisualElement rightRow;
        public DropdownField dropdown;
        public Image image;
        public Button button;
    }

    public void CloseFromBackButton()
    {
        closedByBackButton = true;
    }
}