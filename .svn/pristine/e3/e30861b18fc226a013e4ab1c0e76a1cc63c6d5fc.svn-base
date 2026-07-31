using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// Proporciona acceso a la API de modelos 3D (E3D Models).
/// Gestiona la configuración y la URL base de la API para modelos 3D.
/// </summary>
public static class E3DModelDB
{
    private static APIEndpointConfig config;
    public static string ApiEndpointUrl => Config != null ? Config.endpointURL : "";
    private static APIEndpointConfig Config
    {
        get
        {
            string className = MethodBase.GetCurrentMethod().DeclaringType.Name;
            if(className == null)
            {
                Debug.LogWarning("Couldn't obtained class name.");
                return null;
            }
            else
            {
                //Debug.Log("Name of the class: " + className);
            }
     
            config = Resources.Load<APIEndpointConfig>("API_URLS_ENDPOINT/" + className);
            if (config == null)
            {
                Debug.LogWarning("No APIBaseConfig found in Resources. Create an asset with that name in Resources.");
            }
            return config;
        }
    }
    public static string ApiUrl => GlobalManagement.Instance.urlBase + ApiEndpointUrl;
    
    /// <summary>
    /// Elimina un modelo 3D de la API.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando se completa la eliminación.</param>
    /// <param name="id">ID del modelo 3D a eliminar.</param>
    public static IEnumerator DeleteE3DModelFromApi(UnityAction<ErrorMessage, bool> onCompleted, int id)
    {
        UnityWebRequest request = UnityWebRequest.Delete(ApiUrl + "/" + id);
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Successful elimination of E3DModel: " + request.downloadHandler.text);
            ErrorMessage errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(errorMessage.error))
            {
               //Debug.Log("Element deleted");
                onCompleted?.Invoke(null, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + errorMessage.error);
                onCompleted?.Invoke(errorMessage, false);
            }
        }
        else
        {
            Debug.LogWarning("Error: " + request.error);
            Debug.LogWarning("Server reply: " + request.downloadHandler.text);
            onCompleted?.Invoke(null, false);
        }
        
        request.Dispose();
    }
    
    /// <summary>
    /// Obtiene un modelo 3D por su ID.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando se completa la obtención del modelo.</param>
    /// <param name="id">ID del modelo 3D a obtener.</param>
    public static IEnumerator GetE3DModelByID(UnityAction<E3DModelData, bool> onCompleted, int id)
    {
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log("E3DModel obtained successfully: " + request.downloadHandler.text);
            E3DModelData e3DModelData = JsonConvert.DeserializeObject<E3DModelData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(e3DModelData.error))
            {
                //Debug.Log("E3DModel: " + e3DModelData.name);
                onCompleted?.Invoke(e3DModelData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + e3DModelData.error);
                onCompleted?.Invoke(e3DModelData, false);
            }
        }
        else
        {
            Debug.LogWarning("Error: " + request.error);
            Debug.LogWarning("Server reply: " + request.downloadHandler.text);
            onCompleted?.Invoke(null, false);
        }
        
        request.Dispose();
    }
    
    /// <summary>
    /// Actualiza un modelo 3D en la API.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando se completa la actualización del modelo.</param>
    /// <param name="id">ID del modelo 3D a actualizar.</param>
    /// <param name="description">Descripción del modelo.</param>
    /// <param name="height">Altura del modelo.</param>
    /// <param name="icon">Ícono del modelo.</param>
    /// <param name="idChelement">ID del elemento químico asociado.</param>
    /// <param name="length">Longitud del modelo.</param>
    /// <param name="name">Nombre del modelo.</param>
    /// <param name="referenceDate">Fecha de referencia del modelo.</param>
    /// <param name="scale">Escala del modelo.</param>
    /// <param name="width">Anchura del modelo.</param>
    public static IEnumerator PutE3DModelFromApi(UnityAction<E3DModelData, bool> onCompleted, int id,
         string description = null, float? height = null, int? icon = null,
        int? idChelement = null, float? length = null,  string name = null,
         string referenceDate = null, float? scale = null, float? width = null)
    {
        E3DModelPost alterationFormPutData = new E3DModelPost()
        {
            description = description,
            height = height,
            icon = icon,
            id_chelement = idChelement,
            length = length,
            name = name,
            reference_date = referenceDate,
            scale = scale,
            width = width
        };
        
        string jsonData = JsonConvert.SerializeObject(alterationFormPutData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id, "PUT");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("E3DModel changed successfully: " + request.downloadHandler.text);
            E3DModelData e3DModelData = JsonConvert.DeserializeObject<E3DModelData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(e3DModelData.error))
            {
               //Debug.Log("E3DModel changed: " + e3DModelData.name);
                onCompleted?.Invoke(e3DModelData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + e3DModelData.error);
                onCompleted?.Invoke(e3DModelData, false);
            }
        }
        else
        {
            Debug.LogWarning("Error: " + request.error);
            Debug.LogWarning("Server reply: " + request.downloadHandler.text);
            onCompleted?.Invoke(null, false);
        }
        
        request.Dispose();
    }
    
    /// <summary>
    /// Obtiene una lista de modelos 3D por sus IDs.
    /// </summary>
    /// <param name="ids">Lista de IDs de los modelos 3D a obtener.</param>
    /// <param name="idChElement">ID del elemento químico asociado.</param>
    /// <param name="onCompleted">Acción a ejecutar cuando se completa la obtención de la lista.</param>
    public static IEnumerator GetE3DModelByIDList(List<int> ids, int idChElement, UnityAction<List<E3DModelData>, bool> onCompleted)
    {
        List<E3DModelData> results = new List<E3DModelData>();
        bool hasError = false;
        int completedCount = 0;

        foreach (int id in ids)
        {
            yield return GetE3DModelByID((data, success) =>
            {
                completedCount++;
                if (success && data != null)
                {
                    if (data.id_chelement == idChElement)
                        results.Add(data);
                }
                else
                {
                    hasError = true;
                }
            }, id);
        }

        onCompleted?.Invoke(results, !hasError);
    }
    
    /// <summary>
    /// Obtiene la lista de datos anexos de un modelo 3D.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando se completa la obtención de los datos anexos.</param>
    /// <param name="id">ID del modelo 3D.</param>
    /// <param name="page">Número de página para la paginación.</param>
    /// <param name="perPage">Número de elementos por página.</param>
    /// <param name="search">Cadena de búsqueda para filtrar resultados.</param>
    /// <param name="sort">Campo por el cual ordenar los resultados.</param>
    /// <param name="order">Orden de clasificación (ascendente/descendente).</param>
    /// <param name="mimeType">Lista de tipos MIME para filtrar los resultados.</param>
    public static IEnumerator GetAnnexDataListFromE3DModel(UnityAction<AnnexDataResponse, bool> onCompleted,
        int id, int? page = null, int? perPage = null,  string search = null,
         string sort = null,  string order = null,
         List<string> mimeType = null)
    {
        QueryParams queryParams = new QueryParams()
        {
            page = page,
            per_page = perPage,
            search = search,
            sort = sort,
            order = order,
            mime_type = mimeType
        };
        
        string query = ApiUrl + "/" + id + "/annexdata" + QueryManagement.ToQueryString(queryParams);
        //Debug.Log("API URL: " + query);
        UnityWebRequest request = new UnityWebRequest(query, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log("AnnexDatas obtained successfully: " + request.downloadHandler.text);
            AnnexDataResponse annexDataResponse = JsonConvert.DeserializeObject<AnnexDataResponse>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(annexDataResponse.error))
            {
                //Debug.Log("List of AnnexData saved with the size of " + annexDataResponse.items.Count);
                onCompleted?.Invoke(annexDataResponse, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + annexDataResponse.error);
                onCompleted?.Invoke(annexDataResponse, false);
            }
        }
        else
        {
            Debug.LogWarning("Error: " + request.error);
            Debug.LogWarning("Server reply: " + request.downloadHandler.text);
            onCompleted?.Invoke(null, false);
        }
        
        request.Dispose();
    }
    
    /// <summary>
    /// Obtiene la lista de instancias 3D de un modelo 3D.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando se completa la obtención de las instancias 3D.</param>
    /// <param name="id">ID del modelo 3D.</param>
    /// <param name="page">Número de página para la paginación.</param>
    /// <param name="perPage">Número de elementos por página.</param>
    /// <param name="search">Cadena de búsqueda para filtrar resultados.</param>
    /// <param name="sort">Campo por el cual ordenar los resultados.</param>
    /// <param name="order">Orden de clasificación (ascendente/descendente).</param>
    public static IEnumerator GetThreeDInstanceListFromE3DModel(UnityAction<ThreeDInstanceResponse, bool> onCompleted,
        int id, int? page = null, int? perPage = null,  string search = null,
         string sort = null,  string order = null)
    {
        QueryParams queryParams = new QueryParams()
        {
            page = page,
            per_page = perPage,
            search = search,
            sort = sort,
            order = order
        };
        
        string query = ApiUrl + "/" + id + "/e3dinstances" + QueryManagement.ToQueryString(queryParams);
        //Debug.Log("API URL: " + query);
        UnityWebRequest request = new UnityWebRequest(query, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            ThreeDInstanceResponse threeDInstanceResponse = JsonConvert.DeserializeObject<ThreeDInstanceResponse>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(threeDInstanceResponse.error))
            {
                onCompleted?.Invoke(threeDInstanceResponse, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + threeDInstanceResponse.error);
                onCompleted?.Invoke(threeDInstanceResponse, false);
            }
        }
        else
        {
            Debug.LogWarning("Error: " + request.error);
            Debug.LogWarning("Server reply: " + request.downloadHandler.text);
            onCompleted?.Invoke(null, false);
        }
        
        request.Dispose();
    }
    
    
    /// <summary>
    /// Obtiene el ícono de un modelo 3D.
    /// </summary>
    /// <param name="id">ID del ícono a obtener.</param>
    /// <param name="onComplete">Acción a ejecutar cuando se completa la descarga del ícono.</param>
    public static IEnumerator GetE3DIcon(int? id, UnityAction<Texture2D, bool> onComplete)
    {
        yield return DownloadDB.GetDownloadByID(id, (rawDownload, success) =>
        {
            if (success && rawDownload.data != null && rawDownload.data.Length > 0)
            {
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                bool loaded = tex.LoadImage(rawDownload.data); // Esto convierte los bytes en textura

                if (loaded)
                {
                    onComplete?.Invoke(tex, true);
                }
                else
                {
                    Debug.LogWarning("Texture couldn't be created from downloaded data.");
                    onComplete?.Invoke(null, false);
                }
            }
            else
            {
                Debug.LogWarning("Empty data or error in the download.");
                onComplete?.Invoke(null, false);
            }
        });
    }

    /// <summary>
    /// Crea una nueva instancia 3D en un modelo 3D.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando se completa la creación de la instancia 3D.</param>
    /// <param name="id">ID del modelo 3D.</param>
    /// <param name="description">Descripción de la instancia 3D.</param>
    /// <param name="detailLevel">Nivel de detalle de la instancia 3D.</param>
    /// <param name="format">Formato de la instancia 3D.</param>
    /// <param name="idFile">ID del archivo asociado a la instancia 3D.</param>
    /// <param name="licenseType">Tipo de licencia de la instancia 3D.</param>
    /// <param name="name">Nombre de la instancia 3D.</param>
    /// <param name="sensorConfiguration">Configuración del sensor para la instancia 3D.</param>
    /// <param name="icon">Ícono de la instancia 3D.</param>
    public static IEnumerator PostNewThreeDInstanceToE3DModel(UnityAction<ThreeDInstanceData, bool> onCompleted,
        int id, string description, DetailLevel detailLevel, Format format, int idFile,
        LicenseType licenseType, string name, string sensorConfiguration, int? icon = null)
    {
        ThreeDInstancePost threeDInstancePostData = new ThreeDInstancePost()
        {
            description = description,
            detail_level = detailLevel.ToString(),
            format = format.ToString(),
            icon = icon,
            id_file = idFile,
            license_type = licenseType.ToString(),
            name = name,
            sensor_configuration = sensorConfiguration
        };

        string jsonData = JsonConvert.SerializeObject(threeDInstancePostData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id + "/e3dinstances", "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Successful creation of ThreeDInstance: " + request.downloadHandler.text);
            ThreeDInstanceData threeDInstanceData = JsonConvert.DeserializeObject<ThreeDInstanceData>(request.downloadHandler.text);

            if (string.IsNullOrEmpty(threeDInstanceData.error))
            {
               //Debug.Log("ThreeDInstance added " + threeDInstanceData.name);
                onCompleted?.Invoke(threeDInstanceData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + threeDInstanceData.error);
                onCompleted?.Invoke(threeDInstanceData, false);
            }
        }
        else
        {
            Debug.LogWarning("Error: " + request.error);
            Debug.LogWarning("Server reply: " + request.downloadHandler.text);
            onCompleted?.Invoke(null, false);
        }

        request.Dispose();
    }
    
    /// <summary>
    /// Obtiene la lista de anotaciones de un modelo 3D.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando se completa la obtención de las anotaciones.</param>
    /// <param name="id">ID del modelo 3D.</param>
    /// <param name="page">Número de página para la paginación.</param>
    /// <param name="perPage">Número de elementos por página.</param>
    /// <param name="search">Cadena de búsqueda para filtrar resultados.</param>
    /// <param name="sort">Campo por el cual ordenar los resultados.</param>
    /// <param name="order">Orden de clasificación (ascendente/descendente).</param>
    public static IEnumerator GetAnnotationListFromE3DModel(UnityAction<AnnotationResponse, bool> onCompleted,
        int id, int? page = null, int? perPage = null,  string search = null,
         string sort = null,  string order = null)
    {
        QueryParams queryParams = new QueryParams()
        {
            page = page,
            per_page = perPage,
            search = search,
            sort = sort,
            order = order
        };
        
        string query = ApiUrl + "/" + id + "/annotations" + QueryManagement.ToQueryString(queryParams);
        //Debug.Log("API URL: " + query);
        UnityWebRequest request = new UnityWebRequest(query, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Annotations obtained successfully: " + request.downloadHandler.text);
            AnnotationResponse annotationResponse = JsonConvert.DeserializeObject<AnnotationResponse>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(annotationResponse.error))
            {
               //Debug.Log("List of Annotation saved with the size of " + annotationResponse.items.Count);
                onCompleted?.Invoke(annotationResponse, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + annotationResponse.error);
                onCompleted?.Invoke(annotationResponse, false);
            }
        }
        else
        {
            Debug.LogWarning("Error: " + request.error);
            Debug.LogWarning("Server reply: " + request.downloadHandler.text);
            onCompleted?.Invoke(null, false);
        }
        
        request.Dispose();
    }
    
    /// <summary>
    /// Obtiene la lista de informes de condición de un modelo 3D.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando se completa la obtención de los informes de condición.</param>
    /// <param name="id">ID del modelo 3D.</param>
    /// <param name="page">Número de página para la paginación.</param>
    /// <param name="perPage">Número de elementos por página.</param>
    /// <param name="search">Cadena de búsqueda para filtrar resultados.</param>
    /// <param name="sort">Campo por el cual ordenar los resultados.</param>
    /// <param name="order">Orden de clasificación (ascendente/descendente).</param>
    public static IEnumerator GetConditionReportListFromE3DModel(UnityAction<ConditionReportResponse, bool> onCompleted,
        int id, int? page = null, int? perPage = null,  string search = null,
         string sort = null,  string order = null)
    {
        QueryParams queryParams = new QueryParams()
        {
            page = page,
            per_page = perPage,
            search = search,
            sort = sort,
            order = order
        };
        
        string query = ApiUrl + "/" + id + "/conditionreports" + QueryManagement.ToQueryString(queryParams);
        //Debug.Log("API URL: " + query);
        UnityWebRequest request = new UnityWebRequest(query, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("ConditionReports obtained successfully: " + request.downloadHandler.text);
            ConditionReportResponse conditionReportResponse = JsonConvert.DeserializeObject<ConditionReportResponse>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(conditionReportResponse.error))
            {
               //Debug.Log("List of ConditionReport saved with the size of " + conditionReportResponse.items.Count);
                onCompleted?.Invoke(conditionReportResponse, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + conditionReportResponse.error);
                onCompleted?.Invoke(conditionReportResponse, false);
            }
        }
        else
        {
            Debug.LogWarning("Error: " + request.error);
            Debug.LogWarning("Server reply: " + request.downloadHandler.text);
            onCompleted?.Invoke(null, false);
        }
        
        request.Dispose();
    }
    
    /// <summary>
    /// Crea un nuevo informe de condición en un modelo 3D.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando se completa la creación del informe de condición.</param>
    /// <param name="id">ID del modelo 3D.</param>
    /// <param name="address">Dirección del lugar.</param>
    /// <param name="author">Autor del informe.</param>
    /// <param name="chInspectionModes">Modos de inspección.</param>
    /// <param name="chUsages">Descripciones de uso.</param>
    /// <param name="colocationCharacteristics">Características de colocación.</param>
    /// <param name="conservationHistory">Historial de conservación.</param>
    /// <param name="culturalSphere">Esfera cultural.</param>
    /// <param name="date">Fecha del informe.</param>
    /// <param name="description">Descripción del informe.</param>
    /// <param name="diagnosisDescription">Descripción del diagnóstico.</param>
    /// <param name="environmentalConditionsCurrentWeatherCondition">Condiciones ambientales actuales (clima).</param>
    /// <param name="environmentalConditionsPastWeekWeatherCondition">Condiciones ambientales de la semana pasada (clima).</param>
    /// <param name="environmentalConditionsRelativeHumidity">Humedad relativa.</param>
    /// <param name="environmentalConditionsTemperature">Temperatura.</param>
    /// <param name="idAnnotations">IDs de las anotaciones asociadas.</param>
    /// <param name="legalStatus">Estado legal.</param>
    /// <param name="municipality">Municipio.</param>
    /// <param name="name">Nombre del informe.</param>
    /// <param name="objectString">Cadena del objeto.</param>
    /// <param name="parentAssetName">Nombre del activo padre.</param>
    /// <param name="province">Provincia.</param>
    /// <param name="referenceCartographyCadastralParcel">Referencia cartográfica (parcela catastral).</param>
    /// <param name="referenceCartographyCadastralSheet">Referencia cartográfica (hoja catastral).</param>
    /// <param name="referenceCartographyMunicipality">Referencia cartográfica (municipio).</param>
    /// <param name="referenceCartographyOther">Referencia cartográfica (otra).</param>
    /// <param name="reportReferences">Referencias del informe.</param>
    /// <param name="specificLocation">Ubicación específica.</param>
    /// <param name="stateOfConservation">Estado de conservación.</param>
    /// <param name="status">Estado del informe.</param>
    /// <param name="subject">Asunto del informe.</param>
    /// <param name="surveyDate">Fecha de la encuesta.</param>
    /// <param name="surveyResponsible">Responsable de la encuesta.</param>
    /// <param name="techniques">Técnicas utilizadas.</param>
    public static IEnumerator PostNewConditionReportToE3DModel(UnityAction<ConditionReportData, bool> onCompleted,
        int id,  string address = null,  string author = null,
         List<ChInspectionMode> chInspectionModes = null,  List<ChUsageDescription> chUsages = null,
         string colocationCharacteristics = null,  string conservationHistory = null,
         string culturalSphere = null,  string date = null,  string description = null,
        string diagnosisDescription = null,  string environmentalConditionsCurrentWeatherCondition = null,
         string environmentalConditionsPastWeekWeatherCondition = null,
         string environmentalConditionsRelativeHumidity = null,  string environmentalConditionsTemperature = null,
         List<int> idAnnotations = null,  string legalStatus = null,  string municipality = null,
        string name = null,  string objectString = null,  string parentAssetName = null,
         string province = null,  string referenceCartographyCadastralParcel = null,
         string referenceCartographyCadastralSheet = null,  string referenceCartographyMunicipality = null,
         string referenceCartographyOther = null,  string reportReferences = null,
         string specificLocation = null, StateOfConservationType? stateOfConservation = null, StatusType? status = null,
         string subject = null,  string surveyDate = null,  string surveyResponsible = null,
         string techniques = null)
    {
        ConditionReportPost conditionReportPostData = new ConditionReportPost()
        {
            address = address,
            author = author,
            ch_inspection_modes = chInspectionModes,
            ch_usages = chUsages,
            colocation_characteristics = colocationCharacteristics,
            conservation_history = conservationHistory,
            cultural_sphere = culturalSphere,
            date = date,
            description = description,
            diagnosis_description = diagnosisDescription,
            environmental_conditions_current_weather_condition = environmentalConditionsCurrentWeatherCondition,
            environmental_conditions_past_week_weather_condition = environmentalConditionsPastWeekWeatherCondition,
            environmental_conditions_relative_humidity = environmentalConditionsRelativeHumidity,
            environmental_conditions_temperature = environmentalConditionsTemperature,
            id_annotations = idAnnotations,
            legal_status = legalStatus,
            municipality = municipality,
            name = name,
            object_string = objectString,
            parent_asset_name = parentAssetName,
            province = province,
            reference_cartography_cadastral_parcel = referenceCartographyCadastralParcel,
            reference_cartography_cadastral_sheet = referenceCartographyCadastralSheet,
            reference_cartography_municipality = referenceCartographyMunicipality,
            reference_cartography_other = referenceCartographyOther,
            report_references = reportReferences,
            specific_location = specificLocation,
            state_of_conservation = stateOfConservation.ToString(),
            status = status.ToString(),
            subject = subject,
            survey_date = surveyDate,
            survey_responsible = surveyResponsible,
            techniques = techniques
        };
        
        string jsonData = JsonConvert.SerializeObject(conditionReportPostData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id + "/conditionreports", "POST");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Successful creation of ConditionReport: " + request.downloadHandler.text);
            ConditionReportData conditionReportData = JsonConvert.DeserializeObject<ConditionReportData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(conditionReportData.error))
            {
               //Debug.Log("ConditionReport added " + conditionReportData.name);
                onCompleted?.Invoke(conditionReportData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + conditionReportData.error);
                onCompleted?.Invoke(conditionReportData, false);
            }
        }
        else
        {
            Debug.LogWarning("Error: " + request.error);
            Debug.LogWarning("Server reply: " + request.downloadHandler.text);
            onCompleted?.Invoke(null, false);
        }
        
        request.Dispose();
    }
    
    /// <summary>
    /// Obtiene la lista de eventos de alteración de un modelo 3D.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando se completa la obtención de los eventos de alteración.</param>
    /// <param name="id">ID del modelo 3D.</param>
    /// <param name="page">Número de página para la paginación.</param>
    /// <param name="perPage">Número de elementos por página.</param>
    /// <param name="search">Cadena de búsqueda para filtrar resultados.</param>
    /// <param name="sort">Campo por el cual ordenar los resultados.</param>
    /// <param name="order">Orden de clasificación (ascendente/descendente).</param>
    /// <param name="alterationId">Lista de IDs de alteraciones para filtrar resultados.</param>
    /// <param name="materialId">Lista de IDs de materiales para filtrar resultados.</param>
    /// <param name="yearFrom">Año de inicio para filtrar por año.</param>
    /// <param name="yearTo">Año de fin para filtrar por año.</param>
    public static IEnumerator GetAlterationEventListFromE3DModel(UnityAction<AlterationEventResponse, bool> onCompleted,
        int id, int? page = null, int? perPage = null,  string search = null,
         string sort = null,  string order = null,  List<int> alterationId = null,
         List<int> materialId = null, int? yearFrom = null, int? yearTo = null)
    {
        QueryParams queryParams = new QueryParams()
        {
            page = page,
            per_page = perPage,
            search = search,
            sort = sort,
            order = order,
            alteration_id = alterationId,
            material_id = materialId,
            year_from = yearFrom,
            year_to = yearTo
        };
        
        string query = ApiUrl + "/" + id + "/alterationevents" + QueryManagement.ToQueryString(queryParams);
        //Debug.Log("API URL: " + query);
        UnityWebRequest request = new UnityWebRequest(query, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("AlterationEvents obtained successfully: " + request.downloadHandler.text);
            AlterationEventResponse alterationEventResponse = JsonConvert.DeserializeObject<AlterationEventResponse>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(alterationEventResponse.error))
            {
               //Debug.Log("List of AlterationEvent saved with the size of " + alterationEventResponse.items.Count);
                onCompleted?.Invoke(alterationEventResponse, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + alterationEventResponse.error);
                onCompleted?.Invoke(alterationEventResponse, false);
            }
        }
        else
        {
            Debug.LogWarning("Error: " + request.error);
            Debug.LogWarning("Server reply: " + request.downloadHandler.text);
            onCompleted?.Invoke(null, false);
        }
        
        request.Dispose();
    }
}