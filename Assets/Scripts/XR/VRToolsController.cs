using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class VRToolsController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VRRaycaster vrRaycaster;
    [SerializeField] private Ruler ruler;
    [SerializeField] private TextureManagerUI textureManagerUI;
    [Header("Buttons")]
    [SerializeField] private DoubleStateButton laserButton;
    [SerializeField] private DoubleStateButton rulerButton;
    [SerializeField] private DoubleStateButton textureButton;
    
    private void OnEnable()
    {
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
        if (enableTM)
        {
            DisableLaser();
            DisableRuler();

        }
        else
        {

        }
    }
    private void OnDisable()
    {
        laserButton.onFirstState.RemoveListener(EnableLaser);
        laserButton.onSecondState.RemoveListener(DisableLaser);
        rulerButton.onFirstState.RemoveListener(EnableRuler);
        rulerButton.onSecondState.RemoveListener(DisableRuler);
        textureButton.onFirstState.RemoveListener(EnableTextureManager);
        textureButton.onSecondState.RemoveListener(DisableTextureManager);
    }

    private void InitMenu()
    {
        
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
        vrRaycaster.EnableLaser(enableL);
        laserButton.SetState(enableL);
        if (enableL)
        {
            DisableRuler();
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
        }
    }
    
    public void EnableManipulationTools(bool enableMT)
    {
        if (enableMT)
        {
            DisableLaser();
            DisableRuler();
        }
    }
}
