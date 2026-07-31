using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace UIControllers
{
    /// <summary>
    /// Controlador de UI para mostrar y gestionar la lista de elementos culturales (CH Elements).
    /// Permite paginación, filtrado y visualización de detalles de cada elemento.
    /// </summary>
    public class ChElementSiteListUI : BaseUI
    {
        // Map things
        /// <summary>
        /// Plantilla visual para cada elemento de la lista.
        /// </summary>
        [SerializeField] private VisualTreeAsset itemTemplate;
        /// <summary>
        /// Referencia al controlador de la página principal.
        /// </summary>
        [SerializeField] private HomePageUI homePageUI;
        /// <summary>
        /// Icono por defecto para los elementos.
        /// </summary>
        [SerializeField] private Texture2D defaultIcon;
    
        // UI Elements
        private string itemsContainerName = "chelements-container";
        private string backButtonName = "back-button";
        private string prevPageButtonName = "prev-page-button";
        private string nextPageButtonName = "next-page-button";
        private string siteSelectedName = "site-selected";
        private string siteLabelName = "site-label";
        private string currentPageLabelName = "current-page-label";
        private string siteNameName = "site-name";
        private string chElementNameName = "ch-element-name";
        private string chElementDescriptionName = "ch-element-description";
        private string filtersCountName = "filters-count";
        private string lastUpdateName = "last-update";
        private string authorName = "author";
        private string chElementImageName = "chelement-image";
        private string usernameName = "username-text";
    
        // Internal variables
        private int currentPage = 1;
        private int perPage = 10;
        private int? page;
        private int? nextPage;
        private int? prevPage;
        private int parentID = -1;
        private string parentName = "";
        private SiteType siteType = SiteType.None;
        private Coroutine currentSearchCoroutine;

        /// <summary>
        /// Al habilitar el objeto, inicializa la UI y establece el sitio padre por defecto.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        /// <summary>
        /// Callback que se ejecuta al recibir la lista de elementos culturales.
        /// </summary>
        /// <param name="chElementResponse">Respuesta con la lista de elementos.</param>
        /// <param name="success">Indica si la obtención fue exitosa.</param>
        private void OnChElementListReceived(CHElementResponse chElementResponse, bool success)
        {
            if (success)
            {
                page = chElementResponse.page;
                prevPage = chElementResponse.prev_page;
                nextPage = chElementResponse.next_page;
                InsertItems(chElementResponse.items);
            }
            else
            {
                Debug.LogWarning("Error when obtaining CH elements list.");
            }
        }

        /// <summary>
        /// Inserta los elementos culturales en la UI.
        /// </summary>
        /// <param name="chElementDatas">Lista de datos de elementos culturales.</param>
        private void InsertItems(List<CHElementData> chElementDatas)
        {
            ClearItems();
            var itemsContainer = rootElement.Q<ScrollView>(itemsContainerName);
            itemsContainer.mode = ScrollViewMode.Vertical;

            foreach (var chElement in chElementDatas)
            {
                var element = itemTemplate.CloneTree();
                itemsContainer.Add(element);
  
                var siteLabel = element.Q<Label>(siteNameName);
                if (siteLabel != null)
                {
                    if (chElement.parent != null)
                        siteLabel.text = chElement.parent.name;
                    else
                        siteLabel.text = "No collection associated";
                }

                var chElementNameLabel = element.Q<Label>(chElementNameName);
                if (chElementNameLabel != null)
                    chElementNameLabel.text = chElement.name;
        
                var chElementDescriptionLabel = element.Q<Label>(chElementDescriptionName);
                if (chElementDescriptionLabel != null)
                    chElementDescriptionLabel.text = chElement.description;
        
                var lastUpdateLabel = element.Q<Label>(lastUpdateName);
                if (lastUpdateLabel != null)
                    lastUpdateLabel.text = HelpFunctions.FormatDateTo_DD_MM_YYYY(chElement.created_on);
        
                var authorLabel = element.Q<Label>(authorName);
                if (authorLabel != null)
                    authorLabel.text = chElement.username;
        
                var chElementImage = element.Q<VisualElement>(chElementImageName);
        
                int chElementID = chElement.id;
                string chElementName = chElement.name;
                element.RegisterCallback<ClickEvent>(ev =>
                {
                    if (ev.button == 0)
                    {
                        UIDocumentManager.Instance.SwitchContext("e3ds", scripts =>
                        {
                            foreach (var script in scripts)
                                if(script is E3DListUI e3DListUI)
                                    e3DListUI.SetParentCHElement(chElementID, chElementName, parentID, parentName, siteType);
                        });
                    }
                });
                DownloadAndFillCHelementImage(chElementImage, chElement.icon);
            }
        
            var itemsCountLabel = rootElement.Q<Label>(filtersCountName);
            if (itemsCountLabel != null)
                itemsCountLabel.text = chElementDatas.Count + " results";
            
            UpdatePagination();
        }
    
        /// <summary>
        /// Descarga y asigna la imagen del elemento cultural.
        /// </summary>
        /// <param name="image">Elemento visual donde se asignará la imagen.</param>
        /// <param name="chElementIconID">ID del icono del elemento.</param>
        private void DownloadAndFillCHelementImage(VisualElement image, int? chElementIconID)
        {
            StartCoroutine(CHElementDB.GetCHElementIcon(chElementIconID, FillImage(image)));
        }

        /// <summary>
        /// Devuelve una acción para rellenar la imagen de un elemento visual.
        /// </summary>
        /// <param name="imageToFill">Elemento visual a rellenar.</param>
        /// <returns>Acción que asigna la textura.</returns>
        private UnityAction<Texture2D, bool> FillImage(VisualElement imageToFill)
        {
            return (texture, success) =>
            {
                if (success && texture != null)
                {
                    imageToFill.style.backgroundImage = texture;
                }
                else
                {
                    imageToFill.style.backgroundImage = defaultIcon;
                    Debug.LogWarning("Couldn't load CH element image.");
                }
            };
        }
    
        /// <summary>
        /// Actualiza la paginación de la lista de elementos.
        /// </summary>
        private void UpdatePagination()
        {
            var prevPageButton = rootElement.Q<Button>(prevPageButtonName);
            var nextPageButton = rootElement.Q<Button>(nextPageButtonName);
            var currentPageLabel = rootElement.Q<Label>(currentPageLabelName);

            if (prevPageButton != null && nextPageButton != null && currentPageLabel != null)
            {
                // Actualizamos la página actual
                currentPageLabel.text = $"Page {page}";

                // Desactivamos o activamos la flecha izquierda según si hay una página previa
                prevPageButton.SetEnabled(prevPage != null);

                // Desactivamos o activamos la flecha derecha según si hay una página siguiente
                nextPageButton.SetEnabled(nextPage != null);
            }
            else
            {
                Debug.LogWarning("ERROR: Pagination buttons or current page label not found in UI Document.");
            }
        }

        /// <summary>
        /// Limpia los elementos visuales de la lista.
        /// </summary>
        private void ClearItems()
        {
            if(rootElement == null) return;
            var itemsContainer = rootElement.Q<ScrollView>(itemsContainerName);
            if(itemsContainer == null) return;
            itemsContainer.Clear();
        }

        /// <summary>
        /// Inicializa los botones de paginación y otros controles.
        /// </summary>
        private void InitializeButtons()
        {
            var backButton = rootElement.Q<Button>(backButtonName);
            if (backButton != null)
                backButton.clicked += () => UIDocumentManager.Instance.SwitchContext("chElementList", scripts =>
                {
                    foreach (var script in scripts)
                        if(script is ChElementListUI chElementListUI)
                            chElementListUI.ListChElements();
                });
            else
                Debug.LogWarning("ERROR: 'prev-page-button' not found in UI Document.");
            
            // Paginación: botones de "anterior" y "siguiente"
            var prevPageButton = rootElement.Q<Button>(prevPageButtonName);
            if (prevPageButton != null)
                prevPageButton.clicked += () => ChangePage(false);
            else
                Debug.LogWarning("ERROR: 'prev-page-button' not found in UI Document.");

            var nextPageButton = rootElement.Q<Button>(nextPageButtonName);
            if (nextPageButton != null)
                nextPageButton.clicked += () => ChangePage(true);
            else
                Debug.LogWarning("ERROR: 'next-page-button' not found in UI Document.");
        
            var usernameLabel = rootElement.Q<Label>(usernameName);
            if (usernameLabel != null)
                usernameLabel.text = GlobalManagement.Instance.username;
        }
    
        /// <summary>
        /// Inicializa la UI y solicita la lista de elementos culturales.
        /// </summary>
        protected override void InitializeUI()
        {
            InitializeButtons();
        }
    
        /// <summary>
        /// Establece el sitio padre para filtrar los elementos culturales.
        /// </summary>
        /// <param name="id">ID del sitio padre.</param>
        public void SetParentSite(int id, string nameParent, SiteType siteTypeSelected)
        {
            parentID = id;
            parentName = nameParent;
            siteType = siteTypeSelected;
            
            var siteSelectedLabel = rootElement.Q<Label>(siteSelectedName);
            var siteLabel = rootElement.Q<Label>(siteLabelName);

            if (siteLabel != null)
                siteLabel.text = parentName;

            if (siteSelectedLabel != null)
            {
                switch (siteType)
                {
                    case SiteType.Site:
                        siteSelectedLabel.text = "Site selected:";
                        break;
                    case SiteType.Collection:
                        siteSelectedLabel.text = "Collection selected:";
                        break;
                    case SiteType.MyList:
                        siteSelectedLabel.text = "My list selected:";
                        break;
                }
            }
            
            ListChElementsFromSite();
        }

        /// <summary>
        /// Solicita la lista de elementos culturales según el sitio padre.
        /// </summary>
        public void ListChElements()
        {
            ListChElementsFromSite();
        }
        
        /// <summary>
        /// Solicita la lista de elementos culturales de un sitio específico.
        /// </summary>
        private void ListChElementsFromSite()
        {
            switch (siteType)
            {
                case SiteType.MyList:
                    StartCoroutine(MyListDB.GetCHElementListFromMyList(OnChElementListReceived, parentID, currentPage, perPage, homePageUI.GetSearchText(),
                        homePageUI.GetSortBy(), homePageUI.GetOrderDirection()));
                    break;
                case SiteType.Collection:
                    StartCoroutine(CollectionDB.GetCHElementListFromCollection(OnChElementListReceived, parentID, currentPage, perPage, homePageUI.GetSearchText(),
                        homePageUI.GetSortBy(), homePageUI.GetOrderDirection()));
                    break;
                case SiteType.Site:
                    StartCoroutine(SiteDB.GetCHElementListFromSite(OnChElementListReceived, parentID, currentPage, perPage, homePageUI.GetSearchText(),
                        homePageUI.GetSortBy(), homePageUI.GetOrderDirection()));
                    break;
            }
        }

        /// <summary>
        /// Cambia la página de la lista de elementos culturales.
        /// </summary>
        /// <param name="isNextPage">Si es true, avanza a la siguiente página; si es false, retrocede.</param>
        private void ChangePage(bool isNextPage)
        {
            if (isNextPage)
                currentPage++;
            else
                currentPage--;

            ListChElements();
        }
    }
}