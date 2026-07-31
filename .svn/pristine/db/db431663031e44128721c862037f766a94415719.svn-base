using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Gestiona el menú principal de la aplicación y sus opciones.
/// Permite mostrar, ocultar y navegar entre diferentes secciones del menú.
/// </summary>
public class MenuManager : MonoBehaviour
{
    /// <summary>
    /// Referencia a las opciones del menú.
    /// </summary>
    public UIBar[] mainOptions; // Cámara, Movimiento, Mundo
    public UnityEvent OnExitMainMenu;

    /// <summary>
    /// Muestra el menú principal.
    /// </summary>
    public void ShowMainOptions()
    {
        foreach (var option in mainOptions)
            option.Show();
    }

    /// <summary>
    /// Oculta el menú principal.
    /// </summary>
    public void HideMainOptions()
    {
        foreach (var option in mainOptions)
            option.Hide();
    }

    /// <summary>
    /// Selecciona una opción del menú principal.
    /// </summary>
    public void OnMainOptionSelected(UIBar subWindow)
    {
        HideMainOptions();
        OnExitMainMenu?.Invoke();
        subWindow.Show();
    }
}
