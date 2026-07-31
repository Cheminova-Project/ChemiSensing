using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Controlador de UI para mostrar y gestionar formularios y alteraciones.
/// Crea foldouts y toggles para cada formulario y alteración, y configura los eventos de selección.
/// </summary>
public class AlterationUI : BaseUI
{
    /// <summary>
    /// Plantilla visual para los formularios de alteración.
    /// </summary>
    [SerializeField] private VisualTreeAsset alterationFormTemplate;
    /// <summary>
    /// Plantilla visual para cada alteración individual.
    /// </summary>
    [SerializeField] private VisualTreeAsset alterationTemplate;
    /// <summary>
    /// Referencia al controlador de la página principal.
    /// </summary>
    [SerializeField] private HomePageUI homePageUI;
    
    /// <summary>
    /// Nombre del foldout de formulario de alteración en el árbol visual.
    /// </summary>
    private string alterationFormFoldoutName = "alteration-form-foldout";
    /// <summary>
    /// Nombre del ScrollView de alteraciones en el árbol visual.
    /// </summary>
    private string alterationFormScrollViewName = "alteration-scrollview";
    /// <summary>
    /// Nombre del toggle de alteración en el árbol visual.
    /// </summary>
    private string alterationToggleName = "alteration-toggle";
    /// <summary>
    /// Nombre del toggle de foldout de formulario de alteración en el árbol visual.
    /// </summary>
    private string alterationFormToggleName = "alterationform-foldout-toggle";
    /// <summary>
    /// Nombre del contenedor de formularios de alteración en el árbol visual.
    /// </summary>
    private string alterationFormsContainerName = "alteration-container";
    
    /// <summary>
    /// Callback que se ejecuta al recibir la lista de formularios de alteración.
    /// </summary>
    /// <param name="data">Respuesta con la lista de formularios de alteración.</param>
    /// <param name="success">Indica si la obtención fue exitosa.</param>
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

    /// <summary>
    /// Crea los foldouts y toggles para cada formulario y alteración recibidos.
    /// </summary>
    /// <param name="alterationForms">Respuesta con la lista de formularios de alteración.</param>
    public void CreateFoldouts(AlterationFormResponse alterationForms)
    {
        var alterationFormsFoldoutRoot = rootElement.Q<VisualElement>(alterationFormsContainerName);
        
        foreach (var alterationForm in alterationForms.items)
        {
            var alterationFormInstance = alterationFormTemplate.CloneTree();
            var alterationFormFoldout = alterationFormInstance.Q<Foldout>(alterationFormFoldoutName);
            alterationFormFoldout.text = alterationForm.name;
            alterationFormFoldout.value = false;
            
            var alterationFormScrollView = alterationFormInstance.Q<ScrollView>(alterationFormScrollViewName);
            var alterationFormFoldoutToggle = alterationFormInstance.Q<Toggle>(alterationFormToggleName);

            foreach (var alteration in alterationForm.alterations)
            {
                var alterationInstance = alterationTemplate.CloneTree();
                var alterationToggle = alterationInstance.Q<Toggle>(alterationToggleName);
                alterationToggle.text = alteration.name;
                alterationToggle.value = false;
                
                alterationFormScrollView.Add(alterationToggle);
                homePageUI.ConfigureToggleEvents(alterationToggle, alteration.name, alteration.id, FilterType.Alteration);
            }
            
            homePageUI.ConfigureToggleEvents(alterationFormFoldoutToggle, alterationForm.name, alterationForm.id, FilterType.AlterationForm, 
                alterationFormScrollView.contentContainer.Children().OfType<Toggle>().ToList());
            
            alterationFormsFoldoutRoot.Add(alterationFormInstance);
        }
    }

    /// <summary>
    /// Inicializa la UI solicitando la lista de formularios de alteración.
    /// </summary>
    protected override void InitializeUI()
    {
        StartCoroutine(AlterationFormDB.GetAlterationFormList(OnAlterationFormListReceived));
    }
}