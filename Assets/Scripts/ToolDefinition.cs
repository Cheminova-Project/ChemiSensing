#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Define el comportamiento del toggle en el menú.
/// </summary>
public enum ToolBehaviourType
{
    GroupToggle,        // Comportamiento original: desactiva otros del mismo tipo.
    IndependentToggle,  // Funciona por su cuenta (como un Mute), no cierra el menú actual.
    Button              // Ejecuta la acción y se desmarca visualmente al instante.
}

/// <summary>
/// ScriptableObject que define una herramienta en el sistema de herramientas.
/// Contiene información básica, soporte de plataformas, UI y scripts asociados.
/// </summary>
[CreateAssetMenu(fileName = "NewTool", menuName = "Tools/ToolDefinition", order = 0)]
public class ToolDefinition : ScriptableObject
{
    /// <summary>
    /// Identificador único de la herramienta.
    /// </summary>
    public string Id;
    /// <summary>
    /// Nombre visible en la interfaz.
    /// </summary>
    public string DisplayName;
    /// <summary>
    /// Icono de la herramienta.
    /// </summary>
    public Sprite Icon;
    /// <summary>
    /// Icono de la herramienta cuando está activa.
    /// </summary>
    public Sprite ActiveIcon;
    /// <summary>
    /// Tipo de comportamiento de la herramienta.
    /// </summary>
    public ToolBehaviourType BehaviourType = ToolBehaviourType.GroupToggle;
    /// <summary>
    /// Herramienta persistente.
    /// </summary>
    public bool isPersistent = false;
    /// <summary>
    /// Herramienta de audio.
    /// </summary>
    public bool isAudioTool = false;
    /// <summary>
    /// Plataformas en las que esta herramienta está disponible.
    /// </summary>
    public PlayerCharacterType[] SupportedPlatforms;
    /// <summary>
    /// Indica si esta herramienta es un submenú con otras herramientas.
    /// </summary>
    public bool IsSubmenu;
    /// <summary>
    /// Asset de la ventana UI para esta herramienta.
    /// </summary>
    public VisualTreeAsset WindowAsset;
    /// <summary>
    /// Segundo asset de la ventana UI para esta herramienta.
    /// </summary>
    public VisualTreeAsset SecondWindowAsset;
    /// <summary>
    /// GameObject que contiene los scripts de la herramienta.
    /// </summary>
    public GameObject ScriptsContainer;
    /// <summary>
    /// Herramientas contenidas en el submenú (si IsSubmenu es true).
    /// </summary>
    public ToolDefinition[] SubmenuTools;
    /// <summary>
    /// Indica si esta herramienta está deshabilitada.
    /// </summary>
    public bool disabled = false;
    /// <summary>
    /// Verifica si esta herramienta es válida para la plataforma especificada.
    /// </summary>
    /// <param name="platform">Tipo de plataforma a verificar.</param>
    /// <returns>True si la herramienta soporta la plataforma.</returns>
    public bool IsValidForPlatform(PlayerCharacterType platform)
    {
        foreach (var p in SupportedPlatforms)
        {
            if (p == platform) return true;
        }
        return false;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ToolDefinition))]
public class ToolDefinitionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Id"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("DisplayName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Icon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ActiveIcon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("BehaviourType"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isPersistent"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isAudioTool"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("SupportedPlatforms"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("IsSubmenu"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("disabled"));
        if (serializedObject.FindProperty("IsSubmenu").boolValue)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SubmenuTools"));
        }
        else
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("WindowAsset"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SecondWindowAsset"));
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ScriptsContainer"));
        serializedObject.ApplyModifiedProperties();
    }
}
#endif