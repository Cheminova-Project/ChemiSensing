/* The `AnnexDataDB` class in C# provides methods for interacting with an API to manage annex data,
including creating, updating, deleting, and retrieving annex data along with associated files and
multimedia content. */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Paroxe.PdfRenderer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Video;

/// <summary>
/// Base de datos para gestionar datos anexos en la aplicación.
/// Permite almacenar y recuperar información adicional asociada a objetos.
/// </summary>
public static class AnnexDataDB
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
    /// Crea un nuevo dato anexo en la base de datos.
    /// </summary>
    /// <param name="onCompleted">Callback que se llama al completar la operación.</param>
    /// <param name="category">Categoría del dato anexo.</param>
    /// <param name="content">Contenido del dato anexo.</param>
    /// <param name="title">Título del dato anexo.</param>
    /// <param name="icon">Ícono asociado al dato anexo.</param>
    /// <param name="idAnnexDocs">Lista de IDs de documentos anexos.</param>
    /// <param name="idChElement">ID del elemento químico asociado.</param>
    /// <param name="ide3dModel">ID del modelo 3D asociado.</param>
    /// <param name="idSite">ID del sitio asociado.</param>
    /// <param name="timestamp">Marca de tiempo opcional.</param>
    public static IEnumerator PostNewAnnexData(UnityAction<AnnexDataData, bool> onCompleted,
        AnnexDataCategory category, string content, string title, int? icon = null,
         List<int> idAnnexDocs = null, int? idChElement = null, int? ide3dModel = null,
        int? idSite = null,  string timestamp = null)
    {
        AnnexDataPost annexDataPostData = new AnnexDataPost()
        {
            category = category.ToString(),
            content = content,
            title = title,
            icon = icon,
            id_annex_docs = idAnnexDocs,
            id_chelement = idChElement,
            id_e3dmodel = ide3dModel,
            id_site = idSite,
            timestamp = timestamp
        };

        string jsonData = JsonConvert.SerializeObject(annexDataPostData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        UnityWebRequest request = new UnityWebRequest(ApiUrl, "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Successful creation of AnnexData: " + request.downloadHandler.text);
            AnnexDataData annexDataData = JsonConvert.DeserializeObject<AnnexDataData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(annexDataData.error))
            {
               //Debug.Log("AnnexData added " + annexDataData.content);
                onCompleted?.Invoke(annexDataData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + annexDataData.error);
                onCompleted?.Invoke(annexDataData, false);
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
    /// Elimina un dato anexo de la base de datos.
    /// </summary>
    /// <param name="onCompleted">Callback que se llama al completar la operación.</param>
    /// <param name="id">ID del dato anexo a eliminar.</param>
    public static IEnumerator DeleteAnnexDataFromApi(UnityAction<ErrorMessage, bool> onCompleted, int id)
    {
        UnityWebRequest request = UnityWebRequest.Delete(ApiUrl + "/" + id);
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Successful elimination of AnnexData: " + request.downloadHandler.text);
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
    /// Obtiene un dato anexo por su ID.
    /// </summary>
    /// <param name="onCompleted">Callback que se llama al completar la operación.</param>
    /// <param name="id">ID del dato anexo a obtener.</param>
    public static IEnumerator GetAnnexDataByID(UnityAction<AnnexDataData, bool> onCompleted, int id)
    {
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log("AnnexData obtained successfully: " + request.downloadHandler.text);
            AnnexDataData annexDataData = JsonConvert.DeserializeObject<AnnexDataData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(annexDataData.error))
            {
                //Debug.Log("AnnexData: " + annexDataData.content);
                onCompleted?.Invoke(annexDataData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + annexDataData.error);
                onCompleted?.Invoke(annexDataData, false);
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
    /// Actualiza un dato anexo en la base de datos.
    /// </summary>
    /// <param name="onCompleted">Callback que se llama al completar la operación.</param>
    /// <param name="id">ID del dato anexo a actualizar.</param>
    /// <param name="category">Categoría del dato anexo.</param>
    /// <param name="content">Contenido del dato anexo.</param>
    /// <param name="icon">Ícono asociado al dato anexo.</param>
    /// <param name="idChElement">ID del elemento químico asociado.</param>
    /// <param name="ide3dModel">ID del modelo 3D asociado.</param>
    /// <param name="idSite">ID del sitio asociado.</param>
    /// <param name="title">Título del dato anexo.</param>
    public static IEnumerator PutAnnexDataFromApi(UnityAction<AnnexDataData, bool> onCompleted, int id,
        AnnexDataCategory? category = null,  string content = null, int? icon = null,
        int? idChElement = null, int? ide3dModel = null, int? idSite = null,  string title = null)
    {
        AnnexDataPost annexDataPutData = new AnnexDataPost()
        {
            category = category.ToString(),
            content = content,
            icon = icon,
            id_chelement = idChElement,
            id_e3dmodel = ide3dModel,
            id_site = idSite,
            title = title
        };
        
        string jsonData = JsonConvert.SerializeObject(annexDataPutData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id, "PUT");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("AnnexData changed successfully: " + request.downloadHandler.text);
            AnnexDataData annexDataData = JsonConvert.DeserializeObject<AnnexDataData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(annexDataData.error))
            {
               //Debug.Log("AnnexData changed: " + annexDataData.content);
                onCompleted?.Invoke(annexDataData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + annexDataData.error);
                onCompleted?.Invoke(annexDataData, false);
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
    /// Obtiene la lista de archivos asociados a un dato anexo.
    /// </summary>
    /// <param name="onCompleted">Callback que se llama al completar la operación.</param>
    /// <param name="id">ID del dato anexo.</param>
    /// <param name="page">Número de página para paginación.</param>
    /// <param name="perPage">Número de elementos por página.</param>
    /// <param name="search">Término de búsqueda opcional.</param>
    /// <param name="sort">Campo por el cual ordenar los resultados.</param>
    /// <param name="order">Orden de clasificación (ascendente/descendente).</param>
    public static IEnumerator GetAnnexDataFileListFromAnnexData(UnityAction<AnnexDataFileResponse, bool> onCompleted,
        int id, int? page = null, int? perPage = null,  string search = null,
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
        
        string query = ApiUrl + "/" + id + "/files" + QueryManagement.ToQueryString(queryParams);
        //Debug.Log("API URL: " + query);
        UnityWebRequest request = new UnityWebRequest(query, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("AnnexDataFiles obtained successfully: " + request.downloadHandler.text);
            AnnexDataFileResponse annexDataFileResponse = JsonConvert.DeserializeObject<AnnexDataFileResponse>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(annexDataFileResponse.error))
            {
               //Debug.Log("List of AnnexDataFile saved with the size of " + annexDataFileResponse.items.Count);
                onCompleted?.Invoke(annexDataFileResponse, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + annexDataFileResponse.error);
                onCompleted?.Invoke(annexDataFileResponse, false);
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
    /// Crea un nuevo archivo anexo asociado a un dato anexo.
    /// </summary>
    /// <param name="onCompleted">Callback que se llama al completar la operación.</param>
    /// <param name="id">ID del dato anexo.</param>
    /// <param name="idFile">ID del archivo a asociar.</param>
    public static IEnumerator PostNewAnnexDataFileToAnnexData(UnityAction<AnnexDataFileData, bool> onCompleted,
        int id, int idFile)
    {
        AnnexDataFilePost annexDataFilePostData = new AnnexDataFilePost()
        {
            id_file = idFile,
        };

        string jsonData = JsonConvert.SerializeObject(annexDataFilePostData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id + "/files", "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Successful creation of AnnexDataFile: " + request.downloadHandler.text);
            AnnexDataFileData annexDataFileData = JsonConvert.DeserializeObject<AnnexDataFileData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(annexDataFileData.error))
            {
               //Debug.Log("AnnexDataFile added " + annexDataFileData.id);
                onCompleted?.Invoke(annexDataFileData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + annexDataFileData.error);
                onCompleted?.Invoke(annexDataFileData, false);
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
    /// Descarga y obtiene la imagen asociada a un dato anexo.
    /// </summary>
    /// <param name="id">ID del dato anexo.</param>
    /// <param name="onComplete">Callback que se llama al completar la descarga.</param>
    public static IEnumerator GetAnnexDataImage(int? id, UnityAction<Texture2D, bool> onComplete)
    {
        yield return DownloadDB.GetDownloadByID(id, (rawDownload, success) =>
        {
            if (success && rawDownload.data != null && rawDownload.data.Length > 0)
            {
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                bool loaded = tex.LoadImage(rawDownload.data); // Esto convierte los bytes en textura

                if (loaded)
                {
                    onComplete?.Invoke(tex, true);
                }
                else
                {
                    Debug.LogWarning("No se pudo crear la textura desde los datos descargados.");
                    onComplete?.Invoke(null, false);
                }
            }
            else
            {
                Debug.LogWarning("Error en la descarga o datos vacíos.");
                onComplete?.Invoke(null, false);
            }
        });
    }
    
    /// <summary>
    /// Descarga y obtiene el audio asociado a un dato anexo.
    /// </summary>
    /// <param name="id">ID del dato anexo.</param>
    /// <param name="fileName">Nombre del archivo de audio.</param>
    /// <param name="onComplete">Callback que se llama al completar la descarga.</param>
    public static IEnumerator GetAnnexDataAudio(int? id, string fileName, UnityAction<AudioClip, bool> onComplete)
    {
        yield return DownloadDB.GetDownloadByID(id, (rawDownload, success) =>
        {
            if (success && rawDownload.data != null && rawDownload.data.Length > 0)
            {
                // Get audio type from file extension
                AudioType audioType = HelpFunctions.GetAudioTypeFromExtension(fileName);
                if (audioType == AudioType.UNKNOWN)
                {
                    Debug.LogWarning("Unsupported audio format.");
                    onComplete?.Invoke(null, false);
                    return;
                }

                // Save bytes temporarily
                string tempPath = Path.Combine(Application.temporaryCachePath, fileName);
                File.WriteAllBytes(tempPath, rawDownload.data);

                UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + tempPath, audioType);
                var request = www.SendWebRequest();

                request.completed += _ =>
                {
                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                        onComplete?.Invoke(clip, true);
                    }
                    else
                    {
                        Debug.LogWarning("Audio load error: " + www.error);
                        onComplete?.Invoke(null, false);
                    }

                    www.Dispose();
                };
            }
            else
            {
                Debug.LogWarning("Download failed or data empty.");
                onComplete?.Invoke(null, false);
            }
        });
    }
    
    /// <summary>
    /// Descarga y obtiene el PDF asociado a un dato anexo.
    /// </summary>
    /// <param name="id">ID del dato anexo.</param>
    /// <param name="onComplete">Callback que se llama al completar la descarga.</param>
    public static IEnumerator GetAnnexDataPdf(int? id, UnityAction<PDFDocument, bool> onComplete)
    {
        yield return DownloadDB.GetDownloadByID(id, (rawDownload, success) =>
        {
            if (success && rawDownload.data != null && rawDownload.data.Length > 0)
            {
                byte[] pdfData = rawDownload.data;
                
                if (pdfData != null)
                {
                    PDFDocument document = new PDFDocument(pdfData);
                    onComplete?.Invoke(document, true);
                }
                else
                {
                    Debug.LogWarning("No se pudo crear el PDFDocument desde los datos descargados.");
                    onComplete?.Invoke(null, false);
                }
            }
            else
            {
                Debug.LogWarning("Error en la descarga o datos vacíos.");
                onComplete?.Invoke(null, false);
            }
        });
    }
    
    /// <summary>
    /// Descarga y obtiene el video asociado a un dato anexo.
    /// </summary>
    /// <param name="id">ID del dato anexo.</param>
    /// <param name="targetVideoPlayer">VideoPlayer objetivo donde se reproducirá el video.</param>
    /// <param name="onComplete">Callback que se llama al completar la descarga.</param>
    public static IEnumerator GetAnnexDataVideo(int? id, VideoPlayer targetVideoPlayer, UnityAction<VideoPlayer, bool> onComplete)
    {
        yield return DownloadDB.GetDownloadByID(id, (rawDownload, success) =>
        {
            if (success && rawDownload.data != null && rawDownload.data.Length > 0)
            {
                try
                {
                    string filename = $"/video_{id}.mp4";
                    string path = Application.persistentDataPath + filename;
                    File.WriteAllBytes(path, rawDownload.data);

                    targetVideoPlayer.source = VideoSource.Url;
                    targetVideoPlayer.url = path;
                    targetVideoPlayer.playOnAwake = false;

                    onComplete?.Invoke(targetVideoPlayer, true);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error saving video {id}: {e.Message}");
                    onComplete?.Invoke(null, false);
                }
            }
            else
            {
                Debug.LogWarning("Video download failed or data is empty.");
                onComplete?.Invoke(null, false);
            }
        });
    }
}