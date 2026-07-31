using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Controlador de UI para mostrar y gestionar la lista de materiales.
/// Crea toggles para cada material y configura los eventos de selección.
/// </summary>
public class MaterialUI : BaseUI
{
    /// <summary>
    /// Plantilla visual para los toggles de material.
    /// </summary>
    [SerializeField] private VisualTreeAsset materialToggleTemplate;
    /// <summary>
    /// Referencia al controlador de la página principal.
    /// </summary>
    [SerializeField] private HomePageUI homePageUI;
    
    /// <summary>
    /// Nombre del contenedor de toggles de materiales en el árbol visual.
    /// </summary>
    private string materialsTogglesContainerName = "material-container";
    /// <summary>
    /// Nombre del toggle de material en el árbol visual.
    /// </summary>
    private string materialToggleName = "material-toggle";
    
    /// <summary>
    /// Callback que se ejecuta al recibir la lista de materiales.
    /// </summary>
    /// <param name="materials">Respuesta con la lista de materiales.</param>
    /// <param name="success">Indica si la obtención fue exitosa.</param>
    private void OnMaterialListReceived(MaterialResponse materials, bool success)
    {
        if (success)
        {
            InsertToggles(materials);
        }
        else
        {
            Debug.LogWarning("Error when obtaining material list.");
        }
    }

    /// <summary>
    /// Inserta toggles para cada material recibido y configura sus eventos.
    /// </summary>
    /// <param name="materials">Respuesta con la lista de materiales.</param>
    private void InsertToggles(MaterialResponse materials)
    {
        var materialsFoldoutRoot = rootElement.Q<VisualElement>(materialsTogglesContainerName);
        materialsFoldoutRoot.Clear();
        
        foreach (var material in materials.items)
        {
            var materialToggleInstance = materialToggleTemplate.CloneTree();
            var materialToggle = materialToggleInstance.Q<Toggle>(materialToggleName);
            materialToggle.text = material.name;
            materialToggle.value = false;
            materialsFoldoutRoot.Add(materialToggleInstance);
            homePageUI.ConfigureToggleEvents(materialToggle, material.name, material.id, FilterType.Material);
        }
    }

    /// <summary>
    /// Inicializa la UI solicitando la lista de materiales.
    /// </summary>
    protected override void InitializeUI()
    {
        StartCoroutine(MaterialDB.GetMaterialList(OnMaterialListReceived));
    }
}