using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Gyroscope = UnityEngine.InputSystem.Gyroscope;

public class MobileFirstPersonController : FirstPersonController
{
    [Header("Mobile Settings")]
    
    public UnityAction<bool> onCameraMovementChanged;
    private Vector3 currentAngularVelocity = Vector3.zero;
    private Vector2 temporalMoveInput;
    private Vector2 touchLookInput; 
    private Gyroscope gyroscopeSensor;
    private Quaternion accumulatedRotation = Quaternion.identity;
    public bool isGyroEnabled = false; 
    public bool isJoystickEnabled = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        isGyroEnabled = false;
        isJoystickEnabled = false;
        StartCoroutine("FindSensorDelay");
    }

    private IEnumerator FindSensorDelay()
    {
        yield return new WaitForSeconds(3f);
        gyroscopeSensor = Gyroscope.current;
        
        if (gyroscopeSensor != null)
        {
            yield return new WaitForSeconds(1f);
            InputSystem.EnableDevice(gyroscopeSensor);
            yield return new WaitForSeconds(1f);
            if(gyroscopeSensor.enabled)
            {
                //Debug.Log("Gyroscope habilitado: " + gyroscopeSensor.name);
            }
            else
            {
                Debug.LogWarning("Gyroscope sensor is not enabled after enabling it.");
            }
        }
        else
        {
            Debug.LogError("No se encontró ningún Gyroscope en este dispositivo.");
            // Si no hay giroscopio físico, podemos forzar el apagado de la opción
            isGyroEnabled = false; 
        }
        yield return null;
    }

    public void InputLook(Vector2 input)
    {
        touchLookInput = input;
    }

    protected override void UpdateLookInput()
    {
        Vector3 gyroInput = Vector3.zero;
        Vector3 touchInputCalculated = Vector3.zero;

        // 1. Calcular Input del Giroscopio (SOLO si está activado el bool y el sensor funciona)
        if(isGyroEnabled && gyroscopeSensor != null && gyroscopeSensor.enabled)
        {
            currentAngularVelocity = gyroscopeSensor.angularVelocity.ReadValue();

            currentAngularVelocity /= 2f;
            
            switch (Screen.orientation)
            {
                case ScreenOrientation.LandscapeLeft:
                    gyroInput = new Vector3(-currentAngularVelocity.y, -currentAngularVelocity.x, 0);
                    break;
                case ScreenOrientation.LandscapeRight:
                    gyroInput = new Vector3(-currentAngularVelocity.y, currentAngularVelocity.x, 0);
                    break;
                case ScreenOrientation.Portrait:
                    gyroInput = new Vector3(currentAngularVelocity.x, -currentAngularVelocity.y, 0);
                    break;
                default: 
                    gyroInput = new Vector3(-currentAngularVelocity.y, -currentAngularVelocity.x, 0);
                    break;
            }
        }

        // 2. Calcular Input Táctil (SOLO si está activado el bool)
        if (isJoystickEnabled)
        {
            float touchSensitivity = 2.0f; 
            touchInputCalculated = new Vector3(touchLookInput.y, touchLookInput.x, 0) * touchSensitivity;
        }

        // 3. Combinar ambos (si uno está apagado, simplemente sumará Vector3.zero)
        lookInput = gyroInput + touchInputCalculated;
    }
    
    protected override void UpdateMoveInput()
    {
        moveInput = temporalMoveInput;
    }
 
    public void InputMove(Vector2 input)
    {
        temporalMoveInput = input;
    }

    protected override void ApplyLook()
    {
        bool hasActiveInputMethod = isGyroEnabled || isJoystickEnabled;

        if (cameraMovementAllowed && hasActiveInputMethod)
        {
            Vector3 deltaRotation = lookInput * Time.deltaTime * Mathf.Rad2Deg;
            
            Vector3 headRotation = new Vector3(deltaRotation.x, 0f, 0f);   
            Vector3 bodyRotation = new Vector3(0f, deltaRotation.y, 0f);   
            
            HeadTransform.Rotate(headRotation, Space.Self);
            BodyTransform.Rotate(bodyRotation, Space.Self);
        }
    }

    public override void EnableCameraMovement(bool enable)
    {
        base.EnableCameraMovement(enable);
        onCameraMovementChanged?.Invoke(enable);
    }

    public void ChangeCameraState()
    {
        EnableCameraMovement(!cameraMovementAllowed);
    }

    public void ResetCameraPosition()
    {
        HeadTransform.localRotation = Quaternion.identity;
        //BodyTransform.localRotation = Quaternion.identity;
    }
    
    private Quaternion ConvertGyroToUnity(Quaternion deviceAttitudeQuaternion)
    {
        Quaternion screenRotationCompensation = Quaternion.identity;
        switch (Screen.orientation)
        {
            case ScreenOrientation.LandscapeLeft:
                screenRotationCompensation = Quaternion.Euler(0, 0, -90); 
                break;
            case ScreenOrientation.LandscapeRight:
                screenRotationCompensation = Quaternion.Euler(0, 0, 90); 
                break;
            case ScreenOrientation.PortraitUpsideDown:
                screenRotationCompensation = Quaternion.Euler(0, 0, 180); 
                break;
            case ScreenOrientation.Portrait:
            default:
                break;
        }
        return deviceAttitudeQuaternion * screenRotationCompensation;
    }
}