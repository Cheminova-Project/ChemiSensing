using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIDocPlayerTypeHelper : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private PanelSettings _3DPanelSettings;
    [SerializeField] private PanelSettings _2DPanelSettings;
    [SerializeField] private List<Component> behavioursToDisableIn2D;

    private void Awake()
    {
        // Configurar el UIDocument antes de que se inicialice completamente
        ConfigureUIDocument();
    }

    void Start()
    {
        // Verificar que la configuración se aplicó correctamente
        VerifyConfiguration();
    }

    private void ConfigureUIDocument()
    {
        var platformController = Unity.Netcode.PlatformController.Instance;
        if (platformController == null)
        {
            Debug.LogError("PlatformController no está inicializado en Awake. Reintentando en Start...");
            return;
        }

        var playerType = platformController.GetPlayerCharacterType();

        if (uiDocument == null)
        {
            Debug.LogError("UIDocument no está asignado.");
            return;
        }

        // Configurar el panel settings ANTES de que el UIDocument se active por completo
        if (playerType == PlayerCharacterType.VR)
        {
            uiDocument.panelSettings = _3DPanelSettings;
            
            // Para VR, asegurar que los behaviours 2D estén habilitados
            SetBehavioursEnabled(true);
        }
        else
        {
            uiDocument.panelSettings = _2DPanelSettings;
            
            // Para 2D, deshabilitar behaviours específicos
            SetBehavioursEnabled(false);
        }

        // Asegurar que el UIDocument esté activo desde el principio
        uiDocument.gameObject.SetActive(true);
    }

    private void VerifyConfiguration()
    {
        // Si no se pudo configurar en Awake, intentar de nuevo en Start
        if (uiDocument != null && uiDocument.panelSettings == null)
        {
            Debug.LogWarning("UIDocument no tenía panelSettings configurado. Reintentando configuración...");
            ConfigureUIDocument();
        }
    }

    private void SetBehavioursEnabled(bool enabled)
    {
        if (behavioursToDisableIn2D == null) return;

        foreach (var behaviour in behavioursToDisableIn2D)
        {
            if (behaviour != null)
            {
                if (behaviour is Behaviour b)
                    b.enabled = enabled;
                else
                    behaviour.gameObject.SetActive(enabled);
            }
        }
    }
}