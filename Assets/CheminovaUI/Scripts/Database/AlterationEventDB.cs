using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// Proporciona acceso a la API de eventos de alteración.
/// Gestiona la configuración y la URL base de la API para eventos de alteración.
/// </summary>
public static class AlterationEventDB
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
    /// Elimina un evento de alteración de la API.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar al completar la eliminación, con el mensaje de error y el estado de éxito.</param>
    /// <param name="id">ID del evento de alteración a eliminar.</param>
    public static IEnumerator DeleteAlterationEventFromApi(UnityAction<ErrorMessage, bool> onCompleted, int id)
    {
        UnityWebRequest request = UnityWebRequest.Delete(ApiUrl + "/" + id);
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Successful elimination of alteration event: " + request.downloadHandler.text);
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
    /// Obtiene un evento de alteración por su ID.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar al completar la obtención, con los datos del evento de alteración y el estado de éxito.</param>
    /// <param name="id">ID del evento de alteración a obtener.</param>
    public static IEnumerator GetAlterationEventByID(UnityAction<AlterationEventData, bool> onCompleted, int id)
    {
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Alteration event obtained successfully: " + request.downloadHandler.text);
            
            var alterationEventData = JsonConvert.DeserializeObject<AlterationEventData>(request.downloadHandler.text);
            if (string.IsNullOrEmpty(alterationEventData.error))
            {
               //Debug.Log("Alteration event: " + alterationEventData.name);
                onCompleted?.Invoke(alterationEventData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + alterationEventData.error);
                onCompleted?.Invoke(null, false);
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
    /// Actualiza un evento de alteración en la API.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar al completar la actualización, con los datos del evento de alteración y el estado de éxito.</param>
    /// <param name="id">ID del evento de alteración a actualizar.</param>
    /// <param name="description">Descripción del evento de alteración.</param>
    /// <param name="idAlterationAgent">ID del agente de alteración.</param>
    /// <param name="idAlterations">Lista de IDs de alteraciones.</param>
    /// <param name="idAnnexData">Lista de IDs de datos anexos.</param>
    /// <param name="idMaterial">ID del material.</param>
    /// <param name="name">Nombre del evento de alteración.</param>
    public static IEnumerator PutAlterationEventFromApi(UnityAction<AlterationEventData, bool> onCompleted,
        int id,  string description = null, int? idAlterationAgent = null,
         List<int> idAlterations = null,  List<int> idAnnexData = null,
        int? idMaterial = null,  string name = null)
    {
        AlterationEventPost alterationEventPutData = new AlterationEventPost()
        {
            description = description,
            id_alteration_agent = idAlterationAgent,
            id_alterations = idAlterations,
            id_annex_data = idAnnexData,
            id_material = idMaterial,
            name = name
        };
        
        string jsonData = JsonConvert.SerializeObject(alterationEventPutData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id, "PUT");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Alteration event changed successfully: " + request.downloadHandler.text);
            var alterationEventData = JsonConvert.DeserializeObject<AlterationEventData>(request.downloadHandler.text);
            if (string.IsNullOrEmpty(alterationEventData.error))
            {
               //Debug.Log("Alteration event changed: " + alterationEventData.name);
                onCompleted?.Invoke(alterationEventData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + alterationEventData.error);
                onCompleted?.Invoke(null, false);
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