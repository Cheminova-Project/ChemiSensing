using System.Collections;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// Proporciona acceso a la API de materiales.
/// Gestiona la configuración y la URL base de la API para materiales.
/// </summary>
public static class MaterialDB
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
    /// Obtiene la lista de materiales de la API.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando se completa la solicitud.</param>
    /// <param name="page">Número de página para la paginación.</param>
    /// <param name="perPage">Número de elementos por página.</param>
    /// <param name="search">Cadena de búsqueda para filtrar materiales.</param>
    /// <param name="sort">Campo por el cual ordenar los materiales.</param>
    /// <param name="order">Orden de clasificación (ascendente/descendente).</param>
    public static IEnumerator GetMaterialList(UnityAction<MaterialResponse, bool> onCompleted,
        int? page = null, int? perPage = null,  string search = null,
         string sort = null,  string order = null)
    {
        QueryParams queryParams = new QueryParams()
        {
            page = page,
            per_page = perPage,
            search = search,
            sort = sort,
            order = order
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
            MaterialResponse materialResponse = JsonConvert.DeserializeObject<MaterialResponse>(request.downloadHandler.text);
            if (string.IsNullOrEmpty(materialResponse.error))
            {
                //Debug.Log("List of material saved with the size of " + materialResponse.items.Count);
                onCompleted?.Invoke(materialResponse, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + materialResponse.error);
                onCompleted?.Invoke(materialResponse, false);
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
    /// Crea un nuevo material en la API.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando se completa la solicitud.</param>
    /// <param name="name">Nombre del nuevo material.</param>
    public static IEnumerator PostNewMaterial(UnityAction<MaterialData, bool> onCompleted, string name)
    {
        MaterialPost materialPostData = new MaterialPost()
        {
            name = name
        };

        string jsonData = JsonConvert.SerializeObject(materialPostData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        UnityWebRequest request = new UnityWebRequest(ApiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Successful creation of material: " + request.downloadHandler.text);
            MaterialData materialData = JsonConvert.DeserializeObject<MaterialData>(request.downloadHandler.text);
            if (string.IsNullOrEmpty(materialData.error))
            {
               //Debug.Log("Material added " + materialData.name);
                onCompleted?.Invoke(materialData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + materialData.error);
                onCompleted?.Invoke(materialData, false);
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
    /// Elimina un material de la API.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando se completa la solicitud.</param>
    /// <param name="id">ID del material a eliminar.</param>
    public static IEnumerator DeleteMaterialFromApi(UnityAction<ErrorMessage, bool> onCompleted, int id)
    {
        UnityWebRequest request = UnityWebRequest.Delete(ApiUrl + "/" + id);
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            ErrorMessage errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(request.downloadHandler.text);
            if (errorMessage == null)
            {
                Debug.LogWarning("Element deleted");
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
    /// Obtiene un material por su ID.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando se completa la solicitud.</param>
    /// <param name="id">ID del material a obtener.</param>
    public static IEnumerator GetMaterialByID(UnityAction<MaterialData, bool> onCompleted, int id)
    {
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            MaterialData materialData = JsonConvert.DeserializeObject<MaterialData>(request.downloadHandler.text);
            if (string.IsNullOrEmpty(materialData.error))
            {
               //Debug.Log("Material: " + materialData.name);
                onCompleted?.Invoke(materialData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + materialData.error);
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
    /// Actualiza un material en la API.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando se completa la solicitud.</param>
    /// <param name="id">ID del material a actualizar.</param>
    /// <param name="name">Nuevo nombre para el material.</param>
    public static IEnumerator PutMaterialFromApi(UnityAction<MaterialData, bool> onCompleted, int id,
         string name = null)
    {
        MaterialPost materialPutData = new MaterialPost()
        {
            name = name
        };
        
        string jsonData = JsonConvert.SerializeObject(materialPutData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id, "PUT");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Material changed successfully: " + request.downloadHandler.text);
            MaterialData materialData = JsonConvert.DeserializeObject<MaterialData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(materialData.error))
            {
               //Debug.Log("Material changed: " + materialData.name);
                onCompleted?.Invoke(materialData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + materialData.error);
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