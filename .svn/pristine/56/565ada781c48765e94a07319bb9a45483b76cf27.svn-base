using System;
using System.Collections.Generic;
using UIControllers;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

/// <summary>
/// Clase que gestiona la interfaz de usuario para listar y filtrar datos anexos (AnnexData).
/// Permite ordenar y filtrar los datos anexos asociados a un modelo E3D seleccionado.
/// </summary>
public class AnnexDataListUI : ToolComponent
{
    private readonly Dictionary<string, string> displayLabels = new()
    {
        { "None", OrderType.None.ToString() },
        { "A-Z", OrderType.NameAsc.ToString() },
        { "Z-A", OrderType.NameDesc.ToString() },
        { "Creation date asc.", OrderType.TimeAsc.ToString() },
        { "Creation date desc.", OrderType.TimeDesc.ToString() }
    };
    // UI Elements
    private string returnButtonName = "back-button";
    
    // Map things
    public VisualTreeAsset annexDataElement;
    public GameObject multimediaViewerPrefab;

    private GameObject multimediaViewerInstance;
    private MultimediaManager multimediaManager;
    private int pendingRequests = 0;
    private List<AnnexDataData> combinedAnnexData = new List<AnnexDataData>();

    private VisualElement documentsContainer;


    private string itemsContainerName = "annexdata-container";
    private string itemLabelName = "annexdata-name";
    private string itemLabelDateName = "annexdata-date";
    private string itemLabelUserName = "annexdata-username";
    private string documentsContainerName = "documents-window";

    private string sortByDropdownName = "ordering-options";
    private string typeFilterDropdownName = "type-options";

    private DropdownField orderingDropdown;
    private DropdownField mimeTypeDropdown;
    
    private int chElementID;
    private int e3dModelID;

    public string nameOrderingValue;
    public string timeOrderingValue;

    private string searchValue = "";
    private string sortBy = "";
    private string orderDirection = "";

    private List<string> mimeType = new List<string>();



    //private int siteID;

    protected override void OnEnable()
    {
        base.OnEnable();
        InitializeUI();
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
        if (multimediaViewerInstance != null)
        {
            multimediaManager.OnReturnToAnnexData.RemoveListener(HideDocumentsContainer);
            Destroy(multimediaViewerInstance);
            multimediaViewerInstance = null;
        }
       
    }

    /// <summary>
    /// Inicializa la interfaz de usuario y configura los elementos necesarios.
    /// </summary>
    private void InitializeUI()
    {
        InitializeButtons();
        if (GlobalVariables.Instance.GetSelectedE3DModelData() != null)
        {
            E3DModelData selectedE3DModelData = GlobalVariables.Instance.GetSelectedE3DModelData();
            e3dModelID = GlobalVariables.Instance.GetSelectedE3DModelData().id;
            chElementID = selectedE3DModelData.id_chelement;
            ListAnnexData();

            //Instanciaremos el canvas del viewer aquí
            //Pero antes, buscaremos por si ya existe este multimediaviewer debido a que VR lo trae preinstanciado
            var multimediaViewerInScene = GameObject.FindWithTag("MultimediaViewer");
            
            if (multimediaViewerInScene != null)
            {
                multimediaViewerInstance = multimediaViewerInScene;
            }
            else
            {
                multimediaViewerInstance = Instantiate(multimediaViewerPrefab);
            }
            multimediaManager = multimediaViewerInstance.GetComponentInChildren<MultimediaManager>();
            multimediaManager.OnReturnToAnnexData.AddListener(HideDocumentsContainer);
            
            documentsContainer = uIDocument.rootVisualElement.Q<VisualElement>(documentsContainerName);
            ShowDocumentsContainer(false);

            if (multimediaManager == null)
            {
                Debug.LogError("MultimediaManager component not found in the instantiated prefab.");
            }

            orderingDropdown = uIDocument.rootVisualElement.Q<DropdownField>(sortByDropdownName);
            mimeTypeDropdown = uIDocument.rootVisualElement.Q<DropdownField>(typeFilterDropdownName);


            if (orderingDropdown != null)
            {
                // Set display values (user-friendly)
                orderingDropdown.choices = new List<string>(displayLabels.Keys);

                // Register for change callback
                orderingDropdown.RegisterValueChangedCallback(OnSortChanged);
            }
            
            if (mimeTypeDropdown != null)
            {
                mimeTypeDropdown.RegisterValueChangedCallback(OnMimeTypeChanged);
            }
        }
        else
        {
            Debug.LogError("No E3D Model selected. Cannot load annex data.");
        }
    }

    /// <summary>
    /// Maneja el evento cuando se cambia el filtro de tipo MIME.
    /// </summary>
    private void OnMimeTypeChanged(ChangeEvent<string> evt)
    {
        if(mimeType == null)
            mimeType = new List<string>();
        mimeType.Clear();

        switch (evt.newValue)
        {
            case "All":
                mimeType = null;
                break;
            case "PDF":
                mimeType.Add("application/pdf");
                break;
            case "Image":
            case "Audio":
            case "Video":
                mimeType = HelpFunctions.BuildMediaQuery(evt.newValue);
                break;
            default:
                mimeType = null;
                break;
        }
        ListAnnexData();
    }
    /// <summary>
    /// Maneja el evento cuando se cambia el criterio de ordenación.
    /// </summary>
    private void OnSortChanged(ChangeEvent<string> evt)
    {
        string selectedDisplay = evt.newValue;
        if (displayLabels.TryGetValue(selectedDisplay, out string internalValue))
        {
            // Use internalValue (e.g., "nameAsc", "timeDesc", etc.) in your logic
            switch (internalValue)
            {
                case "None":
                    sortBy = null;
                    orderDirection = null;
                    break;
                case "NameAsc":
                    sortBy = nameOrderingValue;
                    orderDirection = "asc";
                    break;
                case "NameDesc":
                    sortBy = nameOrderingValue;
                    orderDirection = "desc";
                    break;
                case "TimeAsc":
                    sortBy = timeOrderingValue;
                    orderDirection = "asc";
                    break;
                case "TimeDesc":
                    sortBy = timeOrderingValue;
                    orderDirection = "desc";
                    break;
                default:
                    sortBy = null;
                    orderDirection = null;
                    break;
            }
            ListAnnexData();
        }
    }
    
    private void OnPartialDataReceived(AnnexDataResponse annexDataResponse, bool success)
    {
        pendingRequests--;

        if (success && annexDataResponse != null && annexDataResponse.items != null)
        {
            combinedAnnexData.AddRange(annexDataResponse.items);
        }
        else
        {
            Debug.LogWarning("Error al obtener una de las listas de annex data.");
        }

        // Solo instanciamos la UI cuando ambas peticiones han devuelto respuesta
        if (pendingRequests == 0)
        {
            InsertItems(combinedAnnexData);
        }
    }
    
    /// <summary>
    /// Oculta el contenedor de documentos y desinicializa el gestor multimedia.
    /// </summary>
    private void HideDocumentsContainer()
    {
        multimediaManager.Deinitialize();
        ShowDocumentsContainer(false);
    }
    
    /// <summary>
    /// Muestra u oculta el contenedor de documentos.
    /// </summary>
    private void ShowDocumentsContainer(bool show)
    {
        if (documentsContainer != null)
        {
            documentsContainer.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            multimediaViewerInstance.SetActive(show);
        }
        else
        {
            Debug.LogError("Documents container is null.");
        }
    }


    private void InitializeButtons()
    {
        /*var returnButton = rootElement.Q<Button>(returnButtonName);
        if (returnButton != null)
            returnButton.clicked += ReturnHome;
        */
        
        //siteID = selectedE3DModelData.site_id;
    }

    /// <summary>
    /// Inserta los elementos de datos anexos en la interfaz de usuario.
    /// </summary>
    private void InsertItems(List<AnnexDataData> annexDataItems)
    {
        var rootElement = uIDocument.rootVisualElement;
        var itemsContainer = rootElement.Q<ScrollView>(itemsContainerName);

        foreach (var annexData in annexDataItems)
        {
            if (AnnexDataHasValidFiles(annexData.docs))
            {
                var element = annexDataElement.CloneTree();
                itemsContainer.Add(element);
                // Assign name and description to the element
                var nameLabel = element.Q<Label>(itemLabelName);
                if (nameLabel != null)
                    nameLabel.text = annexData.title;
                var dateLabel = element.Q<Label>(itemLabelDateName);
                if (dateLabel != null)
                    dateLabel.text = HelpFunctions.FormatDate(annexData.created_on);
                var usernameLabel = element.Q<Label>(itemLabelUserName);
                if (usernameLabel != null)
                    usernameLabel.text = annexData.username;
                int annexDataID = annexData.id;
                element.RegisterCallback<ClickEvent>(ev =>
                {
                    if (ev.button == 0)
                    {
                        ShowDocumentsContainer(true);
                        multimediaManager.Initialize(uIDocument, annexDataID);
                    }
                });
            }
        }
    }
    
    /// <summary>
    /// Verifica si los datos anexos contienen archivos válidos (PDF, audio, video
    /// o imágenes).
    /// </summary>
    public bool AnnexDataHasValidFiles(List<Doc> docs)
    {
        foreach (var doc in docs)
        {
            if (doc.content_type.Contains("pdf") || doc.content_type.Contains("audio") ||
                doc.content_type.Contains("video") || doc.content_type.Contains("image"))
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Limpia los elementos actuales de la lista de datos anexos en la UI.
    /// </summary>
    private void ClearItems()
    {
        var rootElement = uIDocument.rootVisualElement;
        var itemsContainer = rootElement.Q<ScrollView>(itemsContainerName);
        itemsContainer.Clear();
    }
    
    /// <summary>
    /// Lista los datos anexos asociados al modelo E3D seleccionado,
    /// aplicando los filtros y ordenaciones seleccionadas.
    /// </summary>
    public void ListAnnexData()
    {
        ClearItems();
        combinedAnnexData.Clear();
        pendingRequests = 2;

        DropdownField sortByDropdown = uIDocument.rootVisualElement.Q<DropdownField>(sortByDropdownName);
        string currentSortBy = sortByDropdown != null ? sortByDropdown.value : "date_desc";

        StartCoroutine(CHElementDB.GetAnnexDataListFromCHElement(OnPartialDataReceived,
            chElementID, sort: currentSortBy, order: orderDirection, mimeType: mimeType));

        StartCoroutine(E3DModelDB.GetAnnexDataListFromE3DModel(OnPartialDataReceived, 
            e3dModelID, sort: currentSortBy, order: orderDirection, mimeType: mimeType)); 
    }
}