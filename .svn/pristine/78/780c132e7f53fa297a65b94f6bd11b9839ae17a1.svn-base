using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// Proporciona acceso a la API de alteraciones.
/// Gestiona la configuración y la URL base de la API para alteraciones.
/// </summary>
public static class AlterationDB
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
    /// Obtiene la lista de alteraciones.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando la solicitud se completa.</param>
    /// <param name="page">Número de página para la paginación.</param>
    /// <param name="perPage">Número de elementos por página.</param>
    /// <param name="search">Cadena de búsqueda para filtrar alteraciones.</param>
    /// <param name="sort">Campo por el cual ordenar los resultados.</param>
    /// <param name="order">Orden de clasificación (ascendente/descendente).</param>
    /// <param name="alterationId">Lista de IDs de alteraciones para filtrar.</param>
    /// <param name="materialId">Lista de IDs de materiales para filtrar.</param>
    /// <param name="yearFrom">Año de inicio para el filtro por año.</param>
    /// <param name="yearTo">Año de fin para el filtro por año.</param>
    public static IEnumerator GetAlterationList(UnityAction<AlterationResponse, bool> onCompleted,
        int? page = null, int? perPage = null,  string search = null,
         string sort = null,  string order = null,  List<int> alterationId = null,
         List<int> materialId = null, int? yearFrom = null, int? yearTo = null)
    {
        QueryParams queryParams = new QueryParams()
        {
            page = page,
            per_page = perPage,
            search = search,
            sort = sort,
            order = order,
            alteration_id = alterationId,
            material_id = materialId,
            year_from = yearFrom,
            year_to = yearTo
        };
        
        string query = ApiUrl + QueryManagement.ToQueryString(queryParams);
        //Debug.Log("API URL: " + query);
        UnityWebRequest request = new UnityWebRequest(query, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Alterations obtained successfully: " + request.downloadHandler.text);
            AlterationResponse alterationResponse = JsonConvert.DeserializeObject<AlterationResponse>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(alterationResponse.error))
            {
               //Debug.Log("List of alteration saved with the size of " + alterationResponse.items.Count);
                onCompleted?.Invoke(alterationResponse, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + alterationResponse.error);
                onCompleted?.Invoke(alterationResponse, false);
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
    /// Crea una nueva alteración.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando la solicitud se completa.</param>
    /// <param name="idAlterationForm">ID del formulario de alteración.</param>
    /// <param name="name">Nombre de la alteración.</param>
    public static IEnumerator PostNewAlteration(UnityAction<AlterationData, bool> onCompleted,
        int idAlterationForm, string name)
    {
        AlterationPost alterationPostData = new AlterationPost()
        {
            id_alteration_form = idAlterationForm,
            name = name
        };

        string jsonData = JsonConvert.SerializeObject(alterationPostData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        UnityWebRequest request = new UnityWebRequest(ApiUrl, "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Successful creation of alteration: " + request.downloadHandler.text);
            AlterationData alterationData = JsonConvert.DeserializeObject<AlterationData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(alterationData.error))
            {
               //Debug.Log("Alteration added " + alterationData.name);
                onCompleted?.Invoke(alterationData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + alterationData.error);
                onCompleted?.Invoke(alterationData, false);
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
    /// Elimina una alteración por su ID.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando la solicitud se completa.</param>
    /// <param name="id">ID de la alteración a eliminar.</param>
    public static IEnumerator DeleteAlterationFromApi(UnityAction<ErrorMessage, bool> onCompleted, int id)
    {
        UnityWebRequest request = UnityWebRequest.Delete(ApiUrl + "/" + id);
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Successful elimination of alteration: " + request.downloadHandler.text);
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
    /// Obtiene una alteración por su ID.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando la solicitud se completa.</param>
    /// <param name="id">ID de la alteración a obtener.</param>
    public static IEnumerator GetAlterationByID(UnityAction<AlterationData, bool> onCompleted, int id)
    {
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Alteration obtained successfully: " + request.downloadHandler.text);
            AlterationData alterationData = JsonConvert.DeserializeObject<AlterationData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(alterationData.error))
            {
               //Debug.Log("Alteration: " + alterationData.name);
                onCompleted?.Invoke(alterationData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + alterationData.error);
                onCompleted?.Invoke(alterationData, false);
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
    /// Actualiza una alteración por su ID.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando la solicitud se completa.</param>
    /// <param name="id">ID de la alteración a actualizar.</param>
    /// <param name="idAlterationForm">Nuevo ID del formulario de alteración.</param>
    /// <param name="name">Nuevo nombre de la alteración.</param>
    public static IEnumerator PutAlterationFromApi(UnityAction<AlterationData, bool> onCompleted,
        int id, int? idAlterationForm = null,  string name = null)
    {
        AlterationPost alterationPutData = new AlterationPost()
        {
            id_alteration_form = idAlterationForm,
            name = name
        };
        
        string jsonData = JsonConvert.SerializeObject(alterationPutData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id, "PUT");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Alteration changed successfully: " + request.downloadHandler.text);
            AlterationData alterationData = JsonConvert.DeserializeObject<AlterationData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(alterationData.error))
            {
               //Debug.Log("Alteration changed: " + alterationData.name);
                onCompleted?.Invoke(alterationData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + alterationData.error);
                onCompleted?.Invoke(alterationData, false);
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