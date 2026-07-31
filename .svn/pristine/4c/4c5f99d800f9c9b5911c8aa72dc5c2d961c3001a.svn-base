using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;

/// <summary>
/// Representa un personaje de jugador sincronizado en red.
/// Gestiona el estado del jugador entre clientes y servidor.
/// </summary>
[RequireComponent(typeof(NetworkAudio))]
public class NetworkedPlayerCharacter : NetworkBehaviour, ICharacter
{
    /// <summary>
    /// Nombre del jugador
    /// </summary>
    public string playerName;

    public UnityEvent OnPlayerSpawned = new UnityEvent();

    private NetworkAudio networkAudio;
    private NetworkPlayerName networkPlayerName;

    public override void OnNetworkSpawn()
    {
        networkAudio = GetComponent<NetworkAudio>();
        networkPlayerName = GetComponent<NetworkPlayerName>();

        // Solo el owner puede enviar audio
        if (IsOwner && GlobalVariables.Instance.GetIsAudioRoom())
            networkAudio.enabled = true;

        if (IsOwner)
        {
            TunnelingVignetteController vignette = GetComponentInChildren<TunnelingVignetteController>(true);
            if (vignette != null)
                vignette.gameObject.SetActive(true);
        }

        // Suscribirse al evento de LocalRegistry
        if (LocalRegistry.Instance != null)
        {
            LocalRegistry.Instance.OnClientUsernameReceived += OnClientUsernameReceived;
            
            string currentName = LocalRegistry.Instance.GetClientUsername(OwnerClientId);
            
            if (currentName != "Unknown" && !string.IsNullOrEmpty(currentName))
                SetPlayerName(currentName);
        }

        OnPlayerSpawned.Invoke();
    }

    public override void OnNetworkDespawn()
    {
        // Desuscribirse para evitar memory leaks
        if (LocalRegistry.Instance != null)
            LocalRegistry.Instance.OnClientUsernameReceived -= OnClientUsernameReceived;
    }

    /// <summary>
    /// Se llama desde LocalRegistry cuando se recibe el username del cliente
    /// </summary>
    /// <param name="clientId">Id del cliente</param>
    /// <param name="username">Username recibido</param>
    private void OnClientUsernameReceived(ulong clientId, string username)
    {
        // Solo nos importa el nombre de nuestro PlayerObject
        if (clientId != OwnerClientId)
            return;

        playerName = username;
        
        if (networkPlayerName != null)
            networkPlayerName.ChangeUsernameText(playerName);
    }

    /// <summary>
    /// Setear nombre manualmente (por compatibilidad)
    /// </summary>
    /// <param name="namePlayer"></param>
    public void SetPlayerName(string namePlayer)
    {
        playerName = namePlayer;

        if (networkPlayerName != null)
            networkPlayerName.ChangeUsernameText(playerName);
    }
}