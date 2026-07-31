using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Puente para registrar y gestionar objetos de red en la aplicación.
/// Permite la integración y registro de objetos sincronizados en red.
/// </summary>
public class NetworkRegistryBridge : NetworkBehaviour
{
    private static NetworkRegistryBridge _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    /// <summary>
    /// LocalRegistry llama esto cuando un cliente se registra
    /// </summary>
    /// <param name="clientId">ID del cliente que se está registrando</param>
    public void RequestUsername(ulong clientId)
    {
        SendExistingUsernamesToClientRpc(clientId);
        RequestUsernameClientRpc(clientId);
    }

    /// <summary>
    /// RPC: El servidor envía todos los usernames existentes al cliente recién conectado
    /// </summary>
    /// <param name="newClientId">ID del nuevo cliente</param>
    [ClientRpc]
    private void SendExistingUsernamesToClientRpc(ulong newClientId)
    {
        // Solo ejecutar en el cliente que se conectó, no en todos
        if (NetworkManager.Singleton.LocalClientId == newClientId)
        {
            ReceiveExistingUsernamesServerRpc();
        }
    }

    /// <summary>
    /// RPC: El cliente nuevo solicita al servidor todos los usernames existentes
    /// </summary>
    /// <param name="serverRpcParams">Parámetros del RPC del servidor</param>
    [ServerRpc(RequireOwnership = false)]
    private void ReceiveExistingUsernamesServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong requestingClientId = serverRpcParams.Receive.SenderClientId;

        // Obtener todos los usernames actuales del LocalRegistry
        var allUsernames = LocalRegistry.Instance.GetAllClientUsernames();

        var serializableDict = new UsernameDictionarySerializable(allUsernames);
        SendAllUsernamesClientRpc(serializableDict, requestingClientId);
    }

    /// <summary>
    /// RPC: El servidor envía todos los usernames al cliente específico
    /// </summary>
    /// <param name="usernames">Diccionario serializable de usernames</param>
    /// <param name="targetClientId">ID del cliente objetivo</param>
    [ClientRpc]
    private void SendAllUsernamesClientRpc(UsernameDictionarySerializable usernames, ulong targetClientId)
    {
        // Solo el cliente que lo solicitó recibe esta información
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {

            var dict = usernames.ToDictionary();
            
            // Actualizar LocalRegistry con todos los usernames existentes
            foreach (var kvp in dict)
            {
                if (!kvp.Value.Equals("Unknown"))
                    LocalRegistry.Instance.UpdateClientUsername(kvp.Key, kvp.Value);
            }
        }
    }

    /// <summary>
    /// RPC: El servidor solicita el username a un cliente específico
    /// </summary>
    /// <param name="targetClientId">ID del cliente objetivo</param>
    [ClientRpc]
    private void RequestUsernameClientRpc(ulong targetClientId)
    {
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {
            string username = GlobalManagement.Instance.GetUsername();
            SendUsernameToServerRpc(username);
        }
    }

    /// <summary>
    /// RPC: El cliente responde con su username al servidor
    /// </summary>
    /// <param name="username">El username del cliente</param>
    /// <param name="senderClientId">ID del cliente que envía el username</param>
    [ServerRpc(RequireOwnership = false)]
    private void SendUsernameToServerRpc(string username, ServerRpcParams serverRpcParams = default)
    {
        ulong actualSenderId = serverRpcParams.Receive.SenderClientId;
        LocalRegistry.Instance.UpdateClientUsername(actualSenderId, username);
        BroadcastUsernameToAllClientRpc(actualSenderId, username);
    }

    /// <summary>
    /// RPC: Notificar a todos los clientes del nuevo username
    /// </summary>
    /// <param name="clientId">ID del cliente cuyo username ha cambiado</param>
    /// <param name="username">El nuevo username del cliente</param>
    [ClientRpc]
    private void BroadcastUsernameToAllClientRpc(ulong clientId, string username)
    {
        LocalRegistry.Instance.UpdateClientUsername(clientId, username);
    }

    public static NetworkRegistryBridge Instance => _instance;

    [System.Serializable]
    public struct UsernameDictionarySerializable : INetworkSerializable
    {
        public ulong[] clientIds;
        public string[] usernames;

        public UsernameDictionarySerializable(Dictionary<ulong, string> dict)
        {
            clientIds = new ulong[dict.Count];
            usernames = new string[dict.Count];
            
            int index = 0;
            foreach (var kvp in dict)
            {
                clientIds[index] = kvp.Key;
                usernames[index] = kvp.Value ?? "Unknown";
                index++;
            }
        }

        public Dictionary<ulong, string> ToDictionary()
        {
            var dict = new Dictionary<ulong, string>();
            if (clientIds == null || usernames == null)
                return dict;
                
            for (int i = 0; i < clientIds.Length; i++)
            {
                dict[clientIds[i]] = usernames[i] ?? "Unknown";
            }
            return dict;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            int length = clientIds?.Length ?? 0;
            serializer.SerializeValue(ref length);

            if (!serializer.IsWriter)
            {
                clientIds = new ulong[length];
                usernames = new string[length];
            }

            for (int i = 0; i < length; i++)
            {
                serializer.SerializeValue(ref clientIds[i]);
                serializer.SerializeValue(ref usernames[i]);
            }
        }
    }
}