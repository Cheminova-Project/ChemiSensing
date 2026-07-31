using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/API Base Config")]
public class APIBaseConfig : ScriptableObject
{
    [Header("API URL Base - Production")]
    public string productionBaseURL = "https://api.cheminova.eu";

    [Header("API URL Base - Development")]
    public string developmentBaseURL = "https://cgisdev.utcluj.ro/cheminovaStagingAPI";

    /// <summary>
    /// Devuelve la URL base dependiendo del modo de conexión actual.
    /// </summary>
    public string GetBaseURL()
    {
        if (NetworkServerConfiguration.Instance != null && 
            NetworkServerConfiguration.Instance.connectMode == ConnectMode.PRODUCTION_REMOTE)
        {
            return productionBaseURL;
        }
        
        return developmentBaseURL; 
    }
}