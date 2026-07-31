using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Herramienta para depurar y visualizar estilos USS en dropdowns de la UI.
/// Permite inspeccionar y modificar estilos visuales en tiempo de ejecución.
/// </summary>
public class DropdownUssDebugger : MonoBehaviour
{
    /// <summary>
    /// Referencia al dropdown a depurar.
    /// </summary>
    public object dropdown;

    [Tooltip("If true, prints class names for DropdownField on Awake when entering Play Mode.")]
    public bool logOnAwake = true;

    void Awake()
    {
        if (logOnAwake)
            LogClassNames();
    }

    [ContextMenu("Log DropdownField USS class names")]
    public void LogClassNames()
    {
        try
        {
            Debug.Log($"DropdownField.ussClassName = {DropdownField.ussClassName}");
            Debug.Log($"DropdownField.textUssClassName = {DropdownField.textUssClassName}");
            Debug.Log($"DropdownField.arrowUssClassName = {DropdownField.arrowUssClassName}");
            Debug.Log($"DropdownField.inputUssClassName = {DropdownField.inputUssClassName}");
            Debug.Log($"DropdownField.labelUssClassName = {DropdownField.labelUssClassName}");
            Debug.Log($"DropdownField.disabledUssClassName = {DropdownField.disabledUssClassName}");
            Debug.Log($"DropdownField.alignedFieldUssClassName = {DropdownField.alignedFieldUssClassName}");
            Debug.Log($"DropdownField.mixedValueLabelUssClassName = {DropdownField.mixedValueLabelUssClassName}");
            Debug.Log($"DropdownField.noLabelVariantUssClassName = {DropdownField.noLabelVariantUssClassName}");
            Debug.Log($"DropdownField.labelDraggerVariantUssClassName = {DropdownField.labelDraggerVariantUssClassName}");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"DropdownUssDebugger: could not read all DropdownField properties. Exception: {ex}");
        }

#if UNITY_EDITOR
        Debug.Log("Nota: también puedes abrir el UI Toolkit Debugger (Window > UI Toolkit > Debugger) para inspeccionar elementos y estilos.");
#endif
    }
}
