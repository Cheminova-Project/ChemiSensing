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
        gyroResetButton = uIDocument.rootVisualElement.Q<Button>("reset-button");

        if (gyroResetButton != null)
            gyroResetButton.clicked += OnGyroResetButtonClicked;
    }

    protected override void OnToolActivatedInternal()
    {
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
        if (mobileFirstPersonController != null)
            mobileFirstPersonController.ResetCameraPosition();
    }

    protected override void OnToolDeactivatedInternal()
    {
        if (mobileFirstPersonController != null)
        {
            mobileFirstPersonController.isGyroEnabled = false;
            
            if (!mobileFirstPersonController.isJoystickEnabled)
                mobileFirstPersonController.EnableCameraMovement(false);
        }
    }
    
    private void OnDestroy()
    {
        if (gyroResetButton != null)
        {
            gyroResetButton.clicked -= OnGyroResetButtonClicked;
            gyroResetButton = null;
        }
        
        OnToolDeactivatedInternal(); 
    }
}