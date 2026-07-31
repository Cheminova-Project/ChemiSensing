using System.Collections;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// Proporciona acceso a la API de archivos de reportes de condición.
/// Gestiona la configuración y la URL base de la API para archivos de reportes de condición.
/// </summary>
public static class ConditionReportFileDB
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
    /// Elimina un archivo de reporte de condición en la API.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar al completar la eliminación, con el mensaje de error y el estado de éxito.</param>
    /// <param name="id">ID del archivo de reporte de condición a eliminar.</param>
    /// <returns>Coroutine para la eliminación del archivo de reporte de condición.</returns>
    public static IEnumerator DeleteConditionReportFileFromApi(UnityAction<ErrorMessage, bool> onCompleted, int id)
    {
        UnityWebRequest request = UnityWebRequest.Delete(ApiUrl + "/" + id);
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Successful elimination of ConditionReportFile: " + request.downloadHandler.text);
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
    /// Obtiene un archivo de reporte de condición por su ID.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar al completar la obtención, con los datos del archivo de reporte de condición y el estado de éxito.</param>
    /// <param name="id">ID del archivo de reporte de condición a obtener.</param>
    /// <returns>Coroutine para la obtención del archivo de reporte de condición.</returns>
    public static IEnumerator GetConditionReportFileByID(UnityAction<ConditionReportFileData, bool> onCompleted, int id)
    {
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("ConditionReportFile obtained successfully: " + request.downloadHandler.text);
            ConditionReportFileData conditionReportFileData = JsonConvert.DeserializeObject<ConditionReportFileData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(conditionReportFileData.error))
            {
               //Debug.Log("ConditionReportFile: " + conditionReportFileData.id);
                onCompleted?.Invoke(conditionReportFileData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + conditionReportFileData.error);
                onCompleted?.Invoke(conditionReportFileData, false);
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
    /// Actualiza un archivo de reporte de condición en la API.
    /// </summary>
    /// <param name="idFile">ID del archivo a actualizar.</param>
    /// <param name="onCompleted">Acción a ejecutar al completar la actualización, con los datos del archivo de reporte de condición y el estado de éxito.</param>
    /// <param name="id">ID del archivo de reporte de condición a actualizar.</param>
    /// <returns>Coroutine para la actualización del archivo de reporte de condición.</returns>
    public static IEnumerator PutConditionReportFromApi(int idFile, UnityAction<ConditionReportFileData, bool> onCompleted, int id)
    {
        ConditionReportFilePost conditionReportFilePutData = new ConditionReportFilePost()
        {
            id_file = idFile
        };
        
        string jsonData = JsonConvert.SerializeObject(conditionReportFilePutData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id, "PUT");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("ConditionReportFile changed successfully: " + request.downloadHandler.text);
            ConditionReportFileData conditionReportFileData = JsonConvert.DeserializeObject<ConditionReportFileData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(conditionReportFileData.error))
            {
               //Debug.Log("ConditionReportFile changed: " + conditionReportFileData.id);
                onCompleted?.Invoke(conditionReportFileData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + conditionReportFileData.error);
                onCompleted?.Invoke(conditionReportFileData, false);
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