using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Client Id Config")]
public class ClientIdConfig : ScriptableObject
{
    [Header("Client Id - Production")]
    public string productionClientId = "OLDZA45SAcmzPqV1rpqAGZpK";

    [Header("Client Id - Development")]
    public string developmentClientId = "vTCfl9mPwipn83fwTs8icEaw";
    
    /// <summary>
    /// Devuelve el id del cliente dependiendo del modo de conexión actual.
    /// </summary>
    public string GetClientId()
    {
        if (NetworkServerConfiguration.Instance != null && 
            NetworkServerConfiguration.Instance.connectMode == ConnectMode.PRODUCTION_REMOTE)
        {
            return productionClientId;
        }
        
        return developmentClientId; 
    }
}