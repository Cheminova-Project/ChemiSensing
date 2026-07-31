using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Globalization;

    /// <summary>
    /// Controlador de UI para la búsqueda de artefactos y recursos.
    /// Gestiona la paginación, filtros, tipos y visualización de resultados de búsqueda.
    /// </summary>
public class SearchUI : BaseUI
{
    /// <summary>
    /// Plantilla visual para cada ítem de resultado de búsqueda.
    /// </summary>
    [SerializeField] private VisualTreeAsset itemTemplate;
    private string itemsContainerName = "artifacts-container";
    
    private string searchButtonName = "search-button";
    private string searchFieldName = "search-bar";
    private string filtersCountName = "filters-count";
    private string prevPageButtonName = "prev-page-button";
    private string nextPageButtonName = "next-page-button";
    private string currentPageLabelName = "current-page-label";
    private string typesContainerName = "types-container";
    
    private string itemLabelName = "item-name";
    private string typeLabelName = "item-type";
    private string descriptionLabelName = "item-description";
    private string lastUpdateName = "last-update";

    private string searchFieldValue = string.Empty;
    private int currentPage = 1;
    private List<string> activeTypesFilters;
    
    /// <summary>
    /// Callback que se ejecuta al recibir la lista de resultados de búsqueda.
    /// </summary>
    /// <param name="searchResponse">Respuesta con los resultados de búsqueda.</param>
    /// <param name="success">Indica si la obtención fue exitosa.</param>
    private void OnSearchListReceived(SearchResponse searchResponse, bool success)
    {
        if (success)
            InsertItems(searchResponse);
        else
            Debug.LogWarning("Error when obtaining search list.");
    }

    /// <summary>
    /// Inserta los resultados de búsqueda en la UI.
    /// </summary>
    /// <param name="searchResponse">Respuesta con los resultados de búsqueda.</param>
    private void InsertItems(SearchResponse searchResponse)
    {
        var itemsContainer = rootElement.Q<ListView>(itemsContainerName);
        var searchItems = searchResponse.items;

        itemsContainer.makeItem = () =>
        {
            return itemTemplate.CloneTree();
        };

        itemsContainer.bindItem = (element, index) =>
        {
            var itemData = searchItems[index];
            var label = element.Q<Label>(itemLabelName);
            if (label != null)
                label.text = itemData.name;
            
            var typeLabel = element.Q<Label>(typeLabelName);
            if (typeLabel != null)
                typeLabel.text = SearchDB.TypedStringToSearchType(itemData.entity_type).ToString();
            
            var descriptionLabel = element.Q<Label>(descriptionLabelName);
            if (descriptionLabel != null)
                descriptionLabel.text = itemData.description;

            DateTime date = DateTime.ParseExact(itemData.created_on, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);

            var lastUpdateLabel = element.Q<Label>(lastUpdateName);
            if (lastUpdateLabel != null)
                lastUpdateLabel.text = date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        };

        itemsContainer.itemsSource = searchItems;
        itemsContainer.selectionType = SelectionType.Single;
        itemsContainer.fixedItemHeight = 400;

        itemsContainer.Rebuild();

        var filtersCountLabel = rootElement.Q<Label>(filtersCountName);
        if (filtersCountLabel != null)
        {
            filtersCountLabel.text = $"{searchItems.Count} results";
        }
        else
        {
            Debug.LogWarning("ERROR: 'filters-count' not found in UI Document.");
        }

        UpdatePagination(searchResponse);
    }

    /// <summary>
    /// Actualiza la paginación de los resultados de búsqueda.
    /// </summary>
    /// <param name="searchResponse">Respuesta con los resultados de búsqueda.</param>
    private void UpdatePagination(SearchResponse searchResponse)
    {
        var prevPageButton = rootElement.Q<Button>(prevPageButtonName);
        var nextPageButton = rootElement.Q<Button>(nextPageButtonName);
        var currentPageLabel = rootElement.Q<Label>(currentPageLabelName);

        if (prevPageButton != null && nextPageButton != null && currentPageLabel != null)
        {
            currentPageLabel.text = $"Page {searchResponse.page ?? 0}";
            prevPageButton.SetEnabled(searchResponse.prev_page != null);
            nextPageButton.SetEnabled(searchResponse.next_page != null);
        }
        else
        {
            Debug.LogWarning("ERROR: Pagination buttons or current page label not found in UI Document.");
        }
    }

    /// <summary>
    /// Limpia los elementos visuales de la lista de resultados.
    /// </summary>
    public void ClearItems()
    {
        var itemsContainer = rootElement.Q<ListView>(itemsContainerName);
        itemsContainer.itemsSource = new List<SearchItem>();
        itemsContainer.Rebuild();
    }

    /// <summary>
    /// Inicializa los botones de paginación y búsqueda.
    /// </summary>
    private void InitializeButtons()
    {
        var prevPageButton = rootElement.Q<Button>(prevPageButtonName);
        if (prevPageButton != null)
        {
            prevPageButton.clicked += () => ChangePage(false);
        }
        else
        {
            Debug.LogWarning("ERROR: 'prev-page-button' not found in UI Document.");
        }

        var nextPageButton = rootElement.Q<Button>(nextPageButtonName);
        if (nextPageButton != null)
        {
            nextPageButton.clicked += () => ChangePage(true);
        }
        else
        {
            Debug.LogWarning("ERROR: 'next-page-button' not found in UI Document.");
        }
        
        var searchButton = rootElement.Q<Button>(searchButtonName);
        if (searchButton != null)
        {
            searchButton.clicked += () =>
            {
                ExecuteSearch();
            };
        }
        else
        {
            Debug.LogWarning("ERROR: 'search-button' not found in UI Document.");
        }
    }
    /// <summary>
    /// Inicializa la UI y solicita la lista de resultados de búsqueda.
    /// </summary>
    protected override void InitializeUI()
    {
        InitializeTypesToggles();
        
        // Input text field
        var searchField = rootElement.Q<TextField>(searchFieldName);
        if (searchField != null)
        {
            searchField.RegisterValueChangedCallback(evt =>
            {
                searchFieldValue = evt.newValue;
            });
        }
        else
        {
            Debug.LogWarning("ERROR: 'search-bar' not found in UI Document.");
        }

        InitializeButtons();
        currentPage = 1;
        ExecuteSearch();
    }

    /// <summary>
    /// Inicializa los toggles de tipos de búsqueda.
    /// </summary>
    public void InitializeTypesToggles()
    {
        activeTypesFilters = new List<string>();
        var typesContainer = rootElement.Q<VisualElement>(typesContainerName);
        if (typesContainer == null)
        {
            Debug.LogWarning($"ERROR: '{typesContainerName}' not found in UI Document.");
            return;
        }
        foreach (var typeToggle in typesContainer.Children())
        {
            if(typeToggle is ToggleWithInternalValue toggle)
            {
                toggle.RegisterValueChangedCallback(evt =>
                {
                    if (evt.newValue)
                    {
                        if(!activeTypesFilters.Contains(toggle.GetInternalValue()))
                            activeTypesFilters.Add(toggle.GetInternalValue());
                    }
                    else
                    {
                        if(activeTypesFilters.Contains(toggle.GetInternalValue()))
                            activeTypesFilters.Remove(toggle.GetInternalValue());
                    }
                });
            }
        }
    }
    
    /// <summary>
    /// Ejecuta la búsqueda con los filtros y parámetros actuales.
    /// </summary>
    private void ExecuteSearch()
    {
        ClearItems();
        List<int> materialFilters = GetMaterialFilters();
        List<int> alterationFilters = GetAlterationFilters();
        
        // Obtener ítems
        StartCoroutine(SearchDB.Search(OnSearchListReceived, searchText: searchFieldValue, alterations: alterationFilters, materials: materialFilters, types:activeTypesFilters,page: currentPage));
    }

    /// <summary>
    /// Cambia la página de los resultados de búsqueda.
    /// </summary>
    /// <param name="isNextPage">Si es true, avanza a la siguiente página; si es false, retrocede.</param>
    private void ChangePage(bool isNextPage)
    {
        if (isNextPage)
            currentPage++;
        else
            currentPage--;

        ExecuteSearch();
    }

    /// <summary>
    /// Obtiene los filtros de materiales activos.
    /// </summary>
    /// <returns>Lista de IDs de materiales filtrados.</returns>
    private List<int> GetMaterialFilters()
    {
        var homeUI = GetComponent<HomePageUI>();
        if (homeUI == null)
        {
            Debug.LogWarning("HomePageUI component not found.");
            return null;
        }
        return homeUI.GetActiveMaterials();
    }

    /// <summary>
    /// Obtiene los filtros de alteraciones activos.
    /// </summary>
    /// <returns>Lista de IDs de alteraciones filtradas.</returns>
    private List<int> GetAlterationFilters()
    {
        var homeUI = GetComponent<HomePageUI>();
        if (homeUI == null)
        {
            Debug.LogWarning("HomePageUI component not found.");
            return null;
        }
        return homeUI.GetActiveAlterations();
    }
}