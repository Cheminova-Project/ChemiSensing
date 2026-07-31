using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Clase base abstracta para controladores de UI que usan UI Toolkit.
/// Proporciona inicialización común y acceso a elementos raíz.
/// </summary>
public abstract class BaseUI : MonoBehaviour
{
    /// <summary>
    /// Referencia al componente UIDocument asociado.
    /// </summary>
    protected UIDocument uiDocument;
    /// <summary>
    /// Elemento raíz de la UI.
    /// </summary>
    protected VisualElement rootElement;

    /// <summary>
    /// Inicializa la UI y obtiene referencias principales al habilitar el objeto.
    /// </summary>
    protected virtual void OnEnable()
    {
        uiDocument = GetComponentInParent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogWarning("UIDocument component not found on this GameObject.");
            return;
        }

        rootElement = uiDocument.rootVisualElement;
        InitializeUI();
    }

    /// <summary>
    /// Método abstracto que debe implementar cada UI para inicializar sus elementos.
    /// </summary>
    protected abstract void InitializeUI();
}
