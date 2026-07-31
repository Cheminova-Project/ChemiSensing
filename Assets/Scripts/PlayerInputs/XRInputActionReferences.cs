using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Player Inputs/XR Input Action References")]
public class XRInputActionReferences : ScriptableObject
{
    [SerializeField] private InputActionProperty m_LeftTriggerPressed = new();
    [SerializeField] private InputActionProperty m_RightTriggerPressed = new();
    
    public InputActionProperty leftTriggerPressed => m_LeftTriggerPressed;
    public InputActionProperty rightTriggerPressed => m_RightTriggerPressed;

    private void OnEnable()
    {
        // Validar que todas las referencias estén inicializadas
        ValidateReferences();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // En el editor, validar cuando cambia algo
        ValidateReferences();
    }
#endif

    private void ValidateReferences()
    {
        if (m_LeftTriggerPressed.action == null)
            Debug.LogWarning($"[XRInputActionReferences] Left Trigger Pressed no está asignado en {name}", this);
        
        if (m_RightTriggerPressed.action == null)
            Debug.LogWarning($"[XRInputActionReferences] Right Trigger Pressed no está asignado en {name}", this);
    }

    public bool AreAllReferencesValid()
    {
        return m_LeftTriggerPressed.action != null &&
               m_RightTriggerPressed.action != null;
    }
}