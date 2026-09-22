using System.IO;
using System.Security.Cryptography;
using UnityEngine;
 
public class Checksum : MonoBehaviour
{
    public static string CalculateMD5(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("File not found at path: " + filePath);
            return null;
        }
 
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hash = md5.ComputeHash(stream);
                string md5String = System.BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                return md5String;
            }
        }
    }
}