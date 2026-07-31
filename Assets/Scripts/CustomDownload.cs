using System.Threading.Tasks;
using UnityEngine.Networking;
using GLTFast.Loading;
using UnityEngine;

/// <summary>
/// Implementación personalizada de IDownload para descargas de archivos usando UnityWebRequest.
/// Detecta automáticamente si el contenido es binario basándose en la extensión del archivo y el tipo MIME.
/// </summary>
public class CustomDownload : IDownload
{
    /// <summary>
    /// Datos binarios descargados
    /// </summary>
    public byte[] Data { get; private set; }
    
    /// <summary>
    /// Datos de texto descargados
    /// </summary>
    public string Text { get; private set; }
    
    /// <summary>
    /// Mensaje de error si la descarga falló
    /// </summary>
    public string Error { get; private set; }
    
    /// <summary>
    /// Indica si la descarga fue exitosa (sin errores)
    /// </summary>
    public bool Success => string.IsNullOrEmpty(Error);
    
    /// <summary>
    /// URL del recurso descargado
    /// </summary>
    public string Url { get; private set; }
    
    /// <summary>
    /// Indica si el contenido descargado es binario
    /// </summary>
    public bool? IsBinary { get; private set; }

    private UnityWebRequest request;

    /// <summary>
    /// Constructor que inicializa la descarga con un UnityWebRequest
    /// </summary>
    /// <param name="request">Request de Unity para realizar la descarga</param>
    public CustomDownload(UnityWebRequest request)
    {
        this.request = request;
        Url = request.url;

        // Decide automáticamente si es binario según la extensión del archivo
        IsBinary = request.url.EndsWith(".glb") || request.url.EndsWith(".bin") || request.url.EndsWith(".jpg") || request.url.EndsWith(".png");
    }

    /// <summary>
    /// Ejecuta la descarga de forma asíncrona
    /// </summary>
    /// <returns>Task que se completa cuando la descarga termina</returns>
    public async Task Run()
    {
        var operation = request.SendWebRequest();
        while (!operation.isDone)
        {
            await Task.Yield();
        }

        if (request.result == UnityWebRequest.Result.Success)
        {
            Data = request.downloadHandler.data;
            Text = request.downloadHandler.text;

            // Detectar tipo de archivo binario basándose en el Content-Type
            string contentType = request.GetResponseHeader("Content-Type");
            if (!string.IsNullOrEmpty(contentType))
            {
                IsBinary = contentType.Contains("application/octet-stream") || 
                        contentType.Contains("model/gltf-binary") ||
                        contentType.Contains("image/") ||
                        contentType.Contains("application/octet-stream");
            }
            else if (Data != null && Data.Length > 4)
            {
                // Fallback: si empieza con 'glTF' es binario GLB
                string magic = System.Text.Encoding.ASCII.GetString(Data, 0, 4);
                IsBinary = magic == "glTF";
            }
            else
            {
                IsBinary = false;
            }

        }
        else
        {
            Error = request.error;
            Debug.LogError($"[CustomDownload] Error downloading from {Url}: {Error}");
        }
    }

    /// <summary>
    /// Libera los recursos utilizados por el UnityWebRequest
    /// </summary>
    public void Dispose()
    {
        request?.Dispose();
    }
}
