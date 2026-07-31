using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class InputActionListener : MonoBehaviour
{
    public List<InputActionProperty> inputActions;
    public UnityEvent onActionPerformed;
    private void OnEnable()
    {
        foreach (var actionProperty in inputActions)
        {
            actionProperty.action.Enable();
            actionProperty.action.performed += OnActionPerformed;
        }
    }

    private void OnDisable()
    {
        foreach (var actionProperty in inputActions)
        {
            actionProperty.action.performed -= OnActionPerformed;
            actionProperty.action.Disable();
        }
    }

    private void OnActionPerformed(InputAction.CallbackContext context)
    {
        onActionPerformed?.Invoke();
    }
}