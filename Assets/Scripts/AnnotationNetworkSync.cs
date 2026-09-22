using Unity.Netcode;
using UnityEngine;
using System;

public class AnnotationNetworkSync : NetworkBehaviour
{
    public static AnnotationNetworkSync Instance;

    // Eventos a los que se suscribirá el AnnotationManager
    public Action<ulong, string, int> OnInviteReceived;
    public Action<string, string, string> OnFormUpdated;

    private void Awake()
    {
        Instance = this;
    }

    // ----------------------------------------------------
    // 1. SISTEMA DE INVITACIONES
    // ----------------------------------------------------
    public void SendInvite(int currentAnnotationId = -1)
    {
        if (IsClient)
            SendInviteServerRpc(NetworkManager.Singleton.LocalClientId, currentAnnotationId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendInviteServerRpc(ulong senderId, int annotationId)
    {
        // Obtenemos el nombre del host (o ponemos uno genérico)
        string senderName = LocalRegistry.Instance != null ? LocalRegistry.Instance.GetClientUsername(senderId) : "Un compañero";
        ReceiveInviteClientRpc(senderId, senderName, annotationId);
    }

    [ClientRpc]
    private void ReceiveInviteClientRpc(ulong senderId, string senderName, int annotationId)
    {
        if (senderId == NetworkManager.Singleton.LocalClientId)
            return;
        
        OnInviteReceived?.Invoke(senderId, senderName, annotationId);
    }
    
    public void SendFormUpdate(string title, string desc, string category)
    {
        if (IsClient)
            SendLiveUpdateServerRpc(NetworkManager.Singleton.LocalClientId, title, desc, category);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendLiveUpdateServerRpc(ulong senderId, string title, string desc, string category)
    {
        ReceiveLiveUpdateClientRpc(senderId, title, desc, category);
    }

    [ClientRpc]
    private void ReceiveLiveUpdateClientRpc(ulong senderId, string title, string desc, string category)
    {
        if (senderId == NetworkManager.Singleton.LocalClientId) return;
        OnFormUpdated?.Invoke(title, desc, category);
    }
}