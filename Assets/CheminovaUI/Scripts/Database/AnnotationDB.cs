using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// Proporciona acceso a la API de anotaciones.
/// Gestiona la configuración y la URL base de la API para anotaciones.
/// </summary>
public static class AnnotationDB
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
    /// Elimina una anotación a través de la API.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar al completar la solicitud.</param>
    /// <param name="id">ID de la anotación a eliminar.</param>
    /// <returns>Coroutine para la eliminación de la anotación.</returns>
    public static IEnumerator DeleteAnnotationFromApi(UnityAction<ErrorMessage, bool> onCompleted, int id)
    {
        UnityWebRequest request = UnityWebRequest.Delete(ApiUrl + "/" + id);
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            ErrorMessage errorMessage = null;

            if (!string.IsNullOrWhiteSpace(responseText))
                errorMessage = JsonConvert.DeserializeObject<ErrorMessage>(responseText);
            
            // Si no hay objeto de error (respuesta vacía) o no hay mensaje de error, es un éxito
            if (errorMessage == null || string.IsNullOrEmpty(errorMessage.error))
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
    /// Obtiene una anotación por su ID a través de la API.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar al completar la solicitud.</param>
    /// <param name="id">ID de la anotación a obtener.</param>
    /// <returns>Coroutine para la obtención de la anotación.</returns>
    public static IEnumerator GetAnnotationByID(UnityAction<AnnotationData, bool> onCompleted, int id)
    {
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Annotation obtained successfully: " + request.downloadHandler.text);
            AnnotationData annotationData = JsonConvert.DeserializeObject<AnnotationData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(annotationData.error))
            {
               //Debug.Log("Annotation: " + annotationData.name);
                onCompleted?.Invoke(annotationData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + annotationData.error);
                onCompleted?.Invoke(annotationData, false);
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
    /// Actualiza una anotación a través de la API.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar al completar la solicitud.</param>
    /// <param name="id">ID de la anotación a actualizar.</param>
    /// <param name="description">Descripción de la anotación.</param>
    /// <param name="icon">Ícono de la anotación.</param>
    /// <param name="idGroup">ID del grupo al que pertenece la anotación.</param>
    /// <param name="idTextures">Lista de IDs de texturas asociadas a la anotación.</param>
    /// <param name="name">Nombre de la anotación.</param>
    /// <param name="shapeAndContext">Forma y contexto de la anotación.</param>
    /// <param name="type">Tipo de la anotación.</param>
    /// <returns>Coroutine para la actualización de la anotación.</returns>
    public static IEnumerator PutAnnotationFromApi(UnityAction<AnnotationData, bool> onCompleted,
        int id, float? alpha = null, List<AnnotationGeometry> annotationGeometries = null,
        AnnotationCategory? category = null, Color? color = null,  string description = null,
        int? icon = null, int? idGroup = null,  List<int> idTextures = null,  string name = null,
        float? radius = null, Vector3? referenceNormal = null, Vector3? referencePoint = null, bool? reverseMode = null,
        int? shapeAndContext = null, string transformationMatrix = null, AnnotationType? type = null,
         string userTransformationMatrix = null, AnnotationVisualizationType? visualizationType = null)
    {
        AnnotationPost annotationPutData = new AnnotationPost()
        {
            alpha = alpha,
            annotation_geometries = annotationGeometries,
            category = HelpFunctionsConditionReport.AnnotationCategoryToStringDatabase(category),
            color = color,
            description = description,
            icon = icon,
            id_group = idGroup,
            id_textures = idTextures,
            name = name,
            radius = radius,
            reference_normal = referenceNormal,
            reference_point = referencePoint,
            reverse_mode = reverseMode,
            shape_and_context = shapeAndContext,
            transformation_matrix = transformationMatrix,
            type = HelpFunctionsConditionReport.AnnotationTypeToString(type),
            user_transformation_matrix = userTransformationMatrix,
            vizualization_type = HelpFunctionsConditionReport.AnnotationVisualizationTypeToString(visualizationType)
        };
        
        string jsonData = JsonConvert.SerializeObject(annotationPutData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id, "PUT");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Annotation changed successfully: " + request.downloadHandler.text);
            AnnotationData annotationData = JsonConvert.DeserializeObject<AnnotationData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(annotationData.error))
            {
               //Debug.Log("Annotation changed: " + annotationData.name);
                onCompleted?.Invoke(annotationData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + annotationData.error);
                onCompleted?.Invoke(annotationData, false);
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
    /// Obtiene la lista de eventos de alteración asociados a una anotación.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar al completar la solicitud.</param>
    /// <param name="id">ID de la anotación.</param>
    /// <param name="page">Número de página para la paginación.</param>
    /// <param name="perPage">Número de elementos por página.</param>
    /// <param name="search">Término de búsqueda.</param>
    /// <param name="sort">Campo por el cual ordenar los resultados.</param>
    /// <param name="order">Orden de clasificación (ascendente/descendente).</param>
    /// <param name="alterationId">Lista de IDs de alteraciones.</param>
    /// <param name="materialId">Lista de IDs de materiales.</param>
    /// <param name="yearFrom">Año de inicio para el filtrado por año.</param>
    /// <param name="yearTo">Año de fin para el filtrado por año.</param>
    /// <returns>Coroutine para la obtención de la lista de eventos de alteración.</returns>
    public static IEnumerator GetAlterationEventListFromAnnotation(UnityAction<AlterationEventResponse, bool> onCompleted,
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
    
    /// <summary>
    /// Crea un nuevo evento de alteración asociado a una anotación.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar al completar la solicitud.</param>
    /// <param name="id">ID de la anotación.</param>
    /// <param name="description">Descripción del evento de alteración.</param>
    /// <param name="idAlterations">Lista de IDs de alteraciones.</param>
    /// <param name="idAnnexData">Lista de IDs de datos anexos.</param>
    /// <param name="idMaterial">ID del material asociado.</param>
    /// <param name="name">Nombre del evento de alteración.</param>
    /// <param name="idAlterationAgent">ID del agente de alteración (opcional).</param>
    /// <returns>Coroutine para la creación del nuevo evento de alteración.</returns>
    public static IEnumerator PostNewAlterationEventToAnnotation(UnityAction<AlterationEventData, bool> onCompleted,
        int id, string description, List<int> idAlterations, List<int> idAnnexData, int idMaterial,
        string name, int? idAlterationAgent = null)
    {
        AlterationEventPost alterationEventPostData = new AlterationEventPost()
        {
            description = description,
            id_alteration_agent = idAlterationAgent,
            id_alterations = idAlterations,
            id_annex_data = idAnnexData,
            id_material = idMaterial,
            name = name
        };
        
        string jsonData = JsonConvert.SerializeObject(alterationEventPostData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id + "/alterationevents", "POST");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Successful creation of AlterationEvent: " + request.downloadHandler.text);
            AlterationEventData alterationEventData = JsonConvert.DeserializeObject<AlterationEventData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(alterationEventData.error))
            {
               //Debug.Log("AlterationEvent added " + alterationEventData.name);
                onCompleted?.Invoke(alterationEventData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + alterationEventData.error);
                onCompleted?.Invoke(alterationEventData, false);
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