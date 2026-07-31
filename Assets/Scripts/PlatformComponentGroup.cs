using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Estructura que asocia un PlayerCharacterType con una lista de componentes
/// </summary>
[System.Serializable]
public class PlatformComponentGroup
{
    [Header("Platform Configuration")]
    public PlayerCharacterType platformType;
    
    [Header("Components to Manage")]
    [Tooltip("Components that will be enabled/disabled for this platform")]
    public List<Component> components = new List<Component>();
    
    [Header("Advanced Options")]
    [Tooltip("If true, components will be destroyed instead of disabled when not active")]
    public bool destroyInsteadOfDisable = false;
    
    [Tooltip("If true, this platform group will be processed")]
    public bool isEnabled = true;

    /// <summary>
    /// Activa todos los componentes de este grupo
    /// </summary>
    public void EnableComponents()
    {
        if (!isEnabled) return;

        foreach (var component in components)
        {
            if (component != null)
            {
                if (component is MonoBehaviour monoBehaviour)
                {
                    monoBehaviour.enabled = true;
                }
                else if (component is Renderer renderer)
                {
                    renderer.enabled = true;
                }
                else if (component is Collider collider)
                {
                    collider.enabled = true;
                }
                // Agregar más tipos según necesidad
            }
        }
    }

    /// <summary>
    /// Desactiva todos los componentes de este grupo
    /// </summary>
    public void DisableComponents(List<Component> componentsToIgnore = null)
    {
        if (!isEnabled) return;

        foreach (var component in components)
        {
            if (component != null && (componentsToIgnore == null || !componentsToIgnore.Contains(component)))
            {
                if (destroyInsteadOfDisable)
                {
                    DestroyComponent(component);
                }
                else
                {
                    DisableComponent(component);
                }
            }
        }
    }

    private void DisableComponent(Component component)
    {
        if (component is MonoBehaviour monoBehaviour)
        {
            monoBehaviour.enabled = false;
        }
        else if (component is Renderer renderer)
        {
            renderer.enabled = false;
        }
        else if (component is Collider collider)
        {
            collider.enabled = false;
        }
        // Agregar más tipos según necesidad
    }

    private void DestroyComponent(Component component)
    {
        if (Application.isPlaying)
        {
            UnityEngine.Object.Destroy(component);
        }
        else
        {
            UnityEngine.Object.DestroyImmediate(component);
        }
    }

    /// <summary>
    /// Valida si este grupo corresponde al tipo de plataforma especificado
    /// </summary>
    public bool IsValidForPlatform(PlayerCharacterType currentPlatform)
    {
        return isEnabled && platformType == currentPlatform;
    }

    /// <summary>
    /// Cuenta los componentes válidos (no null) en este grupo
    /// </summary>
    public int GetValidComponentCount()
    {
        int count = 0;
        foreach (var component in components)
        {
            if (component != null) count++;
        }
        return count;
    }
}