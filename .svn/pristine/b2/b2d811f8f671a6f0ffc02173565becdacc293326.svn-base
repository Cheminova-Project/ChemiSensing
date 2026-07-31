using System.Threading.Tasks;
using GLTFast.Loading;
using UnityEngine;
using System;

/// <summary>
/// Implementación de ITextureDownload que crea una textura vacía de color sólido.
/// Útil para texturas placeholder o por defecto.
/// </summary>
public class EmptyTextureDownload : ITextureDownload, IDisposable
{
    /// <summary>
    /// Textura 2D generada
    /// </summary>
    public Texture2D Texture { get; private set; }
    
    /// <summary>
    /// Siempre exitoso
    /// </summary>
    public bool Success => true;
    
    /// <summary>
    /// Sin errores
    /// </summary>
    public string Error => null;

    /// <summary>
    /// Siempre es binario
    /// </summary>
    public bool? IsBinary => true;
    
    /// <summary>
    /// Datos de la textura (actualmente null)
    /// </summary>
    public byte[] Data { get; private set; }
    
    /// <summary>
    /// No aplicable para texturas
    /// </summary>
    public string Text => null;

    /// <summary>
    /// Indica si la textura se creó como no-leíble
    /// </summary>
    public bool NonReadable { get; private set; }

    /// <summary>
    /// Crea una textura vacía de color sólido
    /// </summary>
    /// <param name="width">Ancho de la textura en píxeles</param>
    /// <param name="height">Alto de la textura en píxeles</param>
    /// <param name="color">Color de la textura (transparente por defecto)</param>
    /// <param name="nonReadable">Si true, la textura será no-leíble desde CPU</param>
    public EmptyTextureDownload(int width = 1, int height = 1, Color? color = null, bool nonReadable = true)
    {
        NonReadable = nonReadable;
        var col = color ?? Color.clear;
        Texture = new Texture2D(width, height, TextureFormat.RGBA32, mipChain: false);
        var pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = col;
        Texture.SetPixels(pixels);
        Texture.Apply(updateMipmaps: false, makeNoLongerReadable: nonReadable);
        Texture.name = "EmptyTextureDownload";
    }

    /// <summary>
    /// No hace nada, retorna una tarea completada inmediatamente
    /// </summary>
    public Task Run() => Task.CompletedTask;

    /// <summary>
    /// Destruye la textura y libera recursos
    /// </summary>
    public void Dispose()
    {
        if (Texture != null)
        {
            UnityEngine.Object.Destroy(Texture);
            Texture = null;
        }
    }
}