using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;

[CustomEditor(typeof(DoubleStateButton))]
public class DoubleStateButtonEditor : ButtonEditor
{
    SerializedProperty m_firstStateSprite;
    SerializedProperty m_secondStateSprite;
    SerializedProperty onFirstState;
    SerializedProperty onSecondState;
    SerializedProperty m_applyProhibitedIcon;
    SerializedProperty m_currentState;

    protected override void OnEnable()
    {
        base.OnEnable();
        m_firstStateSprite = serializedObject.FindProperty("m_firstStateSprite");
        m_secondStateSprite = serializedObject.FindProperty("m_secondStateSprite");
        onFirstState = serializedObject.FindProperty("onFirstState");
        onSecondState = serializedObject.FindProperty("onSecondState");
        m_applyProhibitedIcon = serializedObject.FindProperty("m_isBooleanButton");
        m_currentState = serializedObject.FindProperty("m_currentState");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Dibuja el inspector original de Button
        base.OnInspectorGUI();

        // Añade los campos personalizados
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Double State Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(m_currentState, new GUIContent("Initial state"));
        EditorGUILayout.PropertyField(m_applyProhibitedIcon, new GUIContent("Is boolean state button"));
        EditorGUILayout.PropertyField(m_firstStateSprite, new GUIContent("First State Sprite"));
        EditorGUILayout.LabelField("In prohibited state, the second state will be the prohibited");
        
        if (!m_applyProhibitedIcon.boolValue)
            EditorGUILayout.PropertyField(m_secondStateSprite, new GUIContent("Second State Sprite"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("State Events", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(onFirstState);
        EditorGUILayout.PropertyField(onSecondState);

        serializedObject.ApplyModifiedProperties();
        
        
    }
}