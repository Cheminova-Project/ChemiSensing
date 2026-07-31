using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Controlador de herramientas para plataformas de escritorio.
/// Gestiona la activación y uso de herramientas específicas en desktop.
/// </summary>
public class DesktopToolsController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DesktopRaycaster desktopRaycaster;
    [SerializeField] private Ruler ruler;
    [SerializeField] private TextureManagerUI textureManagerUI;
    [Header("Buttons")]
    [SerializeField] private Button manipulationButton;
    [SerializeField] private Button exitManipulationButton;
    [SerializeField] private DoubleStateButton laserButton;
    [SerializeField] private DoubleStateButton rulerButton;
    [SerializeField] private DoubleStateButton textureButton;
    [Header("UI")]
    [SerializeField] private GameObject MainControlButtons;
    [SerializeField] private GameObject manipulationToolsUI;
    
    private void OnEnable()
    {
        manipulationButton.onClick.AddListener(EnableManipulationTools);
        exitManipulationButton.onClick.AddListener(DisableManipulationTools);
        laserButton.onFirstState.AddListener(EnableLaser);
        laserButton.onSecondState.AddListener(DisableLaser);
        rulerButton.onFirstState.AddListener(EnableRuler);
        rulerButton.onSecondState.AddListener(DisableRuler);
        textureButton.onFirstState.AddListener(EnableTextureManager);
        textureButton.onSecondState.AddListener(DisableTextureManager);
        InitMenu();
    }

    private void EnableTextureManager()
    {
        EnableTextureManager(true);
    }

    private void DisableTextureManager()
    {
        EnableTextureManager(false);
    }
    
    
    
    private void EnableTextureManager(bool enableTM)
    {
   
    }
    
    private void OnDisable()
    {
        manipulationButton.onClick.RemoveListener(EnableManipulationTools);
        exitManipulationButton.onClick.RemoveListener(DisableManipulationTools);
        laserButton.onFirstState.RemoveListener(EnableLaser);
        laserButton.onSecondState.RemoveListener(DisableLaser);
        rulerButton.onFirstState.RemoveListener(EnableRuler);
        rulerButton.onSecondState.RemoveListener(DisableRuler);
        textureButton.onFirstState.RemoveListener(EnableTextureManager);
        textureButton.onSecondState.RemoveListener(DisableTextureManager);
    }

    private void InitMenu()
    {
        OpenMainButtons(true);
        OpenManipulationTools(false);
    }


    private void EnableLaser()
    {
        EnableLaser(true);
    }

    private void DisableLaser()
    {
        EnableLaser(false);
    }
    
    public void EnableLaser(bool enableL)
    {
        desktopRaycaster.EnableLaser(enableL);
        laserButton.SetState(enableL);
        if (enableL)
        {
            DisableRuler();
            DisableManipulationTools();
        }
    }

    private void DisableRuler()
    {
        EnableRuler(false);
    }

    private void EnableRuler()
    {
        EnableRuler(true);
    }

    private void EnableRuler(bool enableR)
    {
        rulerButton.SetState(enableR);
        if (enableR)
        {
            DisableLaser();
            DisableManipulationTools();
        }
    }


    public void EnableManipulationTools(bool enableMT)
    {
        desktopRaycaster.DeselectCurrentTarget();
        OpenManipulationTools(enableMT);
        if (enableMT)
        {
            desktopRaycaster.SelectInspectedObject();
            DisableLaser();
            DisableRuler();
        }
    }
    
    public void EnableManipulationTools()
    {
        EnableManipulationTools(true);
    }
    public void DisableManipulationTools()
    {
        EnableManipulationTools(false);
    }
    public void OpenManipulationTools(bool open)
    {
        manipulationToolsUI.SetActive(open);
        OpenMainButtons(!open);
    }

    public void OpenMainButtons(bool open)
    {
        MainControlButtons.SetActive(open);
    }

    /// <summary>
    /// Activa una herramienta específica por nombre.
    /// </summary>
    /// <param name="toolName">Nombre de la herramienta a activar.</param>
    public void ActivateTool(string toolName)
    {
        // Lógica para activar la herramienta indicada
    }

    /// <summary>
    /// Desactiva todas las herramientas activas.
    /// </summary>
    public void DeactivateAllTools()
    {
        // Lógica para desactivar todas las herramientas
    }
}
