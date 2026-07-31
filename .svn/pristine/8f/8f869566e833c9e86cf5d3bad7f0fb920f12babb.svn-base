using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[DisallowMultipleComponent]
/// <summary>
/// Extiende NetworkTransform para enviar la matriz relativa entre el usuario y un objetivo.
/// </summary>
public class ExtendedNetworkTransform : NetworkTransform
{

    private Transform targetTransform;

    // Esta copia local la actualiza el dueño, y los demás la reciben por RPC
    private Matrix4x4 relativeMatrix;
    public Matrix4x4 RelativeMatrix => relativeMatrix;

    /// <summary>
    /// Establece el Transform objetivo para calcular la matriz relativa.
    /// </summary>
    public void SetTargetTransform(Transform target)
    {
        targetTransform = target;
    }


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    private void LateUpdate()
    {
        if (!IsOwner || targetTransform == null)
            return;

        // Calcula la matriz relativa
        Matrix4x4 userMatrix = transform.localToWorldMatrix;
        Matrix4x4 targetMatrix = targetTransform.localToWorldMatrix;
        Matrix4x4 relative = targetMatrix.inverse * userMatrix;

        // Enviar a todos los demás con Unreliable
        SendRelativeMatrixClientRpc(new Matrix4x4Serializable(relative));
    }

    /// <summary>
    /// Envía la matriz relativa a todos los clientes.
    /// </summary>
    [ClientRpc(Delivery = RpcDelivery.Unreliable)]
    private void SendRelativeMatrixClientRpc(Matrix4x4Serializable matrix, ClientRpcParams rpcParams = default)
    {
        if (IsOwner) return; // El dueño ya la tiene

        relativeMatrix = matrix.ToMatrix4x4();
    }
}


[System.Serializable]
/// <summary>
/// Estructura serializable para enviar Matrix4x4 a través de la red.
/// </summary>
public struct Matrix4x4Serializable : INetworkSerializable
{
    public float[] elements;

    /// <summary>
    /// Constructor que convierte un Matrix4x4 en una estructura serializable.
    /// </summary>
    public Matrix4x4Serializable(Matrix4x4 matrix)
    {
        elements = new float[16];
        for (int i = 0; i < 16; i++)
            elements[i] = matrix[i];
    }

    /// <summary>
    /// Convierte la estructura serializable de vuelta a un Matrix4x4.
    /// </summary>
    public Matrix4x4 ToMatrix4x4()
    {
        Matrix4x4 mat = new Matrix4x4();
        for (int i = 0; i < 16; i++)
            mat[i] = elements[i];
        return mat;
    }

    /// <summary>
    /// Serializa o deserializa la estructura para la red.
    /// </summary>
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (elements == null || elements.Length != 16)
            elements = new float[16];
        for (int i = 0; i < 16; i++)
            serializer.SerializeValue(ref elements[i]);
    }
}
