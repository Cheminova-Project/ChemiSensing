using UnityEngine;
using UnityEngine.UIElements;

public class GlobalAnnotationInviteManager : MonoBehaviour
{
    [Header("Referencias UI")]
    private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset invitationTemplate;
    
    [Header("Configuración Herramienta")]
    [Tooltip("El ID exacto de la herramienta de anotaciones en el ToolDatabase")]
    [SerializeField] private string annotationToolId = "annotations"; 
    
    private string popupName = "annotation-invitation-panel";
    
    private ulong pendingHostId;
    private int pendingAnnotationId;

    private void OnEnable()
    {
        UserControllerPlayer.OnAnnotationInviteReceived += ShowInvitationPanel;
    }

    private void OnDisable()
    {
        UserControllerPlayer.OnAnnotationInviteReceived -= ShowInvitationPanel;
    }

    private void ShowInvitationPanel(ulong hostId, int annotationId)
    {
        if (uiDocument == null) uiDocument = FindAnyObjectByType<UIDocument>();
        if (uiDocument == null || invitationTemplate == null) return;

        VisualElement root = uiDocument.rootVisualElement;
        
        if (root.Q(popupName) != null) 
        {
            pendingHostId = hostId;
            pendingAnnotationId = annotationId;
            CancelInvoke(nameof(CloseInvitationPanel));
            Invoke(nameof(CloseInvitationPanel), 15f);
            return;
        }

        pendingHostId = hostId;
        pendingAnnotationId = annotationId;

        VisualElement panel = invitationTemplate.CloneTree();
        panel.name = popupName;
        panel.style.position = Position.Absolute;
        panel.style.width = Length.Percent(100);
        panel.style.height = Length.Percent(100);
        root.Add(panel);
        
        Invoke(nameof(CloseInvitationPanel), 15f);

        Label msgLabel = panel.Q<Label>("message");
        if (msgLabel != null)
        {
            string hostName = "Unknown user";
            if (LocalRegistry.Instance != null)
            {
                var info = LocalRegistry.Instance.GetClientInfo(hostId);
                if (!string.IsNullOrEmpty(info.Username))
                {
                    hostName = info.Username;
                }
            }
            msgLabel.text = msgLabel.text.Replace("XXXXXX", hostName);
        }

        Button acceptBtn = panel.Q<Button>("accept-button");
        if (acceptBtn != null)
        {
            acceptBtn.clicked += () =>
            {
                CancelInvoke(nameof(CloseInvitationPanel));
                AcceptInvite(pendingHostId, pendingAnnotationId);
                panel.RemoveFromHierarchy();
            };
        }

        Button rejectBtn = panel.Q<Button>("reject-button");
        if (rejectBtn != null)
        {
            rejectBtn.clicked += () =>
            {
                CancelInvoke(nameof(CloseInvitationPanel));
                panel.RemoveFromHierarchy();
            };
        }
    }
    
    private void CloseInvitationPanel()
    {
        if (uiDocument == null) return;
        VisualElement panel = uiDocument.rootVisualElement?.Q(popupName);
        if (panel != null) panel.RemoveFromHierarchy();
    }

    private void AcceptInvite(ulong hostId, int annotationId)
    {
        AnnotationManager activeManager = FindAnyObjectByType<AnnotationManager>();
        
        if (activeManager != null && activeManager.gameObject.activeInHierarchy)
        {
            activeManager.ProcessPendingInvite(hostId, annotationId);
        }
        else
        {
            AnnotationManager.PendingAnnotationId = annotationId;
            AnnotationManager.PendingHostId = hostId;
            
            if (ToolMenuController.Instance != null)
            {
                ToolMenuController.Instance.ActivateToolById(annotationToolId);
            }
            else
            {
                Debug.LogWarning("[GlobalAnnotationInvite] No se encontró el ToolMenuController.Instance");
            }
        }
    }
}