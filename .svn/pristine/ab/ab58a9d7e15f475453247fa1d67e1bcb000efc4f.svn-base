using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// ScriptableObject que contiene la base de datos de todas las herramientas disponibles.
/// Centraliza la configuración de herramientas del sistema.
/// </summary>
[CreateAssetMenu(fileName = "ToolDatabase", menuName = "Tools/ToolDatabase", order = 1)]
public class ToolDatabase : ScriptableObject
{
    /// <summary>
    /// Icono para el botón de retroceso en menús.
    /// </summary>
    public Sprite BackIcon;
    /// <summary>
    /// Lista de todas las herramientas disponibles.
    /// </summary>
    public List<ToolDefinition> Tools;
}

#if UNITY_EDITOR
/// <summary>
/// Editor personalizado para ToolDatabase en el inspector de Unity.
/// Permite gestionar la lista de herramientas y el icono de retroceso.
/// </summary>
[CustomEditor(typeof(ToolDatabase))]
public class ToolDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var db = target as ToolDatabase;

        EditorGUILayout.LabelField("Tool Database", EditorStyles.boldLabel);
        db.BackIcon = (Sprite)EditorGUILayout.ObjectField("Back Icon", db.BackIcon, typeof(Sprite), false);
        if (db.Tools == null)
            db.Tools = new List<ToolDefinition>();

        for (int i = 0; i < db.Tools.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            db.Tools[i] = (ToolDefinition)EditorGUILayout.ObjectField(
                db.Tools[i],
                typeof(ToolDefinition),
                false
            );
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                db.Tools.RemoveAt(i);
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Tool"))
        {
            db.Tools.Add(null);
        }

        EditorUtility.SetDirty(db);
    }
}
#endif