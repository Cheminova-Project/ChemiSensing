using System.Collections;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// Proporciona acceso a la API de servicios de GUID.
/// Gestiona la configuración y la URL base de la API para GUIDs.
/// </summary>
public static class GuidServiceDB
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
    
    public static IEnumerator GetGuidServiceByID(UnityAction<GuidServiceData, bool> onCompleted, int id)
    {
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("GuidService obtained successfully: " + request.downloadHandler.text);
            GuidServiceData guidServiceData = JsonConvert.DeserializeObject<GuidServiceData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(guidServiceData.error))
            {
               //Debug.Log("GuidService: " + guidServiceData.entity_type);
                onCompleted?.Invoke(guidServiceData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + guidServiceData.error);
                onCompleted?.Invoke(guidServiceData, false);
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

/// <summary>
/// Representa la respuesta de la API de servicios de GUID.
/// </summary>
public class GuidServiceData
{
    /// <summary>
    /// Entidad asociada al GUID.
    /// </summary>
    public object entity;
    /// <summary>
    /// ID de la entidad.
    /// </summary>
    public int entity_id;
    /// <summary>
    /// Tipo de la entidad.
    /// </summary>
    public string entity_type;
    /// <summary>
    /// Mensaje de error, si existe.
    /// </summary>
    public string error;
}