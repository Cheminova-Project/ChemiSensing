using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

/// <summary>
/// Sincroniza líneas renderizadas entre clientes en red.
/// Permite dibujar y replicar líneas en tiempo real en la red, incluyendo su color.
/// </summary>
public class NetworkedLineRenderer : NetworkBehaviour
{
    /// <summary>
    /// Referencia al componente LineRenderer.
    /// </summary>
    public LineRenderer lineRenderer;

    [SerializeField] private bool hiddenInOwner = true;
    private Color transparentColor = new Color(0, 0, 0, 0);

    // Variable de red para sincronizar los puntos del láser
    private NetworkVariable<SyncedLineData> syncedLineData = new(
        writePerm: NetworkVariableWritePermission.Server
    );

    // --- NUEVO: Variable de red para sincronizar el color ---
    public NetworkVariable<Color> lineColor = new NetworkVariable<Color>(
        Color.white, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if(NetworkObject.IsOwner && hiddenInOwner)
        {
            lineRenderer.enabled = false;
        }
        
        // Suscripciones a los cambios
        syncedLineData.OnValueChanged += OnLineDataChanged;
        lineColor.OnValueChanged += OnColorChanged;

        // Forzar actualización inicial del color por si entra tarde un cliente
        ApplyColor(lineColor.Value);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        // Limpiar suscripciones
        syncedLineData.OnValueChanged -= OnLineDataChanged;
        lineColor.OnValueChanged -= OnColorChanged;
    }

    // --- LÓGICA DE PUNTOS ---
    private void OnLineDataChanged(SyncedLineData previous, SyncedLineData current)
    {
        if (current.Points == null) return;
        if(lineRenderer == null) return;
        lineRenderer.positionCount = current.Points.Count;
        lineRenderer.SetPositions(current.Points.ToArray());
    }

    public void SetPoints(List<Vector3> newPoints)
    {
        SetPointsServerRpc(new SyncedLineData
        {
            Points = newPoints ?? new List<Vector3>()
        });
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetPointsServerRpc(SyncedLineData syncedLine)
    {
        var newPoints = syncedLine.Points;
        if (newPoints == null || newPoints.Count == 0)
        {
            syncedLineData.Value = new SyncedLineData
            {
                Points = new List<Vector3>()
            };
            return;
        }

        syncedLineData.Value = new SyncedLineData { Points = newPoints };
    }

    // --- NUEVA LÓGICA DE COLOR ---
    public void SetColor(Color newColor)
    {
        // Llamamos al servidor para que cambie el color para todos
        SetColorServerRpc(newColor);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetColorServerRpc(Color newColor)
    {
        lineColor.Value = newColor;
    }

    private void OnColorChanged(Color previous, Color current)
    {
        ApplyColor(current);
    }

    private void ApplyColor(Color colorToApply)
    {
        if (lineRenderer != null)
        {
            lineRenderer.startColor = colorToApply;
            lineRenderer.endColor = colorToApply;
            lineRenderer.material.color = colorToApply;
        }
    }
}

// Struct intacto
public struct SyncedLineData : INetworkSerializable
{
    public List<Vector3> Points;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsWriter)
        {
            int count = Points?.Count ?? 0;
            serializer.SerializeValue(ref count);

            for (int i = 0; i < count; i++)
            {
                Vector3 point = Points[i];
                serializer.SerializeValue(ref point);
            }
        }
        else
        {
            int count = 0;
            serializer.SerializeValue(ref count);
            Points = new List<Vector3>(count);

            for (int i = 0; i < count; i++)
            {
                Vector3 point = Vector3.zero;
                serializer.SerializeValue(ref point);
                Points.Add(point);
            }
        }
    }
}