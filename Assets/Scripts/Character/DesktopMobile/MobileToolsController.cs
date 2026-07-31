using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MobileToolsController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MobileFirstPersonController mobileFirstPersonController;
    [SerializeField] private UIVirtualJoystick virtualJoystick;
    [SerializeField] private MobileRaycaster mobileRaycaster;
    [SerializeField] private ScreenRuler mobileRuler;
    [SerializeField] private TextureManagerUI textureManagerUI;
    [Header("Buttons")]
    [SerializeField] private DoubleStateButton cameraMovementButton;
    [SerializeField] private DoubleStateButton joystickButton;
    [SerializeField] private DoubleStateButton laserButton;
    [SerializeField] private DoubleStateButton rulerButton;
    [SerializeField] private Button manipulationButton;
    [SerializeField] private Button exitManipulationButton;
    [SerializeField] private DoubleStateButton textureButton;
    [Header("Joystick variables")]
    [SerializeField] private RectTransform joystickSafeArea;
    [SerializeField] private PointerInputActionReferences m_pointerInputActionReferences;
    
    [Header("Bottom menu management")]
    [SerializeField] private GameObject MainControlButtons;
    [SerializeField] private GameObject ManipulationButtons;
    private void OnEnable()
    {
        if (mobileFirstPersonController == null)
        {
            Debug.LogError("MobileFirstPersonController is not assigned.");
            return;
        }

        if (virtualJoystick == null)
        {
            Debug.LogError("UIVirtualJoystick is not assigned.");
            return;
        }

        if (mobileRaycaster == null)
        {
            Debug.LogError("MobileRaycaster is not assigned.");
            return;
        }
        
        if (mobileRuler == null)
        {
            Debug.LogError("MobileRuler is not assigned.");
            return;
        }

        // Initialize buttons
        cameraMovementButton.onFirstState.AddListener(DisableCameraMovement);
        cameraMovementButton.onSecondState.AddListener(EnableCameraMovement);
        
        joystickButton.onFirstState.AddListener(EnableJoystick);
        joystickButton.onSecondState.AddListener(DisableJoystick);
        
        laserButton.onFirstState.AddListener(EnableLaser);
        laserButton.onSecondState.AddListener(DisableLaser);
        
        rulerButton.onFirstState.AddListener(EnableRuler);
        rulerButton.onSecondState.AddListener(DisableRuler);
        
        manipulationButton.onClick.AddListener(EnableManipulation);
        exitManipulationButton.onClick.AddListener(DisableManipulation);
        
        textureButton.onFirstState.AddListener(EnableTextureManager);
        textureButton.onSecondState.AddListener(DisableTextureManager);
        m_pointerInputActionReferences.m_pointerPress.action.performed += OnPointerPressed;
        m_pointerInputActionReferences.m_pointerPress.action.canceled += OnPointerPressed;

        EnableJoystick(false);
        
        InitMenu();
    }
    

    private void OnDisable()
    {
        // Unsubscribe from button events
        cameraMovementButton.onFirstState.RemoveListener(DisableCameraMovement);
        cameraMovementButton.onSecondState.RemoveListener(EnableCameraMovement);
        
        joystickButton.onFirstState.RemoveListener(DisableJoystick);
        joystickButton.onSecondState.RemoveListener(EnableJoystick);
        
        laserButton.onFirstState.RemoveListener(DisableLaser);
        laserButton.onSecondState.RemoveListener(EnableLaser);
        
        rulerButton.onFirstState.RemoveListener(DisableRuler);
        rulerButton.onSecondState.RemoveListener(EnableRuler);
        
        manipulationButton.onClick.RemoveListener(EnableManipulation);
        exitManipulationButton.onClick.RemoveListener(DisableManipulation);
        
        textureButton.onFirstState.RemoveListener(EnableTextureManager);
        textureButton.onSecondState.RemoveListener(DisableTextureManager);
        
        m_pointerInputActionReferences.m_pointerPress.action.performed -= OnPointerPressed;
        m_pointerInputActionReferences.m_pointerPress.action.canceled -= OnPointerPressed;
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
    
    public void DisableRuler()
    {
        EnableRuler(false);
    }
    
    public void EnableRuler()
    {
        EnableRuler(true);
    }

    public void EnableRuler(bool enableRuler)
    {
        rulerButton.SetState(enableRuler);
        if (enableRuler)
        {
            DisableJoystick();
            DisableLaser();
            DisableManipulation();
        }
    }
    
    private void DisableManipulation()
    {
        mobileRaycaster.DeselectCurrentTarget();
        OpenManipulationButtons(false);
    }

    private void EnableManipulation()
    {
        EnableLaser(false);
        EnableJoystick(false);
        mobileRaycaster.SelectInspectedObject();
        OpenManipulationButtons(true);
    }

    
    private void DisableLaser()
    {
        EnableLaser(false);
    }
    
    private void EnableLaser()
    {
        EnableLaser(true);
    }
    private void EnableLaser(bool enable)
    {
        mobileRaycaster.EnableLaser(enable);
        laserButton.SetState(enable);
        if (enable)
        {
            DisableJoystick();
            DisableManipulation();
            DisableRuler();
        }
    }
    private void EnableJoystick()
    {
        EnableJoystick(true);
    }

    private void DisableJoystick()
    {
        EnableJoystick(false);
    }

    private void EnableJoystick(bool enable)
    {
        virtualJoystick.EnableJoystick(enable);
        joystickButton.SetState(enable);
        if (enable)
        {
            DisableLaser();
            DisableManipulation();
            DisableRuler();
        }
    }

    private void EnableCameraMovement()
    {
        OnCameraMovement(true);
    }
    
    private void DisableCameraMovement()
    {
        OnCameraMovement(false);
    }
    private void OnCameraMovement(bool enable)
    {
        if (mobileFirstPersonController == null)
        {
            Debug.LogError("MobileFirstPersonController is not assigned.");
            return;
        }

        mobileFirstPersonController.EnableCameraMovement(enable);
    }
    
    private void OnPointerPressed(InputAction.CallbackContext pointerEvent)
    {
        if (pointerEvent.phase == InputActionPhase.Performed || pointerEvent.phase == InputActionPhase.Started)
        {
            var pointerpos = m_pointerInputActionReferences.PointerPosition.action.ReadValue<Vector2>();
            if(IsPointInsideRect(joystickSafeArea, pointerpos))
                OnPointerDown(pointerpos);
        }
        else if (pointerEvent.phase == InputActionPhase.Canceled || pointerEvent.phase == InputActionPhase.Disabled)
        {
            OnPointerUp();
        }
    }
    
    bool IsPointInsideRect(RectTransform rectTransform, Vector2 screenPoint)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPoint);
    }
    
    
    public void OnPointerDown(Vector2 pointerPressPosition)
    {
        if(!virtualJoystick.IsEnabled())
            return;
        virtualJoystick.transform.position = pointerPressPosition;
        
        virtualJoystick.ShowJoystick(true);
        
        virtualJoystick.OnPointerDown(new PointerEventData(EventSystem.current)
        {
            position = pointerPressPosition
        });
    }
    
    public void OnPointerUp()
    {
        if(!virtualJoystick.IsEnabled())
            return;
        virtualJoystick.ShowJoystick(false);
    }

    #region "Bottom menu management"

    public void InitMenu()
    {
        MainControlButtons.SetActive(true);
        ManipulationButtons.SetActive(false);
    }

    public void OpenManipulationButtons(bool open = true)
    {
        MainControlButtons.SetActive(!open);
        ManipulationButtons.SetActive(open);
    }


    #endregion "Bottom menu management"
    
}
