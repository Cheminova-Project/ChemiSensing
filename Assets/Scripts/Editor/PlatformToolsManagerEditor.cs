using UnityEngine;
using UnityEditor;
using System.Linq;

#if UNITY_EDITOR
/// <summary>
/// Custom Inspector para PlatformToolsManager que proporciona una interfaz más amigable
/// </summary>
[CustomEditor(typeof(PlatformToolsManager))]
public class PlatformToolsManagerEditor : Editor
{
    private PlatformToolsManager manager;
    private SerializedProperty platformGroupsProp;
    private SerializedProperty autoDetectProp;
    private SerializedProperty updateOnChangeProp;
    private SerializedProperty forcedPlatformProp;
    private SerializedProperty enableDebugProp;

    private bool showPlatformGroups = true;
    private bool showSettings = true;
    private bool showDebugInfo = true;

    private void OnEnable()
    {
        manager = (PlatformToolsManager)target;
        
        platformGroupsProp = serializedObject.FindProperty("platformGroups");
        autoDetectProp = serializedObject.FindProperty("autoDetectPlatform");
        updateOnChangeProp = serializedObject.FindProperty("updateOnPlatformChange");
        forcedPlatformProp = serializedObject.FindProperty("forcedPlatform");
        enableDebugProp = serializedObject.FindProperty("enableDebugLogs");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Header
        EditorGUILayout.Space();
        GUILayout.Label("Platform Tools Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Settings Section
        showSettings = EditorGUILayout.Foldout(showSettings, "Settings", true, EditorStyles.foldoutHeader);
        if (showSettings)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(autoDetectProp, new GUIContent("Auto Detect Platform", 
                "Automatically detect platform from PlatformController"));
            
            if (!autoDetectProp.boolValue)
            {
                EditorGUILayout.PropertyField(forcedPlatformProp, new GUIContent("Forced Platform", 
                    "Platform to use when auto-detection is disabled"));
            }
            
            EditorGUILayout.PropertyField(updateOnChangeProp, new GUIContent("Update On Platform Change", 
                "Listen for platform changes and update components automatically"));
            
            EditorGUILayout.PropertyField(enableDebugProp, new GUIContent("Enable Debug Logs", 
                "Show debug information in console"));
            
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Platform Groups Section
        showPlatformGroups = EditorGUILayout.Foldout(showPlatformGroups, 
            $"Platform Groups ({platformGroupsProp.arraySize})", true, EditorStyles.foldoutHeader);
        
        if (showPlatformGroups)
        {
            EditorGUI.indentLevel++;
            
            // Add new group button
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Platform Group", GUILayout.Width(150)))
            {
                AddNewPlatformGroup();
            }
            EditorGUILayout.EndHorizontal();
            
            // Display each group
            for (int i = 0; i < platformGroupsProp.arraySize; i++)
            {
                DrawPlatformGroup(i);
            }
            
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        // Debug Info Section
        if (Application.isPlaying)
        {
            showDebugInfo = EditorGUILayout.Foldout(showDebugInfo, "Runtime Debug Info", true, EditorStyles.foldoutHeader);
            if (showDebugInfo)
            {
                EditorGUI.indentLevel++;
                EditorGUI.BeginDisabledGroup(true);
                
                EditorGUILayout.LabelField("Current Platform", manager.CurrentPlatform.ToString());
                EditorGUILayout.LabelField("Total Groups", manager.PlatformGroups.Count.ToString());
                
                var activeGroups = manager.PlatformGroups.Count(g => g.IsValidForPlatform(manager.CurrentPlatform));
                EditorGUILayout.LabelField("Active Groups", activeGroups.ToString());
                
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }
        }

        // Action Buttons
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Refresh Configuration"))
        {
            manager.RefreshConfiguration();
        }
        
        if (GUILayout.Button("Clean Null Components"))
        {
            manager.CleanNullComponents();
        }
        
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawPlatformGroup(int index)
    {
        var groupProp = platformGroupsProp.GetArrayElementAtIndex(index);
        var platformTypeProp = groupProp.FindPropertyRelative("platformType");
        var componentsProp = groupProp.FindPropertyRelative("components");
        var destroyInsteadProp = groupProp.FindPropertyRelative("destroyInsteadOfDisable");
        var isEnabledProp = groupProp.FindPropertyRelative("isEnabled");

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        // Header with platform type and remove button
        EditorGUILayout.BeginHorizontal();
        
        string headerText = $"{((PlayerCharacterType)platformTypeProp.enumValueIndex)} ({componentsProp.arraySize} components)";
        isEnabledProp.boolValue = EditorGUILayout.ToggleLeft(headerText, isEnabledProp.boolValue, EditorStyles.boldLabel);
        
        GUILayout.FlexibleSpace();
        
        // Delete button
        GUI.backgroundColor = Color.red;
        if (GUILayout.Button("×", GUILayout.Width(20), GUILayout.Height(20)))
        {
            RemovePlatformGroup(index);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndHorizontal();

        if (isEnabledProp.boolValue)
        {
            EditorGUI.indentLevel++;
            
            // Platform Type
            EditorGUILayout.PropertyField(platformTypeProp, new GUIContent("Platform Type"));
            
            // Advanced options
            EditorGUILayout.PropertyField(destroyInsteadProp, new GUIContent("Destroy Instead of Disable", 
                "If true, components will be destroyed instead of disabled"));
            
            // Components list
            EditorGUILayout.PropertyField(componentsProp, new GUIContent("Components"), true);
            
            // Quick add buttons
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Quick Add:", GUILayout.Width(80));
            
            if (GUILayout.Button("Current Object", GUILayout.Width(100)))
            {
                AddComponentsFromCurrentObject(componentsProp);
            }
            
            if (GUILayout.Button("Children", GUILayout.Width(80)))
            {
                AddComponentsFromChildren(componentsProp);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    private void AddNewPlatformGroup()
    {
        platformGroupsProp.arraySize++;
        var newGroupProp = platformGroupsProp.GetArrayElementAtIndex(platformGroupsProp.arraySize - 1);
        
        newGroupProp.FindPropertyRelative("platformType").enumValueIndex = 0;
        newGroupProp.FindPropertyRelative("destroyInsteadOfDisable").boolValue = false;
        newGroupProp.FindPropertyRelative("isEnabled").boolValue = true;
        
        var componentsProp = newGroupProp.FindPropertyRelative("components");
        componentsProp.arraySize = 0;
        
        serializedObject.ApplyModifiedProperties();
    }

    private void RemovePlatformGroup(int index)
    {
        if (EditorUtility.DisplayDialog("Remove Platform Group", 
            "Are you sure you want to remove this platform group?", "Yes", "No"))
        {
            platformGroupsProp.DeleteArrayElementAtIndex(index);
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void AddComponentsFromCurrentObject(SerializedProperty componentsProp)
    {
        var components = manager.GetComponents<Component>()
            .Where(c => c != null && !(c is Transform) && !(c is PlatformToolsManager))
            .ToArray();

        foreach (var component in components)
        {
            componentsProp.arraySize++;
            var newElementProp = componentsProp.GetArrayElementAtIndex(componentsProp.arraySize - 1);
            newElementProp.objectReferenceValue = component;
        }
        
        serializedObject.ApplyModifiedProperties();
    }

    private void AddComponentsFromChildren(SerializedProperty componentsProp)
    {
        var components = manager.GetComponentsInChildren<Component>(true)
            .Where(c => c != null && !(c is Transform) && !(c is PlatformToolsManager))
            .ToArray();

        foreach (var component in components)
        {
            componentsProp.arraySize++;
            var newElementProp = componentsProp.GetArrayElementAtIndex(componentsProp.arraySize - 1);
            newElementProp.objectReferenceValue = component;
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}
#endif