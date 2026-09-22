using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Tipos de mensajes que se pueden mostrar en la interfaz de usuario.
/// </summary>
public enum MessageType
{
    Info,
    Error
}

/// <summary>
/// Maneja la visualización de mensajes informativos y de error en la interfaz de usuario.
/// </summary>
public class ToolMessageHandler : MonoBehaviour
{
    public Color infoColor = Color.black;
    public Color errorColor = Color.red;

    string messageBoxName = "message-box";
    string messageLabelName = "message-label";
    UIDocument uIDocument;
    private Coroutine currentMessageCoroutine;

    public static ToolMessageHandler Instance { get; private set; }


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnEnable()
    {
        HideMessage();
    }

    private bool EnsureUIDocument()
    {
        if (uIDocument == null && ToolMenuController.Instance != null)
            uIDocument = ToolMenuController.Instance.GetUIDocument();
        
        return uIDocument != null;
    }
    
    void HideMessage()
    {   
        if (!EnsureUIDocument())
            return;
        
        var root = uIDocument.rootVisualElement;
        var messageBox = root.Q<VisualElement>(messageBoxName);
        
        if (messageBox != null)
            messageBox.style.display = DisplayStyle.None;
    }

    /// <summary>
    /// Muestra un mensaje en la interfaz de usuario durante una duración específica.
    /// </summary>
    public void ShowMessage(string message, float displayDuration = 3f, MessageType messageType = MessageType.Info)
    {
        if (!EnsureUIDocument())
        {
            Debug.LogWarning("ToolMessageHandler: UIDocument is not assigned and ToolMenuController is unavailable.");
            return;
        }

        if (currentMessageCoroutine != null)
            StopCoroutine(currentMessageCoroutine);

        currentMessageCoroutine = StartCoroutine(DisplayMessageCoroutine(message, displayDuration, messageType));
    }

    /// <summary>
    /// Corrutina que maneja la visualización del mensaje.
    /// </summary>
    private IEnumerator DisplayMessageCoroutine(string message, float duration, MessageType messageType)
    {
        var root = uIDocument.rootVisualElement;
        var messageBox = root.Q<VisualElement>(messageBoxName);
        var messageLabel = root.Q<Label>(messageLabelName);

        if (messageBox == null || messageLabel == null)
        {
            Debug.LogWarning("ToolMessageHandler: Message box or label not found in the UI.");
            yield break;
        }

        switch (messageType)
        {
            case MessageType.Info:
                messageLabel.style.color = infoColor;
                break;
            case MessageType.Error:
                messageLabel.style.color = errorColor;
                break;
        }

        messageLabel.text = message;
        messageBox.style.display = DisplayStyle.Flex;

        yield return new WaitForSeconds(duration);

        messageBox.style.display = DisplayStyle.None;
        currentMessageCoroutine = null;
    }
}
