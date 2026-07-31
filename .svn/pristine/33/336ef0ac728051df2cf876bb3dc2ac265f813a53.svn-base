using System.Collections;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// Proporciona acceso a la API de descargas de archivos.
/// Gestiona la configuración y la URL base de la API para descargas.
/// </summary>
public static class DownloadDB
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
    /// Descarga un archivo por su ID.
    /// </summary>
    /// <param name="id">ID del archivo a descargar.</param>
    /// <param name="onCompleted">Callback que se llama cuando la descarga se completa.</param>
    public static IEnumerator GetDownloadByID(int? id, UnityAction<RawFileDownload, bool> onCompleted)
    {
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            byte[] data = request.downloadHandler.data;

            // Creamos RawFileDownload con error = null (porque no se espera JSON de error en este endpoint)
            RawFileDownload rawFileDownload = new RawFileDownload(null, data);
            onCompleted?.Invoke(rawFileDownload, true);
        }
        else
        {
            //Debug.LogWarning("Error: " + request.error);
            //Debug.Log("URL: " + request.url);
            onCompleted?.Invoke(null, false);
        }
        
        request.Dispose();
    }
    
    /// <summary>
    /// Genera un código de descarga para un archivo.
    /// </summary>
    /// <param name="onCompleted">Callback que se llama cuando la generación del código se completa.</param>
    /// <param name="id">ID del archivo para el cual se genera el código de descarga.</param>
    public static IEnumerator GetGenerateDownloadCodeByID(UnityAction<RawFileDownload, bool> onCompleted, int id)
    {
        string url = GlobalManagement.Instance.urlBase + "/generate_download_code/" + id;
        UnityWebRequest request = new UnityWebRequest(url, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            ErrorMessage errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(request.downloadHandler.text);
            
            if (errorMessage == null || string.IsNullOrEmpty(errorMessage.error))
            {
                byte[] data = request.downloadHandler.data;
                
                onCompleted?.Invoke(new RawFileDownload(errorMessage, data), true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + errorMessage.error);
                onCompleted?.Invoke(new RawFileDownload(errorMessage, null), false);
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
/// Representa la respuesta de descarga de un archivo crudo.
/// </summary>
public class RawFileDownload
{
    /// <summary>
    /// Mensaje de error asociado a la descarga.
    /// </summary>
    public ErrorMessage error;
    /// <summary>
    /// Datos binarios descargados.
    /// </summary>
    public byte[] data;
    
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="RawFileDownload"/>.
    /// </summary>
    /// <param name="error">Mensaje de error asociado a la descarga.</param>
    /// <param name="data">Datos binarios descargados.</param>
    public RawFileDownload(ErrorMessage error, byte[] data)
    {
        this.error = error;
        this.data = data;
    }
}

/// <summary>
/// Representa la respuesta de generación de código de descarga.
/// </summary>
public class GeneratedDownloadCode
{
    /// <summary>
    /// Mensaje de error asociado a la generación del código.
    /// </summary>
    public ErrorMessage error;
    /// <summary>
    /// Código de descarga generado.
    /// </summary>
    public string download_code;

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="GeneratedDownloadCode"/>.
    /// </summary>
    /// <param name="download_code">Código de descarga generado.</param>
    /// <param name="error">Mensaje de error asociado a la generación del código.</param>
    public GeneratedDownloadCode(string download_code, ErrorMessage error = null)
    {
        this.error = error;
        this.download_code = download_code;
    }
}