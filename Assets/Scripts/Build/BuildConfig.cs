
using UnityEngine;
using Unity.Collections;

#if UNITY_EDITOR
using UnityEditor;

[CreateAssetMenu(fileName = "BuildSettings", menuName = "Build/Build Settings")]
#endif

/// <summary>
/// Configuración de build para la aplicación.
/// Permite seleccionar el tipo de build (Desktop, AR, VR).
/// </summary>
public class BuildConfig : ScriptableObject
{
    /// <summary>
    /// Tipo de build disponible.
    /// </summary>
    [SerializeField] private BuildType currentBuildType = BuildType.Desktop;

    /// <summary>
    /// Obtiene o establece el tipo de build actual.
    /// </summary>
    public BuildType CurrentBuildType
    {
        get => currentBuildType;
        set
        {
            if (currentBuildType != value)
            {
                currentBuildType = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
        }
    }
}