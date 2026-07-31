#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Profile;

/// <summary>
/// ScriptableObject que define un perfil de compilación para diferentes plataformas.
/// Solo disponible en el editor de Unity.
/// </summary>
[CreateAssetMenu(fileName = "BuildTargetProfile", menuName = "Build/BuildTargetProfile", order = 1)]
public class BuildTargetProfile : ScriptableObject
{
    /// <summary>
    /// Tipo de build (Debug, Release, etc.)
    /// </summary>
    public BuildType BuildType;
    
    /// <summary>
    /// Perfil de build de Unity asociado
    /// </summary>
    public BuildProfile buildProfile;
    
    /// <summary>
    /// Plataforma objetivo (Windows, Android, etc.)
    /// </summary>
    public BuildTarget buildTarget;
    
    /// <summary>
    /// Indica si este perfil está habilitado para compilación
    /// </summary>
    public bool isEnabled = false;
}

#endif