using System;
using UnityEngine;

/// <summary>
/// Gestiona variables globales y credenciales de usuario en la aplicación.
/// Implementa el patrón Singleton para asegurar una única instancia.
/// </summary>
public class GlobalManagement : MonoBehaviour
{
    /// <summary>
    /// Instancia única de GlobalManagement.
    /// </summary>
    public static GlobalManagement Instance { get; private set; }
    /// <summary>
    /// URL base de la API.
    /// </summary>
    public string urlBase;
    /// <summary>
    /// Id del cliente.
    /// </summary>
    public string clientId;
    /// <summary>
    /// Token de autenticación del usuario.
    /// </summary>
    public string token;
    /// <summary>
    /// Nombre de usuario autenticado.
    /// </summary>
    public string username;
    public int userID;
    public string userRole;

    /// <summary>
    /// Inicializa el Singleton y asegura que solo exista una instancia.
    /// </summary>
    private void Awake()
    {
        // Singleton pattern to ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        //Set the base URL from the APIBaseConfig resource
        APIBaseConfig BaseConfig = Resources.Load<APIBaseConfig>("API_URLS_BASE/URLBase");
        string ApiBaseUrl = BaseConfig != null ? BaseConfig.GetBaseURL() : "";
        urlBase = ApiBaseUrl;
        ClientIdConfig ClientIdConfig = Resources.Load<ClientIdConfig>("CLIENT_ID_BASE/ClientId");
        string ApiClientId = ClientIdConfig != null ? ClientIdConfig.GetClientId() : "";
        clientId = ApiClientId;

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Obtiene el nombre de usuario actual.
    /// </summary>
    /// <returns>Nombre de usuario.</returns>
    public string GetUsername()
    {
        return username;
    }
    
    void OnApplicationQuit()
    {
        PlayerPrefs.DeleteAll();
    }
}

/// <summary>
/// Representa un mensaje de error simple.
/// </summary>
[Serializable]
public class ErrorMessage
{
    /// <summary>
    /// Texto descriptivo del error.
    /// </summary>
    public string error;

    /// <summary>
    /// Constructor que inicializa el mensaje de error.
    /// </summary>
    /// <param name="error">Texto del error.</param>
    public ErrorMessage(string error)
    {
        this.error = error;
    }
}