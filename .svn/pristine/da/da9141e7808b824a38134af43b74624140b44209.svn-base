using UnityEngine.UIElements;

/// <summary>
/// Toggle personalizado de UI Toolkit que almacena un valor interno adicional como string.
/// </summary>
[UxmlElement]
public partial class ToggleWithInternalValue : Toggle
{
    /// <summary>
    /// Valor interno asociado al toggle, útil para lógica personalizada.
    /// </summary>
    [UxmlAttribute("internal-value")]
    public string InternalValue { get; set; } = string.Empty;

    /// <summary>
    /// Constructor por defecto.
    /// </summary>
    public ToggleWithInternalValue()
    {
    }
    
    /// <summary>
    /// Obtiene el valor interno asociado a este toggle.
    /// </summary>
    /// <returns>El valor interno como string.</returns>
    public string GetInternalValue()
    {
        return InternalValue;
    }
}