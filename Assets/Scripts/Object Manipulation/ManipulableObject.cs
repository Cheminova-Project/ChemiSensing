using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Permite que un objeto sea manipulado por el usuario (mover, rotar, escalar).
/// Gestiona los eventos y estados de manipulación del objeto.
/// </summary>
public class ManipulableObject : NetworkBehaviour
{
    NetworkObject _networkObject;
    private bool isBeingManipulated;
    private static HashSet<ManipulableObject> activeManipulatedObjects = new HashSet<ManipulableObject>();
    public static event Action<ManipulableObject> OnManipulationTimeout;
    
    [Header("Inactivity Timeout")]
    [SerializeField] private float inactivityTimeoutSeconds = 15f;
    private float currentIdleTime = 0f;
    
    private static float lastGrabMessageTime = 0f;
    private static ulong lastGrabberId = ulong.MaxValue;
    
    private Vector3 lastPos;
    private Quaternion lastRot;
    private Vector3 lastScale;
    
    private void Awake()
    {
        _networkObject = GetComponent<NetworkObject>();
        isBeingManipulated = false;
        
        if (_networkObject != null)
            _networkObject.DontDestroyWithOwner = true;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            Application.quitting += OnApplicationQuit;
        }

        if (IsServer)
            NetworkManager.Singleton.OnClientDisconnectCallback += OnServerClientDisconnect;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (NetworkManager.Singleton != null)
        {
            if (!IsServer)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
                Application.quitting -= OnApplicationQuit;
            }
            
            if (IsServer)
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnServerClientDisconnect;
        }
        activeManipulatedObjects.Remove(this);
    }
    
    private void Update()
    {
        if (isBeingManipulated && _networkObject != null && _networkObject.IsOwner)
        {
            if (NetworkManager.Singleton.LocalClientId == NetworkManager.ServerClientId)
                return;

            bool hasMoved = Vector3.Distance(transform.localPosition, lastPos) > 0.001f ||
                            Quaternion.Angle(transform.localRotation, lastRot) > 0.1f ||
                            Vector3.Distance(transform.localScale, lastScale) > 0.001f;

            if (hasMoved)
            {
                currentIdleTime = 0f;
                lastPos = transform.localPosition;
                lastRot = transform.localRotation;
                lastScale = transform.localScale;
            }
            else
            {
                currentIdleTime += Time.deltaTime;
                
                if (currentIdleTime >= inactivityTimeoutSeconds)
                {
                    Debug.LogWarning($"[ManipulableObject] Timeout de inactividad ({inactivityTimeoutSeconds}s) alcanzado. Forzando liberación.");
                    
                    ForceReleaseOwnership(true);
                    OnManipulationTimeout?.Invoke(this);
                    
                    var xrInteractable = GetComponent("UnityEngine.XR.Interaction.Toolkit.XRBaseInteractable");
                    if (xrInteractable != null)
                    {
                        var interactableType = xrInteractable.GetType();
                        var interactionManagerProp = interactableType.GetProperty("interactionManager");
                        if (interactionManagerProp != null)
                        {
                            var manager = interactionManagerProp.GetValue(xrInteractable);
                            if (manager != null)
                            {
                                var cancelMethod = manager.GetType().GetMethod("CancelInteractableSelection");
                                if (cancelMethod != null)
                                {
                                    cancelMethod.Invoke(manager, new object[] { xrInteractable });
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId && isBeingManipulated)
        {
            isBeingManipulated = false;
            ForceReleaseOwnership();
        }
    }

    private void OnApplicationQuit()
    {
        ReleaseAllOwnerships();
    }

    private static void ReleaseAllOwnerships()
    {
        foreach (var obj in activeManipulatedObjects)
        {
            if (obj != null && obj.isBeingManipulated)
            {
                obj.ForceReleaseOwnership();
            }
        }
        activeManipulatedObjects.Clear();
    }

    public void OnSelected()
    {   
        if(NetworkManager.Singleton != null && _networkObject.OwnerClientId != NetworkManager.Singleton.LocalClientId)
        {
            RequestOwnershipServerRpc(NetworkManager.Singleton.LocalClientId);
            activeManipulatedObjects.Add(this);
            lastPos = transform.localPosition;
            lastRot = transform.localRotation;
            lastScale = transform.localScale;
            currentIdleTime = 0f;
        }
    }
    
    public void OnDeselected()
    {
        if(NetworkManager.Singleton != null && _networkObject.OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            ReleaseOwnershipServerRpc(false);
            activeManipulatedObjects.Remove(this);
        }
    }

    private void ForceReleaseOwnership(bool isTimeout = false)
    {
        if (NetworkManager.Singleton != null && _networkObject != null && _networkObject.IsSpawned)
        {
            if (_networkObject.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                ReleaseOwnershipServerRpc(isTimeout);
            }
        }
        activeManipulatedObjects.Remove(this);
    }

    public void ResetTransform()
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    [ClientRpc(RequireOwnership = false)]
    public void OnOwnershipRemovedClientRpc(bool isTimeout, ulong previousOwnerId)
    {
        isBeingManipulated = false;
        
        if (NetworkManager.Singleton.LocalClientId == NetworkManager.ServerClientId)
            return;

        if (NetworkManager.Singleton.LocalClientId == previousOwnerId)
            return;

        if (ToolMessageHandler.Instance != null)
        {
            if (isTimeout)
                ToolMessageHandler.Instance.ShowMessage("A user went AFK. The object is now free to manipulate.", 4f, MessageType.Info);
            else
                ToolMessageHandler.Instance.ShowMessage("The object is now free to manipulate.", 3f, MessageType.Info);
        }
    }
    
    [ClientRpc(RequireOwnership = false)]
    public void OnOwnershipChangedClientRpc(ulong newOwnerId)
    {
        isBeingManipulated = true;
        
        if (NetworkManager.Singleton.LocalClientId == NetworkManager.ServerClientId)
            return;
        
        if (NetworkManager.Singleton.LocalClientId == newOwnerId)
            return;

        if (Time.time - lastGrabMessageTime < 5f && lastGrabberId == newOwnerId)
            return;
        
        lastGrabMessageTime = Time.time;
        lastGrabberId = newOwnerId;

        // Buscamos el nombre del nuevo dueño
        string grabberName = LocalRegistry.Instance != null ? LocalRegistry.Instance.GetClientUsername(newOwnerId) : "A user";
        
        if (ToolMessageHandler.Instance != null)
            ToolMessageHandler.Instance.ShowMessage($"{grabberName} is manipulating the model.", 3f, MessageType.Info);
    }
    
    public bool IsBeingManipulated()
    {
        return isBeingManipulated;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestOwnershipServerRpc(ulong clientId)
    {
        if (_networkObject != null && _networkObject.IsSpawned && _networkObject.OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            _networkObject.ChangeOwnership(clientId);
            OnOwnershipChangedClientRpc(clientId);
            isBeingManipulated = true;
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void ReleaseOwnershipServerRpc(bool isTimeout)
    {
        if (_networkObject != null && _networkObject.IsSpawned)
        {
            ulong previousOwnerId = _networkObject.OwnerClientId;
            _networkObject.RemoveOwnership();
            OnOwnershipRemovedClientRpc(isTimeout, previousOwnerId);
            isBeingManipulated = false;
        }
    }
    
    private void OnServerClientDisconnect(ulong clientId)
    {
        if (_networkObject != null && _networkObject.IsSpawned && _networkObject.OwnerClientId == clientId)
        {
            ulong previousOwnerId = _networkObject.OwnerClientId;
            _networkObject.RemoveOwnership();
            isBeingManipulated = false;
            
            OnOwnershipRemovedClientRpc(false, previousOwnerId);
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            ReleaseAllOwnerships();
    }
}
