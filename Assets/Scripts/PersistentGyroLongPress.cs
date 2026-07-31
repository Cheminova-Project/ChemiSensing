using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class PersistentGyroLongPress : MonoBehaviour
{
    private UIDocument uiDocument; 
    private VisualElement activationToggle;
    private IVisualElementScheduledItem holdTask;
    
    private bool hasTriggeredReset = false;
    private const float REQUIRED_HOLD_TIME = 3.0f;
    
    private bool isInitialized = false;

    private void Update()
    {
        if (!isInitialized)
            TryInitializeUI();
    }

    private void TryInitializeUI()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient) 
            return;

        GameObject playerObject = LocalRegistry.Instance.GetPlayerGameObject(NetworkManager.Singleton.LocalClientId);
        if (playerObject == null) 
            return;
        
        uiDocument = playerObject.GetComponentInChildren<UIDocument>();

        if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            activationToggle = uiDocument.rootVisualElement.Q<VisualElement>("gyroscope");

            if (activationToggle != null)
            {
                // Registramos los eventos
                activationToggle.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
                activationToggle.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
                activationToggle.RegisterCallback<PointerLeaveEvent>(OnPointerLeave, TrickleDown.TrickleDown);
                activationToggle.RegisterCallback<ClickEvent>(OnClick, TrickleDown.TrickleDown);
                
                // Marcamos como inicializado para que el Update no vuelva a ejecutar esto
                isInitialized = true; 
            }
        }
    }

    private void OnDisable()
    {
        // Limpiamos los eventos si se destruye o desactiva este manager
        if (activationToggle != null)
        {
            activationToggle.UnregisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            activationToggle.UnregisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
            activationToggle.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave, TrickleDown.TrickleDown);
            activationToggle.UnregisterCallback<ClickEvent>(OnClick, TrickleDown.TrickleDown);
        }
        isInitialized = false;
    }
    
    private void OnPointerDown(PointerDownEvent evt)
    {
        hasTriggeredReset = false;

        if (activationToggle != null)
        {
            holdTask = activationToggle.schedule.Execute(() => 
            {
                hasTriggeredReset = true;
                TriggerReset();
            }).StartingIn((long)(REQUIRED_HOLD_TIME * 1000));
        }
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (hasTriggeredReset)
        {
            evt.StopPropagation();
            evt.PreventDefault();
        }
        else
        {
            CancelHoldTask();
        }
    }

    private void OnClick(ClickEvent evt)
    {
        if (hasTriggeredReset)
        {
            evt.StopPropagation();
            evt.PreventDefault();
        }
    }

    private void OnPointerLeave(PointerLeaveEvent evt)
    {
        CancelHoldTask();
    }

    private void CancelHoldTask()
    {
        if (holdTask != null)
        {
            holdTask.Pause();
            holdTask = null;
        }
    }
    
    private void TriggerReset()
    {
        if (activationToggle is Toggle toggleUI)
        {
            toggleUI.value = false;
        }

        if (NetworkManager.Singleton != null)
        {
            GameObject playerObject = LocalRegistry.Instance.GetPlayerGameObject(NetworkManager.Singleton.LocalClientId);
            if (playerObject != null)
            {
                var mobileFirstPersonController = playerObject.GetComponentInChildren<MobileFirstPersonController>();
                
                if (mobileFirstPersonController != null)
                {
                    mobileFirstPersonController.ResetCameraPosition();
                    mobileFirstPersonController.isGyroEnabled = false;
                    
                    if (!mobileFirstPersonController.isJoystickEnabled)
                    {
                        mobileFirstPersonController.EnableCameraMovement(false);
                    }
                }
            }
        }
    }
}