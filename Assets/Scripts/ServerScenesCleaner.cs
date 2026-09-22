using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

/// <summary>
/// Componente exclusivo del servidor (Headless).
/// Monitoriza el número de clientes y apaga la instancia si se queda vacía
/// tras un periodo de gracia.
/// </summary>
public class ServerScenesCleaner : NetworkBehaviour
{
    [SerializeField] private float emptyRoomTimeout = 10f;
    private int serverRealPort = -1;
    
    private Coroutine shutdownCoroutine;
    private Coroutine periodicUpdateCoroutine;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer)
        {
            enabled = false;
            return;
        }
        
        if (NetworkManager.Singleton.NetworkConfig.NetworkTransport is UnityTransport transport)
            serverRealPort = transport.ConnectionData.Port;
        else
            Debug.LogError("[ServerScenesCleaner] No se pudo obtener el puerto del UnityTransport.");

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientChanged;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientChanged;

        CheckForEmptyRoom();
        periodicUpdateCoroutine = StartCoroutine(PeriodicUpdateRoutine());
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientChanged;
        }
        
        if (periodicUpdateCoroutine != null)
        {
            StopCoroutine(periodicUpdateCoroutine);
            periodicUpdateCoroutine = null;
        }
    }
    
    private IEnumerator PeriodicUpdateRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            NotifyPythonOfChange();
        }
    }

    private void OnClientChanged(ulong clientId)
    {
        CheckForEmptyRoom();
        NotifyPythonOfChange();
    }

    private void CheckForEmptyRoom()
    {
        int realPlayers = NetworkManager.Singleton.ConnectedClientsIds.Count;
        if (NetworkManager.Singleton.IsHost) realPlayers -= 1;

        if (realPlayers <= 0)
        {
            if (shutdownCoroutine == null)
                shutdownCoroutine = StartCoroutine(ShutdownTimer());
        }
        else
        {
            if (shutdownCoroutine != null)
            {
                StopCoroutine(shutdownCoroutine);
                shutdownCoroutine = null;
            }
        }
    }

    private IEnumerator ShutdownTimer()
    {
        if (NetworkServerConfiguration.Instance.connectMode == ConnectMode.LOCAL)
            yield break;
        
        yield return new WaitForSeconds(emptyRoomTimeout);
        Debug.LogWarning("[ZombieCleaner] Apagando servidor por inactividad...");
        
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.Shutdown();
        
        if (Application.isBatchMode)
            Application.Quit();
    }
    
    private void NotifyPythonOfChange()
    {
        if (RoomsManager.Instance == null || serverRealPort == -1)
            return;
        
        List<string> usersActivos = new List<string>();
        foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (id == NetworkManager.ServerClientId && !NetworkManager.Singleton.IsHost)
                continue;
            
            if (LocalRegistry.Instance != null)
            {
                var info = LocalRegistry.Instance.GetClientInfo(id);
                if (!string.IsNullOrEmpty(info.Username))
                    usersActivos.Add(info.Username);
            }
        }
    
        RoomsManager.Instance.EnviarMensaje(
            "UPDATE_USERS", 
            port: serverRealPort, 
            acceptedUsers: usersActivos,
            customHeaderKey: "x-server-token",
            customHeaderValue: "MI_CLAVE_SECRETA_HEADLESS_123"
        );
    }
}