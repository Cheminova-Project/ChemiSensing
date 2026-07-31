using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

public class HelpFunctions : MonoBehaviour
{
    // Generalized method to get EnumMember labels for any enum type
    public static List<string> GetEnumMemberLabels<TEnum>() where TEnum : Enum
    {
        List<string> labels = new List<string>();
        var enumValues = Enum.GetValues(typeof(TEnum));

        foreach (var value in enumValues)
        {
            // Get the enum field by its value
            FieldInfo field = typeof(TEnum).GetField(value.ToString());

            // Get the EnumMember attribute (if present)
            EnumMemberAttribute enumMember = field.GetCustomAttribute<EnumMemberAttribute>();
            if (enumMember != null)
            {
                // Add the custom label to the list
                labels.Add(enumMember.Value);
            }
            else
            {
                // If no EnumMember attribute, fall back to the default enum name
                labels.Add(value.ToString());
            }
        }
        return labels;
    }
    
    // Get audio format from an extension in a string variable
    public static AudioType GetAudioTypeFromExtension(string fileName)
    {
        string ext = Path.GetExtension(fileName).ToLower();

        return ext switch
        {
            ".wav" => AudioType.WAV,
            ".mp3" => AudioType.MPEG,
            ".ogg" => AudioType.OGGVORBIS,
            ".aiff" => AudioType.AIFF,
            ".aif" => AudioType.AIFF,
            ".aac" => AudioType.ACC,
            ".mod" => AudioType.MOD,
            ".xm" => AudioType.XM,
            ".s3m" => AudioType.S3M,
            _ => AudioType.UNKNOWN
        };
    }
    
    // Converts date time "YYYY-MM-DDTHH:MM:SS" to format "MMMM, yyyy"
    public static string FormatDate(string isoDateTime)
    {
        if (DateTime.TryParse(isoDateTime, out DateTime date))
        {
            return date.ToString("MMMM, yyyy", new CultureInfo("en-EN")); // e.g., "May, 2025"
        }
        else
        {
            Debug.LogWarning($"Invalid date format: {isoDateTime}");
            return isoDateTime; // Returns the original if parsing failed
        }
    }
    
    // Converts date time "YYYY-MM-DDTHH:MM:SS" to format "DD-MM-YYYY"
    public static string FormatDateTo_DD_MM_YYYY(string isoDateTime)
    {
        if (DateTime.TryParse(isoDateTime, out DateTime date))
        {
            return date.ToString("dd-MM-yyyy"); // e.g., "20-05-2025"
        }
        else
        {
            Debug.LogWarning($"Invalid date format: {isoDateTime}");
            return isoDateTime; // Returns the original if parsing failed
        }
    }
    
    private static readonly Dictionary<string, List<string>> MimeTypesByCategory = new Dictionary<string, List<string>>()
    {
        {
            "image", new List<string>
            {
                "image/png",
                "image/jpeg",
                "image/bmp",
                "image/aces"
            }
        },
        {
            "audio", new List<string>
            {
                "audio/mpeg",
                "audio/wav",
                "audio/ogg",
                "audio/aiff"
            }
        },
        {
            "video", new List<string>
            {
                "video/mp4",
                "video/webm"
            }
        }
    };

    public static List<string> BuildMediaQuery(string type)
    {
        List<string> queryList = new List<string>();
        
        if (!MimeTypesByCategory.ContainsKey(type.ToLower()))
            return queryList;

        foreach (var mime in MimeTypesByCategory[type.ToLower()])
            queryList.Add(mime);

        return queryList;
    }
    
    public static Vector3[] CopyLineRendererTo(GameObject targetObject, bool newLine, LineRenderer lineRenderer = null, Vector3[] points = null)
    {
        // Create or get LineRenderer on the target
        LineRenderer targetLineRenderer = targetObject.GetComponent<LineRenderer>();
        if (targetLineRenderer == null)
        {
            targetLineRenderer = targetObject.AddComponent<LineRenderer>();
            // Copy basic settings
            if (newLine && lineRenderer != null)
            {
                targetLineRenderer.material = lineRenderer.material;
                targetLineRenderer.startColor = lineRenderer.startColor;
                targetLineRenderer.endColor = lineRenderer.endColor;
                targetLineRenderer.startWidth = 0.08f;//lineRenderer.startWidth;
                targetLineRenderer.endWidth = 0.08f;//lineRenderer.endWidth;
                targetLineRenderer.widthMultiplier = 0.08f;//lineRenderer.widthMultiplier;
                targetLineRenderer.loop = lineRenderer.loop;
                targetLineRenderer.useWorldSpace = false; // Important for following transform
            }
        }

        if (newLine && lineRenderer != null)
        {
            Vector3[] worldPoints = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(worldPoints);
            
            Vector3[] localPositions = new Vector3[worldPoints.Length];
            for (int i = 0; i < worldPoints.Length; i++)
            {
                localPositions[i] = targetObject.transform.InverseTransformPoint(worldPoints[i]);
            }
            
            // Assign positions
            targetLineRenderer.positionCount = localPositions.Length;
            targetLineRenderer.SetPositions(localPositions);
            
            return localPositions;
        }
        
        if (points != null && points.Length > 0)
        {
            Vector3[] worldPoints = points;
            Vector3[] localPositions = new Vector3[worldPoints.Length];
            for (int i = 0; i < worldPoints.Length; i++)
            {
                localPositions[i] = worldPoints[i];
            }
            
            // Assign positions
            targetLineRenderer.positionCount = localPositions.Length;
            targetLineRenderer.SetPositions(localPositions);

            return localPositions;
        }

        return null;
    }
}