using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class UnityColorConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Color) || objectType == typeof(Color?);
    }
    
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        Color color = (Color)value;
        writer.WriteStartObject();
        writer.WritePropertyName("r"); writer.WriteValue(color.r);
        writer.WritePropertyName("g"); writer.WriteValue(color.g);
        writer.WritePropertyName("b"); writer.WriteValue(color.b);
        writer.WritePropertyName("a"); writer.WriteValue(color.a);
        writer.WriteEndObject();
    }
    
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null) return null;

        JObject jo = JObject.Load(reader);
        Color color = new Color();

        if (jo["r"] != null) color.r = (float)jo["r"];
        if (jo["g"] != null) color.g = (float)jo["g"];
        if (jo["b"] != null) color.b = (float)jo["b"];
        if (jo["a"] != null) color.a = (float)jo["a"]; else color.a = 1f;

        return color;
    }
}

public class UnityVector3Converter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Vector3) || objectType == typeof(Vector3?);
    }
    
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        Vector3 vector = (Vector3)value;
        writer.WriteStartObject();
        writer.WritePropertyName("x"); writer.WriteValue(vector.x);
        writer.WritePropertyName("y"); writer.WriteValue(vector.y);
        writer.WritePropertyName("z"); writer.WriteValue(vector.z);
        writer.WriteEndObject();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        JObject jo = JObject.Load(reader);
        Vector3 vector = new Vector3();

        if (jo["x"] != null)
            vector.x = (float)jo["x"];
        
        if (jo["y"] != null)
            vector.y = (float)jo["y"];
        
        if (jo["z"] != null)
            vector.z = (float)jo["z"];

        return vector;
    }
}