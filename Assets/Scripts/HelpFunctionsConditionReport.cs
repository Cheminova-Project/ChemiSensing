using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

public class HelpFunctionsConditionReport : MonoBehaviour
{
    public static void GetDateParts(string dateString, out int year, out int month, out int day)
    {
        DateTime date = DateTime.Parse(dateString);

        year = date.Year;
        month = date.Month;
        day = date.Day;
    }
    
    public static StatusType GetStatusTypeFromString(string statusType)
    {
        switch (statusType.ToLower())
        {
            case "draft":
                return StatusType.draft;
            case "finished":
                return StatusType.finished;
            default:
                return StatusType._public;
        }
    }

    public static string StatusToString(StatusType? statusType)
    {
        switch (statusType)
        {
            case StatusType._public:
                return "public";
            default:
                return statusType.ToString().ToLower();
        }
    }

    public static StateOfConservationType GetStateOfConservationTypeFromString(string state)
    {
        if (state != null)
        {
            switch (state.ToLower())
            {
                case "good":
                    return StateOfConservationType.good;
                case "fair":
                    return StateOfConservationType.fair;
                case "poor":
                    return StateOfConservationType.poor;
                default:
                    return StateOfConservationType.very_bad;
            }
        }
        
        return StateOfConservationType.good;
    }

    public static string StateToString(StateOfConservationType? state)
    {
        switch(state)
        {
            case StateOfConservationType.very_bad:
                return "very bad";
            default:
                return state.ToString().ToLower();
        }
    }
    
    public static DetailLevel GetDetailLevelFromString(string state)
    {
        switch (state.ToLower())
        {
            case "cloud points":
                return DetailLevel.cloudpoints;
            case "high poly":
                return DetailLevel.highpoly;
            case "medium poly":
                return DetailLevel.mediumpoly;
            default:
                return DetailLevel.lowpoly;
        }
    }

    public static string AnnotationTypeToString(AnnotationType? annotationType)
    {
        if (annotationType.Equals(AnnotationType._2D))
            return "2D";
        else
            return "3D";
    }
    
    public static AnnotationType GetAnnotationTypeFromString(string type)
    {
        if (type.Equals("2D"))
            return AnnotationType._2D;
        else
            return AnnotationType._3D;
    }
    
    public static string AnnotationCategoryToStringDatabase(AnnotationCategory? annotationCategory)
    {
        return annotationCategory.ToString().ToLower();
    }
    
    public static string AnnotationCategoryDatabaseToString(string annotationCategory)
    {
        switch (annotationCategory)
        {
            case "info":
                return "Information";
            case "damage":
                return "Alteration";
            default:
                return "Multimedia data";
        }
    }
    
    public static AnnotationCategory GetAnnotationCategoryFromString(string category)
    {
        switch (category)
        {
            case "Information":
                return AnnotationCategory.info;
            case "Alteration":
                return AnnotationCategory.damage;
            default:
                return AnnotationCategory.data;
        }
    }
    
    public static string AnnotationVisualizationTypeToString(AnnotationVisualizationType? type)
    {
        switch (type)
        {
            case AnnotationVisualizationType.split: return "split";
            case AnnotationVisualizationType.horizontal_split: return "horizontal_split";
            case AnnotationVisualizationType.vertical_split: return "vertical_split";
            case AnnotationVisualizationType.spot: return "spot";
            case AnnotationVisualizationType.ring: return "ring";
            case AnnotationVisualizationType.section_plane: return "section_plane";
            default: return "default";
        }
    }
    
    public static AnnotationVisualizationType GetAnnotationVisualizationTypeFromString(string type)
    {
        switch (type.ToLower())
        {
            case "horizontal split":
                return AnnotationVisualizationType.horizontal_split;
            case "vertical split":
                return AnnotationVisualizationType.vertical_split;
            case "spot":
                return AnnotationVisualizationType.spot;
            default:
                return AnnotationVisualizationType.def;
        }
    }

    public static string DetailLevelToString(DetailLevel? level)
    {
        switch (level)
        {
            case DetailLevel.cloudpoints:
                return "cloud points";
            case DetailLevel.highpoly:
                return "high poly";
            case DetailLevel.mediumpoly:
                return "medium poly";
            default:
                return "low poly";
        }
    }

    public static string GetInspectionModeTypeFromToggle(string toggleText)
    {
        switch (toggleText.ToLower())
        {
            case "visual":
                return "Visual";
            case "visual_and_tactile":
                return "Visual and tactile";
            case "support":
                return "Support and/or backside inspection";
            case "instrumental":
                return "Instrumental measurements and analyses";
            case "other":
                return "Other";
            default:
                return "";
        }
    }
    
    public static string AddLeadingZero(int value)
    {
        if (value == 0)
            return "01";
        
        if (value >= 1 && value <= 9)
            return "0" + value;

        return value.ToString();
    }
    
    public static string FormatYearTo4Digits(int value)
    {
        // Asegura que el año esté dentro de un rango razonable
        value = Mathf.Clamp(value, 1, 9999);

        // Devuelve siempre 4 dígitos (0001, 0123, 2025…)
        return value.ToString("0000");
    }
    
    public static void ValidateDay(IntegerField dayField, IntegerField monthField, IntegerField yearField)
    {
        int day = dayField.value;
        int month = monthField.value;
        int year = yearField.value;

        month = Mathf.Clamp(month, 1, 12);
        if (year < 1)
            year = 1;

        int maxDay = DateTime.DaysInMonth(year, month);

        day = Mathf.Clamp(day, 1, maxDay);

        if (day != dayField.value)
            dayField.SetValueWithoutNotify(day);
    }
    
    public static void ValidateMonth(IntegerField field)
    {
        int month = field.value;
        month = Mathf.Clamp(month, 1, 12);

        if (month != field.value)
            field.SetValueWithoutNotify(month);
    }
    
    public static void ValidateYear(IntegerField field)
    {
        int year = field.value;

        // Limitar al rango válido de DateTime
        year = Mathf.Clamp(year, 1, 9999);

        if (year != field.value)
            field.SetValueWithoutNotify(year);
    }
    
    public static string Matrix4X4ToString(Matrix4x4 matrix)
    {
        return matrix.ToString("G9", CultureInfo.InvariantCulture);
    }

    public static Matrix4x4 StringToMatrix4X4(string matrixString)
    {
        var matrixStrings = matrixString.Split('\t','\n');
        float[] matrixFloats = new float[16];
        int index = 0;
        foreach (var subs in matrixStrings[0..16])
        {
            matrixFloats[index++] = float.Parse(subs, CultureInfo.InvariantCulture);
        }
        Matrix4x4 m = new Matrix4x4();
        index = 0;
        for (int i = 0; i < 16; i++)
        {
            m[index++] = matrixFloats[i];
        }

        return m.transpose;
    }
    
    public static void SetTransformFromMatrix(Transform t, Matrix4x4 m)
    {
        t.position = m.GetColumn(3);

        // Extract rotation
        Vector3 forward = m.GetColumn(2);
        Vector3 up = m.GetColumn(1);
        t.rotation = Quaternion.LookRotation(forward, up);

        // Extract scale
        Vector3 scale = new Vector3(
            m.GetColumn(0).magnitude,
            m.GetColumn(1).magnitude,
            m.GetColumn(2).magnitude
        );
        t.localScale = scale;
    }

    public static int GetNumberOrDefault(string input)
    {
        var match = Regex.Match(input, @"\d+");
        if (match.Success)
        {
            return int.Parse(match.Value);
        }

        return 50;
    }
}