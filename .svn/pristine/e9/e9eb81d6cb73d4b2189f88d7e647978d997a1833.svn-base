using System.Collections;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// Proporciona acceso a la API de clientes.
/// Gestiona la configuración y la URL base de la API para clientes.
/// </summary>
public static class ClientDB
{
    private static APIEndpointConfig config;
    public static string ApiEndpointUrl => Config != null ? Config.endpointURL : "";
    private static APIEndpointConfig Config
    {
        get
        {
            string className = MethodBase.GetCurrentMethod().DeclaringType.Name;
            if(className == null)
            {
                Debug.LogWarning("Couldn't obtained class name.");
                return null;
            }
            else
            {
                //Debug.Log("Name of the class: " + className);
            }
     
            config = Resources.Load<APIEndpointConfig>("API_URLS_ENDPOINT/" + className);
            if (config == null)
            {
                Debug.LogWarning("No APIBaseConfig found in Resources. Create an asset with that name in Resources.");
            }
            return config;
        }
    }
    public static string ApiUrl => GlobalManagement.Instance.urlBase + ApiEndpointUrl;
    
    /// <summary>
    /// Crea un nuevo cliente en la API.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando se completa la solicitud.</param>
    /// <param name="clientName">Nombre del cliente.</param>
    /// <param name="clientUri">URI del cliente.</param>
    /// <param name="redirectUris">URIs de redirección.</param>
    public static IEnumerator PostNewClient(UnityAction<ClientData, bool> onCompleted,
         string clientName = null,  string clientUri = null,
         string redirectUris = null)
    {
        ClientPost clientPostData = new ClientPost()
        {
            client_name = clientName,
            client_uri = clientUri,
            redirect_uris = redirectUris
        };
        
        string jsonData = JsonConvert.SerializeObject(clientPostData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        UnityWebRequest request = new UnityWebRequest(ApiUrl, "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Successful creation of client: " + request.downloadHandler.text);
            ClientData clientData = JsonConvert.DeserializeObject<ClientData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(clientData.error))
            {
               //Debug.Log("Client added " + clientData.client_id);
                onCompleted?.Invoke(clientData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + clientData.error);
                onCompleted?.Invoke(clientData, false);
            }
        }
        else
        {
            Debug.LogWarning("Error: " + request.error);
            Debug.LogWarning("Server reply: " + request.downloadHandler.text);
            onCompleted?.Invoke(null, false);
        }
        
        request.Dispose();
    }
    
    /// <summary>
    /// Elimina un cliente de la API.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando se completa la solicitud.</param>
    /// <param name="id">ID del cliente a eliminar.</param>
    public static IEnumerator DeleteClientFromApi(UnityAction<ErrorMessage, bool> onCompleted, int id)
    {
        UnityWebRequest request = UnityWebRequest.Delete(ApiUrl + "/" + id);
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Successful elimination of client: " + request.downloadHandler.text);
            ErrorMessage errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(errorMessage.error))
            {
               //Debug.Log("Element deleted");
                onCompleted?.Invoke(null, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + errorMessage.error);
                onCompleted?.Invoke(errorMessage, false);
            }
        }
        else
        {
            Debug.LogWarning("Error: " + request.error);
            Debug.LogWarning("Server reply: " + request.downloadHandler.text);
            onCompleted?.Invoke(null, false);
        }
        
        request.Dispose();
    }
    
    /// <summary>
    /// Obtiene un cliente por su ID.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando se completa la solicitud.</param>
    /// <param name="id">ID del cliente a obtener.</param>
    public static IEnumerator GetClientByID(UnityAction<ClientData, bool> onCompleted, int id)
    {
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Client obtained successfully: " + request.downloadHandler.text);
            ClientData clientData = JsonConvert.DeserializeObject<ClientData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(clientData.error))
            {
               //Debug.Log("Client: " + clientData.id);
                onCompleted?.Invoke(clientData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + clientData.error);
                onCompleted?.Invoke(clientData, false);
            }
        }
        else
        {
            Debug.LogWarning("Error: " + request.error);
            Debug.LogWarning("Server reply: " + request.downloadHandler.text);
            onCompleted?.Invoke(null, false);
        }
        
        request.Dispose();
    }
}