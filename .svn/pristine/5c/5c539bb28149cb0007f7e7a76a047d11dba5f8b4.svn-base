using System.IO;
using System.Threading.Tasks;
using SimpleJSON;
using UnityEngine;

/// <summary>
/// Utilidades para serializar y deserializar datos en formato JSON.
/// Facilita la conversión entre objetos y cadenas JSON.
/// </summary>
public static class JSONUtilities
{
    /// <summary>
    /// Convierte un objeto en una cadena JSON.
    /// </summary>
    /// <param name="obj">Objeto a serializar.</param>
    /// <returns>Cadena JSON resultante.</returns>
    public static string ToJSON(object obj)
    {
        return JsonUtility.ToJson(obj);
    }

    /// <summary>
    /// Convierte una cadena JSON en un objeto del tipo especificado.
    /// </summary>
    /// <typeparam name="T">Tipo de objeto a deserializar.</typeparam>
    /// <param name="json">Cadena JSON.</param>
    /// <returns>Objeto deserializado.</returns>
    public static T FromJSON<T>(string json)
    {
        return JsonUtility.FromJson<T>(json);
    }

    /// <summary>
    /// Reads the content of a JSON file and returns it
    /// </summary>
    /// <param name="filePath"> path of the JSON file</param>
    /// <returns>the encoded JSON if the reading was successfull, a "Status : failed" JSON otherwise</returns>
    public static JSONNode ReadJSONFromFile(string filePath)
    {
        StreamReader reader = null;
        try
        {
            reader = new StreamReader(filePath);
            return JSONNode.Parse(reader.ReadToEnd());
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }
        finally
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
            }
        }
        return JSONNode.Parse("{ \"Status\" : \"FAILED\" }");
    }

    /// <summary>
    /// Reads the content of a JSON file and returns it, asynchronously
    /// </summary>
    /// <param name="filePath"> path of the JSON file</param>
    /// <returns>the encoded JSON if the reading was successfull, a "Status : failed" JSON otherwise</returns>
    public static async Task<JSONNode> ReadJSONFromFileAsync(string filePath)
    {
        StreamReader reader = null;
        try
        {
            reader = new StreamReader(filePath);
            string fileContent = await reader.ReadToEndAsync().ConfigureAwait(false);
            return JSONNode.Parse(fileContent);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }
        finally
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
            }
        }
        return JSONNode.Parse("{ \"Status\" : \"FAILED\" }");
    }

    /// <summary>
    /// Writes the content of a JSON in a file
    /// </summary>
    /// <param name="filePath"> path of the JSON file</param>
    /// <param name="content">content of the JSON that you want to write</param>
    /// <param name="singleLine">Should the JSON be written without spaces?</param>
    /// <param name="append">Should the original file be completeley rewritten? (if exists)</param>
    public static void WriteJSONToFile(string filePath, JSONNode content, bool singleLine = false, bool append = false)
    {
        EnsureDirectoryExists(filePath);
        StreamWriter writer = null;
        try
        {
            writer = new StreamWriter(filePath, append);
            if (singleLine)
            {
                writer.WriteLine(content.ToString());
            }
            else
            {
                writer.WriteLine(content.ToString(""));
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }
        finally
        {
            if (writer != null)
            {
                writer.Close();
                writer.Dispose();
            }
        }
    }

    static void EnsureDirectoryExists(string filePath)
    {
        FileInfo fi = new FileInfo(filePath);
        if (!fi.Directory.Exists)
        {
            Directory.CreateDirectory(fi.DirectoryName);
        }
    }
}