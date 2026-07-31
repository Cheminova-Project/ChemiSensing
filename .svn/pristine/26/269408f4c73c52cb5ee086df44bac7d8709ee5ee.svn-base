using System.Collections;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public static class SpatialRelationDB
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
    
    public static IEnumerator DeleteSpatialRelationFromApi(UnityAction<ErrorMessage, bool> onCompleted, int id)
    {
        UnityWebRequest request = UnityWebRequest.Delete(ApiUrl + "/" + id);
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Successful elimination of SpatialRelation: " + request.downloadHandler.text);
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
    
    public static IEnumerator GetSpatialRelationByID(UnityAction<SpatialRelationData, bool> onCompleted, int id)
    {
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("SpatialRelation obtained successfully: " + request.downloadHandler.text);
            SpatialRelationData spatialRelationData = JsonConvert.DeserializeObject<SpatialRelationData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(spatialRelationData.error))
            {
               //Debug.Log("SpatialRelation: " + spatialRelationData.id);
                onCompleted?.Invoke(spatialRelationData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + spatialRelationData.error);
                onCompleted?.Invoke(spatialRelationData, false);
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
    
    public static IEnumerator PutSpatialRelationFromApi(UnityAction<SpatialRelationData, bool> onCompleted,
        int id, int? idIncluded = null, InclusionType? inclusionType = null, int? transformation = null)
    {
        SpatialRelationPost spatialRelationPutData = new SpatialRelationPost()
        {
            id_included = idIncluded,
            inclusion_type = inclusionType.ToString(),
            transformation = transformation
        };
        
        string jsonData = JsonConvert.SerializeObject(spatialRelationPutData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id, "PUT");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("SpatialRelation changed successfully: " + request.downloadHandler.text);
            SpatialRelationData spatialRelationData = JsonConvert.DeserializeObject<SpatialRelationData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(spatialRelationData.error))
            {
               //Debug.Log("SpatialRelation changed: " + spatialRelationData.id);
                onCompleted?.Invoke(spatialRelationData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + spatialRelationData.error);
                onCompleted?.Invoke(spatialRelationData, false);
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