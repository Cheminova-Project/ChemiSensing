using System;
using System.Text;
using UnityEngine;

public static class JwtParser
{
    [Serializable]
    public class JwtPayload
    {
        public string sub;
        public string role;
        public string name;
        public string jti;
        public int session_id;
        public long exp;
        public long iat;
    }

    public static JwtPayload ParsePayload(string jwt)
    {
        if (string.IsNullOrEmpty(jwt))
            throw new ArgumentException("JWT is empty.");

        string[] parts = jwt.Split('.');

        if (parts.Length < 2)
            throw new ArgumentException("Invalid JWT format.");

        string payload = parts[1];
        string json = DecodeBase64Url(payload);

        return JsonUtility.FromJson<JwtPayload>(json);
    }

    private static string DecodeBase64Url(string input)
    {
        string base64 = input
            .Replace('-', '+')
            .Replace('_', '/');

        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
            case 0:
                break;
            default:
                throw new FormatException("Invalid Base64URL string.");
        }

        byte[] bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(bytes);
    }

    public static bool IsExpired(JwtPayload payload)
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return payload.exp > 0 && payload.exp < now;
    }
}