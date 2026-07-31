using System.Collections;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public static class SigninDB
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
    
    public static IEnumerator Signin(UnityAction<SigninResponse, bool> onCompleted,
        string password, string username)
    {
        SigninPost signinPost = new SigninPost(password, username);

        string jsonData = JsonConvert.SerializeObject(signinPost);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        UnityWebRequest request = new UnityWebRequest(ApiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log("Successful signin: " + request.downloadHandler.text);
            SigninResponse signinResponse = JsonConvert.DeserializeObject<SigninResponse>(request.downloadHandler.text);
            if (string.IsNullOrEmpty(signinResponse.error))
            {
                //Debug.Log("Access token saved: " + signinResponse.access_token);
                onCompleted?.Invoke(signinResponse, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + signinResponse.error);
                onCompleted?.Invoke(signinResponse, false);
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