using UnityEngine;

/// <summary>
/// Configuración de la dirección IP del servidor para la aplicación de red.
/// Permite definir y modificar la IP utilizada para la conexión.
/// </summary>
[CreateAssetMenu(menuName = "Networking/ServerIPConfiguration")]
public class ServerIPConfiguration : ScriptableObject
{
    /// <summary>
    /// Dirección IP del servidor.
    /// </summary>
    public string serverIP;
}
