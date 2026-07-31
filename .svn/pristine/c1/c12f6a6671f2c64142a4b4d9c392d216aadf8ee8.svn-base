using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] protected InputActionReference moveAction;
    [SerializeField] protected InputActionReference lookAction;
    [SerializeField] protected InputActionReference flyAction;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] protected float lookSensitivity = 2f;
    [SerializeField] protected float verticalLookLimit = 85f;
    [SerializeField] protected Transform rotationPivot;
    
    [Header("References")]
    [SerializeField] protected Transform BodyTransform;
    [SerializeField] protected Transform HeadTransform;
    [SerializeField] private CharacterController characterController;
    
    protected float verticalLookRotation = 0f;
    protected Vector2 moveInput;
    protected Vector3 lookInput;
    protected float flyInput;
    protected bool cameraMovementAllowed;
    protected virtual void Awake()
    {
        
    }
    
    protected virtual void OnEnable()
    {
        if (moveAction != null && moveAction.action != null)
            moveAction.action.Enable();
        
        if (lookAction != null && lookAction.action != null)
            lookAction.action.Enable();
        
        if (flyAction != null && flyAction.action != null)
            flyAction.action.Enable();
    }

    protected virtual void OnDisable()
    {
        if (moveAction != null && moveAction.action != null)
            moveAction.action.Disable();
        
        if (lookAction != null && lookAction.action != null)
            lookAction.action.Disable();
        
        if (flyAction != null && flyAction.action != null)
            flyAction.action.Disable();
    }

    protected virtual void Start()
    {
        EnableCameraMovement(false);
    }

    protected virtual void Update()
    {
        // Get input
        UpdateLookInput();
        UpdateMoveInput();

        // Apply look
        if(cameraMovementAllowed)
            ApplyLook();

        // Apply movement
        ApplyMovement();
    }
    
    protected virtual void UpdateLookInput()
    {
        if (lookAction != null && lookAction.action != null)
            lookInput = lookAction.action.ReadValue<Vector2>();
    }

    protected virtual void UpdateMoveInput()
    {
        if (moveAction != null && moveAction.action != null)
            moveInput = moveAction.action.ReadValue<Vector2>();
            
        if (flyAction != null && flyAction.action != null)
            flyInput = flyAction.action.ReadValue<float>();
    }
    
    protected virtual void ApplyLook()
    {
        float rotX = lookInput.x * lookSensitivity;
        float rotY = lookInput.y * lookSensitivity;

        verticalLookRotation -= rotY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -verticalLookLimit, verticalLookLimit);

        // Rotación vertical de la cámara (pitch)
        HeadTransform.localEulerAngles = new Vector3(verticalLookRotation, 0f, 0f);

        // Rotación horizontal alrededor del pivote (yaw)
        BodyTransform.Rotate(Vector3.up * rotX);
    }

    protected void ApplyMovement()
    {
        Vector3 move = (transform.right * moveInput.x) + (transform.forward * moveInput.y) + (transform.up * flyInput);
        characterController.Move(move * moveSpeed * Time.deltaTime);
    }
    
    public virtual void EnableCameraMovement(bool enable)
    {
        if (enable)
        {
            cameraMovementAllowed = true;
            lookAction.action.Enable();
        }
        else
        {
            cameraMovementAllowed = false;
            lookAction.action.Disable();
        }
    }
    
    public void ForcePitchRotation(float targetPitch)
    {
        if (targetPitch > 180f) 
            targetPitch -= 360f;
        
        verticalLookRotation = Mathf.Clamp(targetPitch, -verticalLookLimit, verticalLookLimit);
        
        HeadTransform.localEulerAngles = new Vector3(verticalLookRotation, 0f, 0f);
    }
}