using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Manager que gestiona la activación/desactivación de componentes según el tipo de plataforma
/// </summary>
public class PlatformToolsManager : MonoBehaviour
{
    [Header("Platform Management")]
    [SerializeField] private List<PlatformComponentGroup> platformGroups = new List<PlatformComponentGroup>();

    [Header("Settings")]
    [SerializeField] private bool autoDetectPlatform = true;
    [SerializeField] private bool updateOnPlatformChange = true;
    [SerializeField] private PlayerCharacterType forcedPlatform = PlayerCharacterType.Desktop;

    private PlayerCharacterType currentPlatform;
    private bool isInitialized = false;

    /// <summary>
    /// Plataforma actual detectada o forzada
    /// </summary>
    public PlayerCharacterType CurrentPlatform => currentPlatform;

    /// <summary>
    /// Lista de grupos de plataforma configurados
    /// </summary>
    public IReadOnlyList<PlatformComponentGroup> PlatformGroups => platformGroups.AsReadOnly();

    private void Awake()
    {
        InitializePlatformManager();
    }

    private void OnValidate()
    {
        // Validar configuración en el editor
        if (platformGroups != null)
        {
            // Remover grupos con plataformas duplicadas
            var duplicateGroups = platformGroups
                .GroupBy(g => g.platformType)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g.Skip(1));

            foreach (var duplicate in duplicateGroups.ToList())
                platformGroups.Remove(duplicate);
        }
    }

    /// <summary>
    /// Inicializa el manager detectando la plataforma y configurando componentes
    /// </summary>
    public void InitializePlatformManager()
    {
        if (isInitialized) return;

        DetectCurrentPlatform();
        ApplyPlatformConfiguration();

        if (updateOnPlatformChange)
        {
            // Suscribirse a cambios de plataforma si es necesario
            // Esto dependerá de cómo implementes la detección de cambios en tu sistema
        }

        isInitialized = true;
    }

    /// <summary>
    /// Detecta la plataforma actual
    /// </summary>
    private void DetectCurrentPlatform()
    {
        if (autoDetectPlatform)
        {
            currentPlatform = PlatformController.Instance != null
                ? PlatformController.Instance.GetPlayerCharacterType()
                : PlayerCharacterType.Desktop;
        }
        else
        {
            currentPlatform = forcedPlatform;
        }
    }

    /// <summary>
    /// Aplica la configuración de componentes para la plataforma actual
    /// </summary>
    public void ApplyPlatformConfiguration()
    {
        if (platformGroups == null || platformGroups.Count == 0)
            return;

        // Encontrar el grupo para la plataforma actual
        var currentPlatformGroup = platformGroups.FirstOrDefault(g => g.IsValidForPlatform(currentPlatform));

        if (currentPlatformGroup == null)
        {
            // Desactivar todos los grupos si no hay uno específico para la plataforma actual
            foreach (var group in platformGroups)
                group.DisableComponents();
            
            return;
        }

        // Activar solo los componentes del grupo de la plataforma actual
        currentPlatformGroup.EnableComponents();
        
        // Desactivar/destruir componentes de todas las demás plataformas
        //No tenía sentido cómo se hacía, porque si queríacompartir un mismo script entre dos plataformas distintas, se desactivaba en ambas.
        int deactivatedComponents = 0;
        foreach (var group in platformGroups)
        {
            if (group != currentPlatformGroup)
            {
                int beforeCount = group.GetValidComponentCount();
                //Podemos pasarle los componentes que no debe desactivar
                group.DisableComponents(currentPlatformGroup.components);
                deactivatedComponents += beforeCount;
            }
        }
    }

    /// <summary>
    /// Cambia manualmente la plataforma y reaplica la configuración
    /// </summary>
    public void ChangePlatform(PlayerCharacterType newPlatform)
    {
        if (currentPlatform == newPlatform)
            return;

        currentPlatform = newPlatform;
        autoDetectPlatform = false;

        ApplyPlatformConfiguration();
    }

    /// <summary>
    /// Agrega un nuevo grupo de plataforma programáticamente
    /// </summary>
    public void AddPlatformGroup(PlayerCharacterType platformType, List<Component> components, bool destroyInsteadOfDisable = false)
    {
        // Verificar si ya existe un grupo para esta plataforma
        var existingGroup = platformGroups.FirstOrDefault(g => g.platformType == platformType);
        if (existingGroup != null)
        {
            existingGroup.components.AddRange(components);
            return;
        }

        // Crear nuevo grupo
        var newGroup = new PlatformComponentGroup
        {
            platformType = platformType,
            components = new List<Component>(components),
            destroyInsteadOfDisable = destroyInsteadOfDisable,
            isEnabled = true
        };

        platformGroups.Add(newGroup);

        // Reaplican configuración si ya estamos inicializados
        if (isInitialized)
            ApplyPlatformConfiguration();
    }

    /// <summary>
    /// Obtiene el grupo de componentes para una plataforma específica
    /// </summary>
    public PlatformComponentGroup GetPlatformGroup(PlayerCharacterType platformType)
    {
        return platformGroups.FirstOrDefault(g => g.platformType == platformType);
    }

    /// <summary>
    /// Refresca la configuración (útil para llamar desde el editor)
    /// </summary>
    [ContextMenu("Refresh Platform Configuration")]
    public void RefreshConfiguration()
    {
        DetectCurrentPlatform();
        ApplyPlatformConfiguration();
    }

    /// <summary>
    /// Limpia componentes null de todos los grupos
    /// </summary>
    [ContextMenu("Clean Null Components")]
    public void CleanNullComponents()
    {
        int removedCount = 0;

        foreach (var group in platformGroups)
        {
            var initialCount = group.components.Count;
            group.components.RemoveAll(c => c == null);
            removedCount += initialCount - group.components.Count;
        }
    }
}