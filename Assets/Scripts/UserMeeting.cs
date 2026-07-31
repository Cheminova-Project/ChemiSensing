using UnityEngine;
using UnityEngine.UIElements;
using Unity.Netcode;
using System.Collections.Generic;

public class UserMeeting : ToolComponent
{
    string usersContainerName = "users-container";
    string joinButtonName = "join-button";
    string attractButtonName = "attract-button";
    
    string joinImageName = "join-image"; 
    string attractImageName = "attract-image";

    [Header("UI Templates")]
    [SerializeField] private VisualTreeAsset userItemTemplate;

    [Header("Icons")]
    [SerializeField] private Sprite joinSprite;
    [SerializeField] private Sprite attractSprite;

    public class UserMeetingEntry
    {
        public ulong ClientId;
        public VisualElement Entry;
        public Button JoinButton;
        public Button AttractButton;
        public System.Action JoinCallback;
        public System.Action AttractCallback;
        public GameObject UserGameObject;
        public UserControllerPlayer RemoteUserController;
    }

    private Dictionary<ulong, UserMeetingEntry> _userEntries = new();

    protected override void OnEnable()
    {
        base.OnEnable();
        ClearUserEntries();
        
        if (LocalRegistry.Instance != null)
        {
            var clientIds = LocalRegistry.Instance.GetAllClientIds();
            foreach (var clientID in clientIds)
            {
                if (clientID == NetworkManager.Singleton.LocalClientId)
                    continue;

                var info = LocalRegistry.Instance.GetClientInfo(clientID);
                AddUser(info.Username, clientID);
            }

            LocalRegistry.Instance.OnClientUsernameReceived += HandleClientUsernameReceived;
            LocalRegistry.Instance.OnClientUnregistered += HandleClientDisconnected;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        ClearUserEntries();
        
        if (LocalRegistry.Instance != null)
        {
            LocalRegistry.Instance.OnClientUsernameReceived -= HandleClientUsernameReceived;
            LocalRegistry.Instance.OnClientUnregistered -= HandleClientDisconnected;
        }
    }
    
    /// <summary>
    /// Se ejecuta cuando LocalRegistry recibe el username de alguien (al conectarse o al actualizarse)
    /// </summary>
    private void HandleClientUsernameReceived(ulong clientId, string username)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId) return;

        // Si el usuario ya está en la interfaz (ej: entró como "Unknown"), solo actualizamos su nombre
        if (_userEntries.TryGetValue(clientId, out var entry))
        {
            Label usernameLabel = entry.Entry.Q<Label>("username");
            if (usernameLabel != null)
                usernameLabel.text = username;
        }
        else
            AddUser(username, clientId);
    }
    
    /// <summary>
    /// Se ejecuta cuando LocalRegistry detecta que alguien se fue
    /// </summary>
    private void HandleClientDisconnected(ulong clientId)
    {
        if (_userEntries.TryGetValue(clientId, out var entry))
        {
            // Desuscribimos botones
            if (entry.JoinButton != null && entry.JoinCallback != null)
                entry.JoinButton.clicked -= entry.JoinCallback;

            if (entry.AttractButton != null && entry.AttractCallback != null)
                entry.AttractButton.clicked -= entry.AttractCallback;

            // Destruimos la UI
            entry.Entry?.RemoveFromHierarchy();
            _userEntries.Remove(clientId);
        }
    }

    private void ClearUserEntries()
    {
        foreach (var entry in _userEntries.Values)
        {
            if (entry.JoinButton != null && entry.JoinCallback != null)
                entry.JoinButton.clicked -= entry.JoinCallback;

            if (entry.AttractButton != null && entry.AttractCallback != null)
                entry.AttractButton.clicked -= entry.AttractCallback;

            entry.Entry?.RemoveFromHierarchy();
        }

        _userEntries.Clear();
    }

    private void AddUser(string username, ulong clientID)
    {
        if (uIDocument == null || uIDocument.rootVisualElement == null)
        {
            Debug.LogError("[UserMeeting-ERROR] uIDocument o rootVisualElement son NULL.");
            return;
        }

        VisualElement usersContainer = uIDocument.rootVisualElement.Q<VisualElement>(usersContainerName);
        if (usersContainer == null)
        {
            Debug.LogError($"[UserMeeting-ERROR] No se encontró el contenedor: '{usersContainerName}' en el UXML base.");
            return;
        }

        if (userItemTemplate == null)
        {
            Debug.LogError("[UserMeeting-ERROR] userItemTemplate es NULL. ¡Se perdió la referencia en el Inspector para esta plataforma!");
            return;
        }

        VisualElement newEntry = userItemTemplate.CloneTree();
        usersContainer.Add(newEntry);
        newEntry.userData = clientID;

        Label usernameLabel = newEntry.Q<Label>("username");
        if (usernameLabel != null)
            usernameLabel.text = username;
        else
            Debug.LogWarning("[UserMeeting-WARN] No se encontró el Label 'username' en el template.");

        // Referencias de red
        GameObject userGO = LocalRegistry.Instance.GetPlayerGameObject(clientID);
        UserControllerPlayer remoteController = null;
        if (userGO != null)
            remoteController = userGO.GetComponent<UserControllerPlayer>();

        Button joinButton = newEntry.Q<Button>(joinButtonName);
        VisualElement joinImageElement = newEntry.Q<VisualElement>(joinImageName);

        System.Action joinCallback = null;
        if (joinButton != null)
        {
            if (joinImageElement != null && joinSprite != null)
                joinImageElement.style.backgroundImage = new StyleBackground(joinSprite);

            joinCallback = () => { OnUserJoinButtonClicked(clientID); };
            joinButton.clicked += joinCallback;
        }

        Button attractButton = newEntry.Q<Button>(attractButtonName);
        VisualElement attractImageElement = newEntry.Q<VisualElement>(attractImageName);

        System.Action attractCallback = null;
        if (attractButton != null)
        {
            if (attractImageElement != null && attractSprite != null)
                attractImageElement.style.backgroundImage = new StyleBackground(attractSprite);

            attractCallback = () => { OnUserRequestJoinButtonClicked(clientID); };
            attractButton.clicked += attractCallback;
        }

        _userEntries[clientID] = new UserMeetingEntry
        {
            ClientId = clientID,
            Entry = newEntry,
            JoinButton = joinButton,
            AttractButton = attractButton,
            JoinCallback = joinCallback,
            AttractCallback = attractCallback,
            UserGameObject = userGO,
            RemoteUserController = remoteController
        };
    }

    private void OnUserJoinButtonClicked(ulong targetClientId) { TeleportToUser(targetClientId); }
    private void OnUserRequestJoinButtonClicked(ulong targetClientId) {
        var localPlayerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (localPlayerObj != null) {
            var localController = localPlayerObj.GetComponent<UserControllerPlayer>();
            if (localController != null) localController.SendTeleportInvitation(targetClientId);
        }
    }
    private void TeleportToUser(ulong targetClientId) {
        if (!_userEntries.TryGetValue(targetClientId, out var entry)) return;
        if (entry == null || entry.UserGameObject == null) return;
        var localObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (localObj == null) return;
        var localController = localObj.GetComponent<UserControllerPlayer>();
        var targetController = entry.RemoteUserController ?? entry.UserGameObject.GetComponent<UserControllerPlayer>();
        if (localController != null && targetController != null) {
            localController.MoveToUser(targetController.playerCapsule, targetController.actualCamera, targetController.GetPlatform());
        }
    }
}