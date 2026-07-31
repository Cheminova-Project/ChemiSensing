using System.Collections;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// Proporciona acceso a la API de municipios.
/// Gestiona la configuración y la URL base de la API para municipios.
/// </summary>
public static class MunicipalityDB
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
    /// Obtiene la lista de municipios.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando la solicitud se completa.</param>
    /// <param name="page">Número de página para la paginación.</param>
    /// <param name="perPage">Número de municipios por página.</param>
    /// <param name="search">Cadena de búsqueda para filtrar municipios.</param>
    /// <param name="sort">Campo por el cual ordenar los resultados.</param>
    /// <param name="order">Orden de clasificación (ascendente/descendente).</param>
    /// <param name="country">Filtro por país.</param>
    /// <param name="admin1">Filtro por administración de nivel 1.</param>
    public static IEnumerator GetMunicipalityList(UnityAction<MunicipalityResponse, bool> onCompleted,
        int? page = null, int? perPage = null,  string search = null,
         string sort = null,  string order = null,
         string country = null,  string admin1 = null)
    {
        QueryParams queryParams = new QueryParams()
        {
            page = page,
            per_page = perPage,
            search = search,
            sort = sort,
            order = order,
            country = country,
            admin1 = admin1
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
            //Debug.Log("Municipalities obtained successfully: " + request.downloadHandler.text);
            MunicipalityResponse municipalityResponse = JsonConvert.DeserializeObject<MunicipalityResponse>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(municipalityResponse.error))
            {
                //Debug.Log("List of municipality saved with the size of " + municipalityResponse.items.Count);
                onCompleted?.Invoke(municipalityResponse, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + municipalityResponse.error);
                onCompleted?.Invoke(municipalityResponse, false);
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
    /// Obtiene un municipio por su ID.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando la solicitud se completa.</param>
    /// <param name="id">ID del municipio a obtener.</param>
    public static IEnumerator GetMunicipalityByID(UnityAction<GeoData, bool> onCompleted, int id)
    {
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log("Municipality obtained successfully: " + request.downloadHandler.text);
            GeoData municipalityData = JsonConvert.DeserializeObject<GeoData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(municipalityData.error))
            {
                //Debug.Log("Municipality: " + municipalityData.name);
                onCompleted?.Invoke(municipalityData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + municipalityData.error);
                onCompleted?.Invoke(municipalityData, false);
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