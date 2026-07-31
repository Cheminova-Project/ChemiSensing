using UnityEngine;
using UnityEngine.UIElements;
using Unity.Netcode;

public class GlobalInvitationManager : MonoBehaviour
{
    [Header("Referencias UI")]
    private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset invitationTemplate;
    
    private string invitationPanelName = "invitation-panel";

    private void OnEnable()
    {
        UserControllerPlayer.OnInvitationReceived += ShowInvitationPanel;
    }

    private void OnDisable()
    {
        UserControllerPlayer.OnInvitationReceived -= ShowInvitationPanel;
    }

    private void ShowInvitationPanel(ulong senderClientId)
    {
        if (uiDocument == null)
            uiDocument = FindAnyObjectByType<UIDocument>();
        
        if (uiDocument == null || invitationTemplate == null)
        {
            Debug.LogError("Faltan referencias en GlobalInvitationManager");
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;
        
        if (root.Q(invitationPanelName) != null) 
        {
            CancelInvoke(nameof(CloseInvitationPanel));
            Invoke(nameof(CloseInvitationPanel), 10f);
            return;
        }

        VisualElement panel = invitationTemplate.CloneTree();
        panel.name = invitationPanelName;
        panel.style.position = Position.Absolute;
        panel.style.width = Length.Percent(100);
        panel.style.height = Length.Percent(100);
        root.Add(panel);
        
        Invoke(nameof(CloseInvitationPanel), 10f);

        Label msgLabel = panel.Q<Label>("message");
        if (msgLabel != null)
        {
            string senderName = "Unknown user";
            if (LocalRegistry.Instance != null)
            {
                var info = LocalRegistry.Instance.GetClientInfo(senderClientId);
                senderName = info.Username;
            }

            msgLabel.text = msgLabel.text.Replace("XXXXXX", senderName);
        }

        Button acceptBtn = panel.Q<Button>("accept-button");
        if (acceptBtn != null)
        {
            acceptBtn.clicked += () =>
            {
                CancelInvoke(nameof(CloseInvitationPanel));
                TeleportToLocalPlayer(senderClientId);
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
        if (uiDocument == null)
            return;
    
        VisualElement root = uiDocument.rootVisualElement;
        VisualElement panel = root?.Q(invitationPanelName);
    
        if (panel != null)
            panel.RemoveFromHierarchy();
    }

    /// <summary>
    /// Lógica de teletransporte independiente de la lista visual de usuarios
    /// </summary>
    private void TeleportToLocalPlayer(ulong targetClientId)
    {
        if (NetworkManager.Singleton == null)
            return;
        
        NetworkObject senderNetObj = null;

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(targetClientId, out NetworkClient client))
        {
            senderNetObj = client.PlayerObject;
        }
        else
        {
            Debug.LogError($"[Teleport] El cliente {targetClientId} no aparece en la lista de conectados.");
            return;
        }

        if (senderNetObj == null)
        {
            Debug.LogError($"[Teleport] El cliente {targetClientId} está conectado, pero NO TIENE un PlayerObject asignado.");
            return;
        }
        
        var senderController = senderNetObj.GetComponent<UserControllerPlayer>();
        if (senderController == null)
            senderController = senderNetObj.GetComponentInChildren<UserControllerPlayer>();

        if (NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject() == null)
            return;
        
        var localNetObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        
        var localController = localNetObj.GetComponent<UserControllerPlayer>();
        if (localController == null) localController = localNetObj.GetComponentInChildren<UserControllerPlayer>();

        if (senderController == null)
        {
            Debug.LogError($"[Teleport] ENCONTRADO el objeto {senderNetObj.name} pero NO TIENE el script 'UserControllerPlayer'.");
            return;
        }

        if (localController != null)
        {
            Transform targetT = senderController.playerCapsule != null ? senderController.playerCapsule : senderController.transform;
            
            localController.MoveToUser(
                targetT,
                senderController.actualCamera,
                senderController.GetPlatform()
            );
        }
    }
}