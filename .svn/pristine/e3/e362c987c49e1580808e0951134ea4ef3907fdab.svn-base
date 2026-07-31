using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class GyroscopeController : ToolComponent
{
    Button gyroResetButton;
    MobileFirstPersonController mobileFirstPersonController;

    private void Start()
    {
        // Solo buscamos el botón de reset normal
        gyroResetButton = uIDocument.rootVisualElement.Q<Button>("reset-button");

        if (gyroResetButton != null)
            gyroResetButton.clicked += OnGyroResetButtonClicked;
    }

    protected override void OnToolActivatedInternal()
    {
        // Lógica de conexión del Player al activar la herramienta
        if (mobileFirstPersonController == null && NetworkManager.Singleton != null)
        {
            GameObject playerObject = LocalRegistry.Instance.GetPlayerGameObject(NetworkManager.Singleton.LocalClientId);
            mobileFirstPersonController = playerObject.GetComponentInChildren<MobileFirstPersonController>();
        }

        if (mobileFirstPersonController != null)
        {
            mobileFirstPersonController.EnableCameraMovement(true);
            mobileFirstPersonController.isGyroEnabled = true;
        }
    }

    private void OnGyroResetButtonClicked()
    {
        // Función del botón físico de reset (clic normal)
        if (mobileFirstPersonController != null)
        {
            mobileFirstPersonController.ResetCameraPosition();
        }
    }

    protected override void OnToolDeactivatedInternal()
    {
        // Apagamos todo al desactivar la herramienta
        if (mobileFirstPersonController != null)
        {
            mobileFirstPersonController.isGyroEnabled = false;
            
            if (!mobileFirstPersonController.isJoystickEnabled)
                mobileFirstPersonController.EnableCameraMovement(false);
        }
    }
    
    private void OnDestroy()
    {
        // Limpiamos el evento del botón
        if (gyroResetButton != null)
        {
            gyroResetButton.clicked -= OnGyroResetButtonClicked;
            gyroResetButton = null;
        }
        
        // Garantiza que al destruir el script, la cámara se bloquee
        OnToolDeactivatedInternal(); 
    }
}