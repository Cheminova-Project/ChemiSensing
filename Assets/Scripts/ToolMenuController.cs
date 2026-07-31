using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

/// <summary>
/// Controlador del menú de herramientas. Gestiona la UI y la lógica de navegación entre herramientas y submenús.
/// </summary>
public class ToolMenuController : MonoBehaviour
{
    /// <summary>
    /// Documento de UI asociado al menú de herramientas.
    /// </summary>
    [SerializeField] private UIDocument uiDocument;
    /// <summary>
    /// Base de datos de herramientas disponibles.
    /// </summary>
    [SerializeField] private ToolDatabase toolDatabase;
    /// <summary>
    /// Template visual para los botones de herramientas.
    /// </summary>
    [SerializeField] private VisualTreeAsset toolButtonTemplate;

    /// <summary>
    /// Instancia singleton del controlador de menú de herramientas.
    /// </summary>
    public static ToolMenuController Instance { get; private set; }

    private VisualElement windowContainer;
    private VisualElement toolbarContainer;
    private VisualElement activeWindow;
    private GameObject activeScriptsContainer;
    private GameObject activeSubmenuContainer; // Para mantener el container del submenú activo

    private ToolDefinition[] currentTools;
    private Stack<ToolDefinition[]> menuStack = new Stack<ToolDefinition[]>();
    private Stack<GameObject> submenuContainerStack = new Stack<GameObject>(); // Stack para containers de submenús
    private Dictionary<string, GameObject> activeIndependentContainers = new Dictionary<string, GameObject>();
    private string activeToolId = null;
    private bool isInitialized = false;



    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Ya existe una instancia de ToolMenuController. Destruyendo duplicado.");
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    public UIDocument GetUIDocument()
    {
        return uiDocument;
    }

    void OnEnable()
    {
        // Esperar a que Start() haya sido llamado
        if (isInitialized && windowContainer != null && toolbarContainer != null)
        {
            var root = uiDocument.rootVisualElement;
            windowContainer = root.Q<VisualElement>("tool-window");
            toolbarContainer = root.Q<VisualElement>("bottom-menu");
            
            OnUIDocumentEnabled();
        }
    }

    void OnDisable()
    {   
        // Solo guardar si ya está inicializado
        if (isInitialized && windowContainer != null && toolbarContainer != null)
        {
            OnUIDocumentDisabled();
        }
    }

    private void Start()
    {
        var root = uiDocument.rootVisualElement;
        windowContainer = root.Q<VisualElement>("tool-window");
        toolbarContainer = root.Q<VisualElement>("bottom-menu");

        // Mostrar menú inicial solo la primera vez
        ShowMenu(toolDatabase.Tools.ToArray());
        isInitialized = true;

        ShowLoadingScreen(true);
        if (InspectedObjectController.Instance != null)
        {
            InitializeListener();
        }
        else
        {
            InspectedObjectController.OnInspectedObjectControllerInitialized += InitializeListener;

            //DEBUG SIN NETWORKING
            if(NetworkManager.Singleton == null)
            {
                ShowLoadingScreen(false);
            }
        }
    }

    private void InitializeListener()
    {
        InspectedObjectController.Instance.OnInspectedObjectLoaded += HideLoadingScreen;
    }

    void OnDestroy()
    {
        InspectedObjectController.Instance.OnInspectedObjectLoaded -= HideLoadingScreen;
    }

    private void HideLoadingScreen()
    {
        ShowLoadingScreen(false);
    }

    /// <summary>
    /// Se llama cuando el UIDocument se habilita en el panel (cada vez que se reactiva)
    /// </summary>
    private void OnUIDocumentEnabled()
    {
        // Si ya fue inicializado, restaurar el estado previo
        if (isInitialized)
            RestoreMenu();
    }

    /// <summary>
    /// Se llama cuando el UIDocument se deshabilita (se oculta)
    /// </summary>
    private void OnUIDocumentDisabled()
    {
        menuStack.Clear();
        submenuContainerStack.Clear();
        currentTools = null;
        
        // Solo destruir el container de herramienta activo, NO el del submenú
        if (activeScriptsContainer != null)
        {
            Destroy(activeScriptsContainer);
            activeScriptsContainer = null;
        }
        
        CleanupNonPersistentTools();
    }

    /// <summary>
    /// Restaura el estado previo del menú
    /// </summary>
    private void RestoreMenu()
    {
        ShowMenu(toolDatabase.Tools.ToArray());
    }

    /// <summary>
    /// Busca una herramienta por su ID en el ToolDatabase sin importar la profundidad del submenú.
    /// </summary>
    private ToolDefinition FindToolById(string toolId)
    {
        return FindToolRecursive(toolDatabase.Tools, toolId);
    }

    private ToolDefinition FindToolRecursive(IEnumerable<ToolDefinition> tools, string toolId)
    {
        if (tools == null) return null;

        foreach (var tool in tools)
        {
            if (tool.Id == toolId)
                return tool;

            if (tool.IsSubmenu && tool.SubmenuTools != null)
            {
                var found = FindToolRecursive(tool.SubmenuTools, toolId);
                if (found != null)
                    return found;
            }
        }

        return null;
    }
    
    public void ForceCloseActiveGroupTool()
    {
        if (windowContainer != null)
        {
            windowContainer.Clear();
        }
        ShowWindowContainer(false);
        activeWindow = null;

        if (activeScriptsContainer != null)
        {
            Destroy(activeScriptsContainer);
            activeScriptsContainer = null;
        }

        if (toolbarContainer != null)
        {
            foreach (var child in toolbarContainer.Children())
            {
                if (child is Toggle toggle)
                {
                    var toolDef = FindToolById(child.name);
                    if (toolDef != null && toolDef.BehaviourType == ToolBehaviourType.GroupToggle)
                    {
                        toggle.SetValueWithoutNotify(false);
                    }
                }
            }
        }
    }

    private void ShowLoadingScreen(bool show)
    {
        var loadingScreen = uiDocument.rootVisualElement.Q<VisualElement>("loading-screen");
        if (loadingScreen != null)
        {
            loadingScreen.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }
        else
        {
            Debug.LogError("Loading screen element not found in the UI Document.");
        }
    }

    private void ShowMenu(ToolDefinition[] tools)
    {
        if (toolbarContainer == null)
        {
            Debug.LogWarning("[ToolMenuController] toolbarContainer is null. Skipping menu display.");
            return;
        }

        toolbarContainer.Clear();
        currentTools = tools;
        ShowWindowContainer(false);
        PlayerCharacterType currentPlatform = PlatformController.Instance != null
            ? PlatformController.Instance.GetPlayerCharacterType()
            : PlayerCharacterType.Desktop;

        // Botón para volver atrás si estamos en un submenú
        if (menuStack.Count > 0)
        {
            // Clonamos el template dentro del toggle
            var backBtn = new Button();
            toolButtonTemplate.CloneTree(backBtn);

            var backIcon = backBtn.Q<VisualElement>("tool-icon");
            var backName = backBtn.Q<Label>("tool-name");
            backName.text = "Back";
            backIcon.style.backgroundImage = new StyleBackground(toolDatabase.BackIcon);
            backIcon.style.unityBackgroundImageTintColor = new StyleColor(Color.black);
            backBtn.AddToClassList("tool-back-btn");
            backBtn.clicked += GoBackToPreviousMenu;
            toolbarContainer.Add(backBtn);
        }
        
        foreach (var tool in tools)
        {
            if (!tool.IsValidForPlatform(currentPlatform))
                continue;

            if (!GlobalVariables.Instance.GetIsAudioRoom() && tool.isAudioTool)
                continue;

            var toggle = new Toggle();
            toggle.AddToClassList("tool-toggle");
            bool isToolActive = activeIndependentContainers.ContainsKey(tool.Id);
            toggle.SetValueWithoutNotify(isToolActive);
            toggle.SetEnabled(!tool.disabled);
            toggle.name = tool.Id;
            toggle.userData = tool;
            toolButtonTemplate.CloneTree(toggle);

            var icon = toggle.Q<VisualElement>("tool-icon");
            var name = toggle.Q<Label>("tool-name");

            if (tool.Icon != null || tool.ActiveIcon != null)
            {
                Sprite iconToShow = (isToolActive && tool.ActiveIcon != null) ? tool.ActiveIcon : tool.Icon;
                if (iconToShow != null)
                    icon.style.backgroundImage = new StyleBackground(iconToShow);
                
                icon.style.unityBackgroundImageTintColor = new StyleColor(Color.black);
            }

            name.text = tool.DisplayName;
            toggle.RegisterValueChangedCallback(evt =>
            {
                if (icon != null)
                {
                    Sprite nextIcon = (evt.newValue && tool.ActiveIcon != null) ? tool.ActiveIcon : tool.Icon;
                    if (nextIcon != null)
                        icon.style.backgroundImage = new StyleBackground(nextIcon);
                }
                
                if (evt.newValue)
                {
                    if (tool.BehaviourType == ToolBehaviourType.GroupToggle)
                    {
                        foreach (var child in toolbarContainer.Children())
                        {
                            if (child == toggle)
                                continue;
        
                            if (child is Toggle otherToggle)
                            {
                                if (otherToggle.userData is ToolDefinition otherToolDef)
                                {
                                    otherToggle.SetValueWithoutNotify(false);
                                    
                                    var otherIcon = otherToggle.Q<VisualElement>("tool-icon");
                                    if (otherIcon != null && otherToolDef.Icon != null)
                                        otherIcon.style.backgroundImage = new StyleBackground(otherToolDef.Icon);
                                }
                            }
                        }
                    }
                    
                    if (tool.IsSubmenu && tool.SubmenuTools != null && tool.SubmenuTools.Length > 0)
                        OpenSubmenu(tool);
                    else
                        OpenTool(tool);

                    if (tool.BehaviourType == ToolBehaviourType.Button)
                    {
                        toggle.SetValueWithoutNotify(false);
                        
                        if (icon != null && tool.Icon != null)
                            icon.style.backgroundImage = new StyleBackground(tool.Icon);
                    }
                }
                else
                {
                    if (tool.BehaviourType == ToolBehaviourType.GroupToggle)
                    {
                        windowContainer.Clear();
                        if (activeScriptsContainer != null)
                        {
                            Destroy(activeScriptsContainer);
                            activeScriptsContainer = null;
                        }
                            
                    }
                    else if (tool.BehaviourType == ToolBehaviourType.IndependentToggle)
                    {
                        if (activeIndependentContainers.TryGetValue(tool.Id, out GameObject containerToDestroy))
                        {
                            if (containerToDestroy != null)
                                Destroy(containerToDestroy);
                            
                            activeIndependentContainers.Remove(tool.Id);
                        }
                    }
                }
            });

            // Agregar a la jerarquía primero
            toolbarContainer.Add(toggle);

            // Programar recalculación del AutoSizeLabel solo cuando esté en un panel
            if (name is AutoSizeLabel asl)
            {
                asl.RegisterCallback<AttachToPanelEvent>(evt =>
                {
                    asl.ScheduleFontRecalculation();
                });
            }
        }
    }

    private void ShowWindowContainer(bool show)
    {
        windowContainer.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void OpenTool(ToolDefinition tool)
    {
        if (tool.BehaviourType == ToolBehaviourType.GroupToggle)
        {
            windowContainer.Clear();

            if (activeScriptsContainer != null)
            {
                Destroy(activeScriptsContainer);
                activeScriptsContainer = null;
            }
        }

        if (tool.BehaviourType == ToolBehaviourType.GroupToggle && tool.WindowAsset != null)
        {
            var newWindow = tool.WindowAsset.CloneTree();

            newWindow.style.width = Length.Percent(100);
            newWindow.style.height = Length.Percent(100);
            newWindow.style.flexGrow = 1;
            newWindow.style.flexShrink = 0;

            windowContainer.Add(newWindow);
            ShowWindowContainer(true);
            activeWindow = newWindow;
        }
        else if (tool.BehaviourType == ToolBehaviourType.GroupToggle)
        {
            ShowWindowContainer(false);
            activeWindow = null;
        }

        if (tool.ScriptsContainer != null)
        {
            GameObject newContainer = Instantiate(tool.ScriptsContainer, transform);
            
            if (tool.BehaviourType == ToolBehaviourType.GroupToggle)
                activeScriptsContainer = newContainer;
            else if (tool.BehaviourType == ToolBehaviourType.IndependentToggle)
            {
                activeIndependentContainers[tool.Id] = newContainer;
            }
        }
    }
    
    private void OpenSubmenu(ToolDefinition tool)
    {
        CleanupNonPersistentTools();
        // Guardar menú actual y mostrar submenú
        menuStack.Push(currentTools);
        ShowMenu(tool.SubmenuTools);
        windowContainer.Clear();
        
        // Destruir solo el container de herramienta activo, no el del submenú anterior
        if (activeScriptsContainer != null)
        {
            Destroy(activeScriptsContainer);
            activeScriptsContainer = null;
        }

        // Guardar el container del submenú anterior en el stack
        if (activeSubmenuContainer != null)
            submenuContainerStack.Push(activeSubmenuContainer);

        // Instanciar el container del nuevo submenú
        if (tool.ScriptsContainer != null)
        {
            activeSubmenuContainer = Instantiate(tool.ScriptsContainer, transform);
            // Los ToolComponents se activarán automáticamente con OnEnable
        }
        else
        {
            activeSubmenuContainer = null;
        }
    }
    
    /// <summary>
    /// Destruye las herramientas independientes activas que NO están marcadas como persistentes.
    /// </summary>
    private void CleanupNonPersistentTools()
    {
        // Usamos una lista temporal para guardar las claves a borrar, 
        // ya que no se puede modificar un diccionario mientras iteramos sobre él.
        List<string> toolsToRemove = new List<string>();

        foreach (var kvp in activeIndependentContainers)
        {
            string toolId = kvp.Key;
            GameObject container = kvp.Value;

            ToolDefinition toolDef = FindToolById(toolId);

            // Si la herramienta existe y NO es persistente, la marcamos para destruir
            if (toolDef != null && !toolDef.isPersistent)
            {
                if (container != null)
                {
                    Destroy(container);
                }
                toolsToRemove.Add(toolId);
            }
        }

        // Finalmente, las quitamos de nuestro diccionario de activos
        foreach (string id in toolsToRemove)
        {
            activeIndependentContainers.Remove(id);
        }
    }
    
    /// <summary>
    /// Fuerza al menú a retroceder al submenú anterior (simula pulsar el botón Back).
    /// Útil para cuando las herramientas expiran por inactividad.
    /// </summary>
    public void GoBackToPreviousMenu()
    {
        if (menuStack.Count > 0)
        {
            CleanupNonPersistentTools();
            var previousMenu = menuStack.Pop();
            ShowMenu(previousMenu);
            windowContainer.Clear();

            if (activeScriptsContainer != null)
            {
                Destroy(activeScriptsContainer);
                activeScriptsContainer = null;
            }

            if (activeSubmenuContainer != null)
            {
                Destroy(activeSubmenuContainer);
                activeSubmenuContainer = null;
            }

            if (submenuContainerStack.Count > 0)
                activeSubmenuContainer = submenuContainerStack.Pop();
            else
                activeSubmenuContainer = null;
        }
    }
}
