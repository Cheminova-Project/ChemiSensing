using System.Threading.Tasks;
using UnityEngine.Networking;
using GLTFast.Loading;
using UnityEngine;

/// <summary>
/// Implementación personalizada de ITextureDownload para descargas de texturas usando UnityWebRequest.
/// Descarga y convierte automáticamente los datos en un Texture2D.
/// </summary>
public class CustomTextureDownload : ITextureDownload
{
    /// <summary>
    /// Datos binarios de la textura descargada
    /// </summary>
    public byte[] Data { get; private set; }
    
    /// <summary>
    /// No aplicable para texturas (siempre null)
    /// </summary>
    public string Text => null;
    
    /// <summary>
    /// Mensaje de error si la descarga falló
    /// </summary>
    public string Error { get; private set; }
    
    /// <summary>
    /// Indica si la descarga fue exitosa (sin errores)
    /// </summary>
    public bool Success => string.IsNullOrEmpty(Error);
    
    /// <summary>
    /// URL de la textura descargada
    /// </summary>
    public string Url { get; private set; }
    
    /// <summary>
    /// Siempre true para texturas (son binarias por definición)
    /// </summary>
    public bool? IsBinary => true;
    
    /// <summary>
    /// Textura 2D descargada y lista para usar
    /// </summary>
    public Texture2D Texture { get; private set; }

    private UnityWebRequest request;

    /// <summary>
    /// Constructor que inicializa la descarga de textura con un UnityWebRequest
    /// </summary>
    /// <param name="request">Request de Unity configurado para descargar texturas</param>
    public CustomTextureDownload(UnityWebRequest request)
    {
        this.request = request;
        Url = request.url;
    }

    /// <summary>
    /// Ejecuta la descarga de la textura de forma asíncrona
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
            Texture = DownloadHandlerTexture.GetContent(request);
            Data = request.downloadHandler.data;
        }
        else
        {
            Error = request.error;
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
