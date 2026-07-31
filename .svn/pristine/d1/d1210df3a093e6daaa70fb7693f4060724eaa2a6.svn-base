using System.Collections;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// Proporciona acceso a la API de registro de usuarios.
/// Gestiona la configuración y la URL base de la API para registro.
/// </summary>
public static class RegisterDB
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
    
    public static IEnumerator Register(UnityAction<UserData, bool> onCompleted, int active,
        int activityStart, string affiliation, string email, int idCountry, string name,
        string password, string profession, UserRole role, string surname, string username)
    {
        UserPost userDataPost = new UserPost()
        {
            active = active,
            activity_start = activityStart,
            affiliation = affiliation,
            email = email,
            id_country = idCountry,
            name = name,
            password = password,
            profession = profession,
            role = role.ToString(), // Convert the enum to string
            surname = surname,
            username = username
        };

        string jsonData = JsonConvert.SerializeObject(userDataPost);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(ApiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Successful register: " + request.downloadHandler.text);
            UserData userData = JsonConvert.DeserializeObject<UserData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(userData.error))
            {
               //Debug.Log("User registered " + userData.username);
                onCompleted?.Invoke(userData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + userData.error);
                onCompleted?.Invoke(userData, false);
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