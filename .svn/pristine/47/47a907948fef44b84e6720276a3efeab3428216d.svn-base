using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Player Inputs/Desktop Input Action References")]
public class DesktopInputActionReferences : ScriptableObject
{
    [Header("Movement")]
    public InputActionProperty m_move; // WASD o flechas

    [Header("Looking")]
    public InputActionProperty m_look; // Delta del ratón para mover la cámara

    [Header("Mouse Inputs")]
    public InputActionProperty m_leftClick;  // Click izquierdo
    public InputActionProperty m_rightClick; // Click derecho
    public InputActionProperty m_middleClick; // Click de la rueda
    public InputActionProperty m_mousePosition; // Posición del cursor
    
    [Header("Keyboard Inputs")]
    public InputActionProperty m_openMenu; // Escape para abrir el menú
    
    public InputActionProperty Move
    {
        get => m_move;
        set => m_move = value;
    }
    public InputActionProperty Look
    {
        get => m_look;
        set => m_look = value;
    }
    public InputActionProperty LeftClick
    {
        get => m_leftClick;
        set => m_leftClick = value;
    }
    public InputActionProperty RightClick
    {
        get => m_rightClick;
        set => m_rightClick = value;
    }
    public InputActionProperty MiddleClick
    {
        get => m_middleClick;
        set => m_middleClick = value;
    }
    public InputActionProperty MousePosition
    {
        get => m_mousePosition;
        set => m_mousePosition = value;
    }
    public InputActionProperty OpenMenu
    {
        get => m_openMenu;
        set => m_openMenu = value;
    }
    
}
