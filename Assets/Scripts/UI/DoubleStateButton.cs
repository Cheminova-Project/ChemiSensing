using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

[RequireComponent(typeof(Image))]
public class DoubleStateButton : Button
{
    public enum ButtonState
    {
        First,
        Second
    }

    [Header("Double State Sprites")]
    [SerializeField] private bool m_isBooleanButton = false;
    [SerializeField] private Sprite m_firstStateSprite;

    [SerializeField] private Sprite m_secondStateSprite;

    [Header("Options")]
    [SerializeField] private ButtonState m_currentState = ButtonState.First;
    
    [Header("Optional Events")]
    public UnityEvent onFirstState;
    public UnityEvent onSecondState;

    private Image m_image;

    protected override void Start()
    {
        base.Start();
        if (!Application.isPlaying)
            return;

        m_image = GetComponent<Image>();
        onClick.AddListener(OnButtonClick);
        UpdateVisual();
    }
    
    private void OnButtonClick()
    {
        m_currentState = m_currentState == ButtonState.First ? ButtonState.Second : ButtonState.First;
        
        if (m_currentState == ButtonState.First)
            onFirstState?.Invoke();
        else
            onSecondState?.Invoke();

        UpdateVisual();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData); // Mantiene el comportamiento original del botón
    }

    private void UpdateVisual()
    {
        if (!m_image) m_image = GetComponent<Image>();

        if (m_isBooleanButton)
        {
            if(m_currentState == ButtonState.Second)
                m_image.color = new Color(m_image.color.r, m_image.color.g, m_image.color.b, 0.5f);
            else
                m_image.color = new Color(m_image.color.r, m_image.color.g, m_image.color.b, 1f);
            
            return;
        }
        
        m_image.sprite = m_currentState == ButtonState.First
            ? m_firstStateSprite
            : m_secondStateSprite;
    }

    // Métodos públicos para leer y forzar estado
    public bool IsSecondState() => m_currentState == ButtonState.Second;
    
    public bool IsDisabledState() => m_isBooleanButton && m_currentState == ButtonState.Second;
    
    public void SetState(ButtonState buttonState)
    {
        m_currentState = buttonState;

        UpdateVisual();
    }
    
    public void SetState(bool allowed)
    {
        ButtonState calculatedState = allowed ? ButtonState.First : ButtonState.Second;
        
        if (m_currentState != calculatedState)
        {
            m_currentState = allowed ? ButtonState.First : ButtonState.Second;
            UpdateVisual();
        }
        
    }
}
