using UnityEngine;
using Unity.Netcode;
using System;

public struct SyncedRulerData : INetworkSerializable, IEquatable<SyncedRulerData>
{
    public ulong MeasuredObjectId;
    public Vector3[] Points;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref MeasuredObjectId);
        if (serializer.IsWriter)
        {
            int count = Points?.Length ?? 0;
            serializer.SerializeValue(ref count);
            for (int i = 0; i < count; i++) serializer.SerializeValue(ref Points[i]);
        }
        else
        {
            int count = 0;
            serializer.SerializeValue(ref count);
            Points = new Vector3[count];
            for (int i = 0; i < count; i++) serializer.SerializeValue(ref Points[i]);
        }
    }

    public bool Equals(SyncedRulerData other)
    {
        if (MeasuredObjectId != other.MeasuredObjectId) return false;
        if (Points == null && other.Points == null) return true;
        if (Points == null || other.Points == null) return false;
        if (Points.Length != other.Points.Length) return false;
        for (int i = 0; i < Points.Length; i++) if (Points[i] != other.Points[i]) return false;
        return true;
    }
}

public struct SharedSegmentData : INetworkSerializable, IEquatable<SharedSegmentData>
{
    public int StartPoint;
    public int EndPoint;
    public bool DirectConnection;
    public bool IsSharing;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref StartPoint);
        serializer.SerializeValue(ref EndPoint);
        serializer.SerializeValue(ref DirectConnection);
        serializer.SerializeValue(ref IsSharing);
    }

    public bool Equals(SharedSegmentData other)
    {
        return StartPoint == other.StartPoint && EndPoint == other.EndPoint &&
               DirectConnection == other.DirectConnection && IsSharing == other.IsSharing;
    }
}

public struct SharedAngleData : INetworkSerializable, IEquatable<SharedAngleData>
{
    public int StartPoint;
    public int VertexPoint;
    public int EndPoint;
    public bool IsSharing;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref StartPoint);
        serializer.SerializeValue(ref VertexPoint);
        serializer.SerializeValue(ref EndPoint);
        serializer.SerializeValue(ref IsSharing);
    }

    public bool Equals(SharedAngleData other)
    {
        return StartPoint == other.StartPoint && VertexPoint == other.VertexPoint &&
               EndPoint == other.EndPoint && IsSharing == other.IsSharing;
    }
}

public class NetworkedRulerSync : NetworkBehaviour
{
    [HideInInspector] 
    public Ruler rulerComponent;

    public NetworkVariable<SyncedRulerData> syncedData = new NetworkVariable<SyncedRulerData>(
        new SyncedRulerData { MeasuredObjectId = 0, Points = new Vector3[0] },
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<SharedSegmentData> sharedSegment = new NetworkVariable<SharedSegmentData>(
        new SharedSegmentData { IsSharing = false },
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    public NetworkVariable<SharedAngleData> sharedAngle = new NetworkVariable<SharedAngleData>(
        new SharedAngleData { IsSharing = false },
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    
    public NetworkVariable<bool> isModelScaleInitialized = new NetworkVariable<bool>(
        false, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            var allSyncs = FindObjectsByType<NetworkedRulerSync>(FindObjectsSortMode.None);
            foreach (var sync in allSyncs)
            {
                if (sync != this && sync.IsSpawned && sync.syncedData.Value.Points != null && sync.syncedData.Value.Points.Length > 0)
                {
                    this.syncedData.Value = sync.syncedData.Value;
                    this.sharedSegment.Value = sync.sharedSegment.Value;
                    this.sharedAngle.Value = sync.sharedAngle.Value;
                    break;
                }
            }
        }
        syncedData.OnValueChanged += OnDataChanged;
        sharedSegment.OnValueChanged += OnSharedSegmentChanged;
        sharedAngle.OnValueChanged += OnSharedAngleChanged;
    }

    public override void OnNetworkDespawn()
    {
        syncedData.OnValueChanged -= OnDataChanged;
        sharedSegment.OnValueChanged -= OnSharedSegmentChanged;
    }

    private void OnDataChanged(SyncedRulerData previous, SyncedRulerData current)
    {
        if (!IsOwner) return;
        if (rulerComponent != null && rulerComponent.gameObject.activeInHierarchy)
            rulerComponent.SyncFromNetwork(current);
    }

    private void OnSharedSegmentChanged(SharedSegmentData previous, SharedSegmentData current)
    {
        if (!IsOwner) return;
        if (rulerComponent != null && rulerComponent.gameObject.activeInHierarchy)
            rulerComponent.ApplySharedSegmentFromNetwork(current);
    }
    
    private void OnSharedAngleChanged(SharedAngleData previous, SharedAngleData current)
    {
        if (!IsOwner)
            return;
        
        if (rulerComponent != null && rulerComponent.gameObject.activeInHierarchy)
        {
            if (rulerComponent is ScreenAngleRuler angleRuler)
            {
                angleRuler.ApplySharedAngleFromNetwork(new ScreenAngleRuler.AngleData(
                    current.StartPoint, 
                    current.VertexPoint, 
                    current.EndPoint, 
                    current.IsSharing));
            }
        }
    }

    public void RequestAddPoint(Vector3 localPoint, ulong measuredObjectId) => AddPointServerRpc(localPoint, measuredObjectId);

    [ServerRpc(RequireOwnership = false)]
    private void AddPointServerRpc(Vector3 localPoint, ulong measuredObjectId)
    {
        var currentPoints = syncedData.Value.Points ?? new Vector3[0];
        var newPoints = new Vector3[currentPoints.Length + 1];
        currentPoints.CopyTo(newPoints, 0);
        newPoints[newPoints.Length - 1] = localPoint;

        var newData = new SyncedRulerData { MeasuredObjectId = measuredObjectId, Points = newPoints };
        var allSyncs = FindObjectsByType<NetworkedRulerSync>(FindObjectsSortMode.None);
        foreach (var sync in allSyncs) sync.syncedData.Value = newData;
    }

    public void RequestResetRuler() => ResetRulerServerRpc();

    [ServerRpc(RequireOwnership = false)]
    private void ResetRulerServerRpc()
    {
        var newData = new SyncedRulerData { MeasuredObjectId = 0, Points = new Vector3[0] };
        var emptySegment = new SharedSegmentData { IsSharing = false };
        var emptyAngle = new SharedAngleData { IsSharing = false };
        var allSyncs = FindObjectsByType<NetworkedRulerSync>(FindObjectsSortMode.None);
        foreach (var sync in allSyncs)
        {
            sync.syncedData.Value = newData;
            sync.sharedSegment.Value = emptySegment;
            sync.sharedAngle.Value = emptyAngle;
        }
    }

    public void RequestShareSegment(int start, int end, bool direct, bool isSharing) => ShareSegmentServerRpc(start, end, direct, isSharing);

    [ServerRpc(RequireOwnership = false)]
    private void ShareSegmentServerRpc(int start, int end, bool direct, bool isSharing)
    {
        var newData = new SharedSegmentData { StartPoint = start, EndPoint = end, DirectConnection = direct, IsSharing = isSharing };
        var allSyncs = FindObjectsByType<NetworkedRulerSync>(FindObjectsSortMode.None);
        foreach (var sync in allSyncs) sync.sharedSegment.Value = newData;
    }
    
    public void RequestShareAngle(int start, int vertex, int end, bool isSharing) => ShareAngleServerRpc(start, vertex, end, isSharing);

    [ServerRpc(RequireOwnership = false)]
    private void ShareAngleServerRpc(int start, int vertex, int end, bool isSharing)
    {
        var newData = new SharedAngleData { StartPoint = start, VertexPoint = vertex, EndPoint = end, IsSharing = isSharing };
        var allSyncs = FindObjectsByType<NetworkedRulerSync>(FindObjectsSortMode.None);
        foreach (var sync in allSyncs) sync.sharedAngle.Value = newData;
    }

    public void RequestTriggerHighlight(int start, int end, bool direct)
    {
        TriggerHighlightServerRpc(start, end, direct);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TriggerHighlightServerRpc(int start, int end, bool direct)
    {
        TriggerHighlightClientRpc(start, end, direct);
    }

    [ClientRpc]
    private void TriggerHighlightClientRpc(int start, int end, bool direct)
    {
        if (NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            var localSync = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponentInChildren<NetworkedRulerSync>();
            if (localSync != null && localSync.rulerComponent != null)
                localSync.rulerComponent.HighlightSegmentLocalOnly(start, end, direct);
        }
    }
    
    public void RequestDeleteSegment(int start, int end)
    {
        DeleteSegmentServerRpc(start, end);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeleteSegmentServerRpc(int start, int end)
    {
        DeleteSegmentClientRpc(start, end);
    }

    [ClientRpc]
    private void DeleteSegmentClientRpc(int start, int end)
    {
        if (NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            var localSync = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponentInChildren<NetworkedRulerSync>();
            if (localSync != null && localSync.rulerComponent != null)
            {
                localSync.rulerComponent.DeleteSegmentLocalOnly(start, end);
            }
        }
    }
    
    public void RequestTriggerAngleHighlight(int start, int vertex, int end)
    {
        TriggerAngleHighlightServerRpc(start, vertex, end);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TriggerAngleHighlightServerRpc(int start, int vertex, int end)
    {
        TriggerAngleHighlightClientRpc(start, vertex, end);
    }

    [ClientRpc]
    private void TriggerAngleHighlightClientRpc(int start, int vertex, int end)
    {
        if (NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            var localSync = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponentInChildren<NetworkedRulerSync>();
            if (localSync != null && localSync.rulerComponent != null)
            {
                if (localSync.rulerComponent is ScreenAngleRuler angleRuler)
                    angleRuler.HighlightAngle(start, vertex, end);
            }
        }
    }
    
    public void RequestMarkScaleAsInitialized()
    {
        if (IsClient)
        {
            MarkScaleAsInitializedServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void MarkScaleAsInitializedServerRpc()
    {
        isModelScaleInitialized.Value = true;
    }
}