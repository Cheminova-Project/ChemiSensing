using StarterAssets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Permite realizar raycasts desde la cámara en plataformas móviles.
/// Utilizado para interacción con objetos mediante toques en pantalla.
/// </summary>
public class MobileRaycaster : ScreenRaycaster
{
    [Header("Mobile Raycaster Settings")]
    [SerializeField] private MobileFirstPersonController firstPersonController;
    protected override void InitListeners()
    {
        base.InitListeners();
        firstPersonController.onCameraMovementChanged += OnCameraModeChanged;
    }

    private void OnCameraModeChanged(bool _enabled)
    {
        if (_enabled)
            OnCameraMoveStateChanged(true);
        else
            OnCameraMoveStateChanged(false);
    }
}