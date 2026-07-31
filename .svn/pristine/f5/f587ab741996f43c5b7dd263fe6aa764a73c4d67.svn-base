using UnityEngine;
using UnityEngine.UIElements;

public class MenuDisconnectHandler : MonoBehaviour
{
    [Header("Referencias UI Toolkit")]
    public UIDocument uiDocument;
    [Tooltip("Arrastra aquí el archivo UXML de tu panel de aviso de desconexión")]
    public VisualTreeAsset disconnectTemplate; 

    [Header("Nombres de Elementos en el UXML")]
    public string panelName = "DisconnectPanelInstance";
    public string messageLabelName = "message";
    public string acceptButtonName = "accept-button";

    private void Start()
    {
        // Comprobamos la variable estática del RoomLifecycleManager
        if (!string.IsNullOrEmpty(ShutdownManager.DisconnectReason))
        {
            // Mostramos el panel pasándole el mensaje
            ShowDisconnectPanel(ShutdownManager.DisconnectReason);
            
            // ¡MUY IMPORTANTE! Vaciamos la variable para no repetir el mensaje al recargar la escena
            ShutdownManager.DisconnectReason = null;
        }
    }

    private void ShowDisconnectPanel(string reasonMessage)
    {
        // Búsqueda de seguridad por si no se asignó en el inspector
        if (uiDocument == null)
            uiDocument = FindAnyObjectByType<UIDocument>();
        
        if (uiDocument == null || disconnectTemplate == null)
        {
            Debug.LogError("[MenuDisconnectHandler] Faltan referencias (UIDocument o el Template UXML).");
            return;
        }

        // 1. Obtener la raíz del documento
        VisualElement root = uiDocument.rootVisualElement;
        
        // Evitar duplicados si ya hay un panel con ese nombre abierto
        if (root.Q(panelName) != null) return;

        // 2. Instanciar (Clonar) el panel desde el UXML
        VisualElement panel = disconnectTemplate.CloneTree();
        panel.name = panelName;
        panel.style.position = Position.Absolute;
        panel.style.width = Length.Percent(100);
        panel.style.height = Length.Percent(100);
        
        // Lo añadimos a la pantalla
        root.Add(panel);

        // 3. Configurar el texto del mensaje
        Label msgLabel = panel.Q<Label>(messageLabelName);
        if (msgLabel != null)
        {
            // Asignamos el mensaje que guardamos antes de salir de la sala
            msgLabel.text = reasonMessage;
        }
        else
        {
            Debug.LogWarning($"No se encontró un Label con el nombre '{messageLabelName}' en el template.");
        }

        // 4. Configurar el botón para cerrar (Aceptar)
        Button acceptBtn = panel.Q<Button>(acceptButtonName);
        if (acceptBtn != null)
        {
            acceptBtn.clicked += () =>
            {
                // Al hacer clic, simplemente eliminamos el panel instanciado de la jerarquía
                panel.RemoveFromHierarchy();
            };
        }
    }
}