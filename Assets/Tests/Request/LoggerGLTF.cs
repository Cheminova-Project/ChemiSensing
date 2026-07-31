using GLTFast.Logging;
using UnityEngine;
public class LoggerGLTF : ICodeLogger
{
    public void Error(LogCode code, params string[] messages)
    {
        Debug.LogError($"GLTFast Error: {code} - {string.Join(", ", messages)}");
    }

    public void Warning(LogCode code, params string[] messages)
    {
        Debug.LogWarning($"GLTFast Warning: {code} - {string.Join(", ", messages)}");
    }

    public void Info(LogCode code, params string[] messages)
    {
        Debug.Log($"GLTFast Info: {code} - {string.Join(", ", messages)}");
    }

    public void Error(string message)
    {
        Debug.LogError($"GLTFast Error: {message}");
    }

    public void Warning(string message)
    {
        Debug.LogWarning($"GLTFast Warning: {message}");
    }

    public void Info(string message)
    {
        Debug.Log($"GLTFast Info: {message}");
    }
}