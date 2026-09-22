using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public static class JsonGlobalSettings
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void SetupNewtonsoftDefaults()
    {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new UnityColorConverter(),
                new UnityVector3Converter()
            },
            
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented 
        };
    }
}