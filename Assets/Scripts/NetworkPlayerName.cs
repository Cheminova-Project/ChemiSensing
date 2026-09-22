using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;

public class NetworkPlayerName : NetworkBehaviour
{
    [Header("UI & Info")]
    [Tooltip("Text with the name of the user")]
    public TextMeshProUGUI usernameText;
    
    [Tooltip("Image with the color of the user")]
    public Image playerIconImage;
    
    public NetworkVariable<Color> playerColor = new NetworkVariable<Color>(
        Color.white, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        playerColor.OnValueChanged += OnColorChanged;

        if (IsServer)
            playerColor.Value = RoomColorManager.Instance.GetUniqueRandomColor();

        UpdateVisuals(playerColor.Value);
        
        if (LocalRegistry.Instance != null)
        {
            string currentName = LocalRegistry.Instance.GetClientUsername(OwnerClientId);
            if (!string.IsNullOrEmpty(currentName) && currentName != "Unknown")
                ChangeUsernameText(currentName);

            LocalRegistry.Instance.OnClientUsernameReceived += HandleUsernameReceived;
        }
    }

    public override void OnNetworkDespawn()
    {
        playerColor.OnValueChanged -= OnColorChanged;

        if (IsServer)
        {
            if (RoomColorManager.Instance != null)
                RoomColorManager.Instance.ReturnColor(playerColor.Value);
        }

        if (LocalRegistry.Instance != null)
                LocalRegistry.Instance.OnClientUsernameReceived -= HandleUsernameReceived;
    }
    
    private void HandleUsernameReceived(ulong clientId, string newUsername)
    {
        if (clientId == OwnerClientId)
            ChangeUsernameText(newUsername);
    }

    private void OnColorChanged(Color previousValue, Color newValue)
    {
        UpdateVisuals(newValue);
    }

    private void UpdateVisuals(Color currentColor)
    {
        if (playerIconImage == null)
            return;

        playerIconImage.color = currentColor;
    }

    public void ChangeUsernameText(string newUsername)
    {
        if (usernameText != null)
            usernameText.text = newUsername;
        else
            Debug.LogError("[NetworkPlayerName] ERROR: La referencia 'usernameText' está vacía. ¡Asígnala en el Inspector del Prefab!");
    }
}