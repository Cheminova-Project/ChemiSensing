using UnityEngine;
using UnityEngine.UIElements;
using Unity.Netcode;
using System.Collections.Generic;

/// <summary>
/// Componente de herramienta que gestiona la interfaz de control de audio de usuarios.
/// Muestra una lista de usuarios conectados y permite controlar su audio.
/// </summary>
public class UserAudio : ToolComponent
{
    string usersContainerName = "users-container";
    string muteButtonName = "mute-button";
    string muteImageName = "mute-image";
    string volumeSliderName = "volume-slider";

    [SerializeField] private VisualTreeAsset userAudioTemplate;
    [SerializeField] private Sprite muteImage;
    [SerializeField] private Sprite unMuteImage;

    /// <summary>
    /// Diccionario que mantiene referencias a las entradas de usuario en la UI
    /// Key: ClientId del usuario, Value: VisualElement y Botón asociados
    /// </summary>
    private class UserAudioEntry
    {
        public ulong ClientId;
        public VisualElement Entry;
        public Button MuteButton;
        public VisualElement MuteIcon;
        public Slider VolumeSlider;

        public GameObject UserGameObject;
        public AudioSource AudioSource;

        public System.Action MuteCallback;
        public EventCallback<ChangeEvent<float>> VolumeCallback;
    }

    private Dictionary<ulong, UserAudioEntry> _userEntries = new();

    /// <summary>
    /// Se llama cuando se activa la herramienta. Carga y muestra todos los usuarios conectados.
    /// </summary>
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

    /// <summary>
    /// Se llama cuando se desactiva la herramienta. Limpia las entradas de usuarios.
    /// </summary>
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
    /// Limpia todas las entradas de usuario de la UI y desuscribe los eventos
    /// </summary>
    private void ClearUserEntries()
    {
        foreach (var value in _userEntries.Values)
        {
            if (value.MuteButton != null && value.MuteCallback != null)
                value.MuteButton.clicked -= value.MuteCallback;
            
            if (value.VolumeSlider != null && value.VolumeCallback != null)
                value.VolumeSlider.UnregisterValueChangedCallback(value.VolumeCallback);
            
            value.Entry?.RemoveFromHierarchy();
        }
        
        _userEntries.Clear();
    }

    /// <summary>
    /// Añade un usuario a la lista de la interfaz
    /// </summary>
    /// <param name="username">Nombre del usuario</param>
    /// <param name="clientID">ID del cliente en la red</param>
    private void AddUser(string username, ulong clientID)
    {
        VisualElement usersContainer = uIDocument.rootVisualElement.Q<VisualElement>(usersContainerName);
        
        if (usersContainer == null || userAudioTemplate == null)
            return;

        VisualElement newEntry = userAudioTemplate.CloneTree();
        usersContainer.Add(newEntry);
        newEntry.userData = clientID;

        Label usernameLabel = newEntry.Q<Label>("username");
        if (usernameLabel == null)
        {
            Debug.LogWarning("No se encontró el Label 'username'");
            return;
        }

        usernameLabel.text = username;
        
        GameObject userGO = LocalRegistry.Instance.GetPlayerGameObject(clientID);
        AudioSource audioSource = userGO != null ? userGO.GetComponent<AudioSource>() : null;
        
        Slider userVolumeSlider = newEntry.Q<Slider>(volumeSliderName);
        EventCallback<ChangeEvent<float>> volumeCallback = null;
        if (userVolumeSlider != null)
        {
            userVolumeSlider.value = audioSource != null ? audioSource.volume : 1f;

            volumeCallback = evt =>
            {
                OnUserVolumeSliderClicked(evt, clientID);
            };
            
            userVolumeSlider.RegisterValueChangedCallback(volumeCallback);
        }
        
        Button userMuteButton = newEntry.Q<Button>(muteButtonName);
        
        VisualElement userMuteIcon = newEntry.Q<VisualElement>(muteImageName); 

        System.Action muteCallback = null;
        if (userMuteButton != null && userMuteIcon != null)
        {
            // Función local para actualizar la visual
            void UpdateMuteIcon(bool isMuted)
            {
                Sprite targetSprite = isMuted ? muteImage : unMuteImage;
                userMuteIcon.style.backgroundImage = new StyleBackground(targetSprite);
            }

            if (audioSource != null)
            {
                UpdateMuteIcon(audioSource.mute);
            }
            
            muteCallback = () =>
            {
                OnUserMuteButtonClicked(clientID);
            };

            userMuteButton.clicked += muteCallback;
        }
        else
        {
            Debug.LogWarning($"Falta el botón '{muteButtonName}' o la imagen '{muteImageName}'");
        }
        
        _userEntries[clientID] = new UserAudioEntry
        {
            ClientId = clientID,
            Entry = newEntry,
            MuteButton = userMuteButton,
            MuteIcon = userMuteIcon,
            VolumeSlider = userVolumeSlider,
            UserGameObject = userGO,
            AudioSource = audioSource,
            MuteCallback = muteCallback,
            VolumeCallback = volumeCallback
        };
    }

    /// <summary>
    /// Maneja el evento de clic en el botón de silenciar a un usuario
    /// </summary>
    /// <param name="clientID">ID del cliente objetivo</param>
    /// <param name="userEntry">Elemento visual del usuario</param>
    private void OnUserMuteButtonClicked(ulong clientID)
    {
        if (!_userEntries.TryGetValue(clientID, out var entry) || entry.AudioSource == null)
            return;

        entry.AudioSource.mute = !entry.AudioSource.mute;
        Sprite targetSprite = entry.AudioSource.mute ? muteImage : unMuteImage;
        
        if (entry.MuteIcon != null)
        {
            entry.MuteIcon.style.backgroundImage = new StyleBackground(targetSprite);
        }
    }
    
    /// <summary>
    /// Maneja el evento de cambio de valor del slider del volumen de usuario
    /// </summary>
    /// <param name="evt">Referencia del evento con el cambio de volumen</param>
    /// <param name="clientID">ID del cliente objetivo</param>
    /// <param name="userEntry">Elemento visual del usuario</param>
    private void OnUserVolumeSliderClicked(ChangeEvent<float> evt, ulong clientID)
    {
        if (!_userEntries.TryGetValue(clientID, out var entry) || entry.AudioSource == null)
            return;

        entry.AudioSource.volume = evt.newValue;
    }
    
    /// <summary>
    /// Se ejecuta cuando LocalRegistry recibe el username de alguien (al conectarse o al actualizarse)
    /// </summary>
    private void HandleClientUsernameReceived(ulong clientId, string username)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId) return;

        // Si el usuario ya está en la interfaz, actualizamos su nombre
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
    /// Se ejecuta automáticamente cuando LocalRegistry detecta que un cliente se va
    /// </summary>
    private void HandleClientDisconnected(ulong clientId)
    {
        if (_userEntries.TryGetValue(clientId, out var entry))
        {
            if (entry.MuteButton != null && entry.MuteCallback != null)
                entry.MuteButton.clicked -= entry.MuteCallback;

            if (entry.VolumeSlider != null && entry.VolumeCallback != null)
                entry.VolumeSlider.UnregisterValueChangedCallback(entry.VolumeCallback);

            entry.Entry?.RemoveFromHierarchy();
            _userEntries.Remove(clientId);
        }
    }
}