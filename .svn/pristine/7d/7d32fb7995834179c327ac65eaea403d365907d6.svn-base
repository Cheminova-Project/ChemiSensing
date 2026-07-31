using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// Proporciona acceso a la API de usos de elementos patrimoniales (CH Usage).
/// Gestiona la configuración y la URL base de la API para usos de elementos patrimoniales.
/// </summary>
public static class CHUsageDB
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
    
    public static IEnumerator GetCHUsageList(UnityAction<CHUsageResponse, bool> onCompleted,
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
            //Debug.Log("CHUsages obtained successfully: " + request.downloadHandler.text);
            CHUsageResponse chUsageResponse = JsonConvert.DeserializeObject<CHUsageResponse>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(chUsageResponse.error))
            {
                //Debug.Log("List of CHUsages saved with the size of " + chUsageResponse.items.Count);
                onCompleted?.Invoke(chUsageResponse, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + chUsageResponse.error);
                onCompleted?.Invoke(chUsageResponse, false);
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