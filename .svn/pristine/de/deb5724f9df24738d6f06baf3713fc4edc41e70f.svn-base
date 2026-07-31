using System.Threading.Tasks;
using GLTFast.Loading;

/// <summary>
/// Implementación vacía de IDownload para casos donde no se necesita descargar nada.
/// Útil como placeholder o para testing.
/// </summary>
public class EmptyDownload : IDownload
{
    /// <summary>Siempre retorna null (sin datos)</summary>
    public byte[] Data => null;
    /// <summary>Siempre retorna null (sin texto)</summary>
    public string Text => null;
    /// <summary>Siempre retorna null (sin error)</summary>
    public string Error => null;
    /// <summary>Siempre retorna true (siempre exitoso)</summary>
    public bool Success => true;
    /// <summary>Retorna un identificador estático</summary>
    public string Url => "EmptyDownload";
    /// <summary>Siempre retorna false (no es binario)</summary>
    public bool? IsBinary => false;

    /// <summary>
    /// No hace nada, retorna una tarea completada inmediatamente.
    /// </summary>
    public Task Run() => Task.CompletedTask;
    /// <summary>
    /// No hace nada, no hay recursos que liberar.
    /// </summary>
    public void Dispose() {}
}
