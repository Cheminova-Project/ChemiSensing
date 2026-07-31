using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// Proporciona acceso a la API de elementos patrimoniales (CH Elements).
/// Gestiona la configuración y la URL base de la API para elementos patrimoniales.
/// </summary>
public static class CHElementDB
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
    /// Obtiene una lista de elementos patrimoniales (CH Elements) desde la API.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando la solicitud se completa.</param>
    /// <param name="page">Número de página para la paginación.</param>
    /// <param name="perPage">Número de elementos por página.</param>
    /// <param name="search">Cadena de búsqueda para filtrar elementos.</param>
    /// <param name="sort">Campo por el cual ordenar los resultados.</param>
    /// <param name="order">Orden de clasificación (ascendente/descendente).</param>
    /// <param name="alterationId">Lista de IDs de alteraciones para filtrar.</param>
    /// <param name="materialId">Lista de IDs de materiales para filtrar.</param>
    /// <param name="yearFrom">Año de inicio para el filtro por año.</param>
    /// <param name="yearTo">Año de fin para el filtro por año.</param>
    /// <param name="scale">Tipo de escala para filtrar.</param>
    /// <param name="category">Categoría de patrimonio para filtrar.</param>
    /// <param name="stateOfConservation">Estado de conservación para filtrar.</param>
    /// <param name="countryId">ID del país para filtrar.</param>
    /// <param name="provinceId">ID de la provincia para filtrar.</param>
    /// <param name="municipalityId">ID del municipio para filtrar.</param>
    public static IEnumerator GetCHElementList(UnityAction<CHElementResponse, bool> onCompleted,
        int? page = null, int? perPage = null,  string search = null,
         string sort = null,  string order = null,  List<int> alterationId = null,
         List<int> materialId = null, int? yearFrom = null, int? yearTo = null, ScaleType? scale = null,
        HeritageCategory? category = null, StateOfConservationType? stateOfConservation = null,
        int? countryId = null, int? provinceId = null, int? municipalityId = null)
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
            year_to = yearTo,
            scale = scale.ToString(),
            category = category.ToString(),
            state_of_conservation = stateOfConservation.ToString(),
            country_id = countryId,
            province_id = provinceId,
            municipality_id = municipalityId
        };
        
        string query = ApiUrl + QueryManagement.ToQueryString(queryParams);
        UnityWebRequest request = new UnityWebRequest(query, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            CHElementResponse chelementResponse = JsonConvert.DeserializeObject<CHElementResponse>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(chelementResponse.error))
                onCompleted?.Invoke(chelementResponse, true);
            
            else
            {
                Debug.LogWarning("Error in server reply: " + chelementResponse.error);
                onCompleted?.Invoke(chelementResponse, false);
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
    /// Crea un nuevo elemento patrimonial (CH Element) en la API.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando la solicitud se completa.</param>
    /// <param name="approximateDate">Indica si la fecha es aproximada.</param>
    /// <param name="description">Descripción del elemento.</param>
    /// <param name="endYear">Año de finalización.</param>
    /// <param name="heritageCategory">Categoría de patrimonio.</param>
    /// <param name="name">Nombre del elemento.</param>
    /// <param name="scale">Tipo de escala.</param>
    /// <param name="spatialDimension">Dimensión espacial.</param>
    /// <param name="startYear">Año de inicio.</param>
    /// <param name="address">Dirección del elemento.</param>
    /// <param name="author">Autor del elemento.</param>
    /// <param name="chElementMaterials">Lista de materiales del elemento.</param>
    /// <param name="culturalSphere">Esfera cultural del elemento.</param>
    /// <param name="icon">ID del ícono del elemento.</param>
    /// <param name="idCountry">ID del país.</param>
    /// <param name="idMunicipality">ID del municipio.</param>
    /// <param name="idProvince">ID de la provincia.</param>
    /// <param name="idSite">ID del sitio.</param>
    /// <param name="latitude">Latitud del elemento.</param>
    /// <param name="longitude">Longitud del elemento.</param>
    public static IEnumerator PostNewCHElement(UnityAction<CHElementData, bool> onCompleted,
        bool approximateDate, string description, int endYear, HeritageCategory heritageCategory,
        string name, ScaleType scale, SpatialDimension spatialDimension, int startYear,
         string address = null,  string author = null,
         List<ChElementMaterial> chElementMaterials = null,  string culturalSphere = null,
        int? icon = null, int? idCountry = null, int? idMunicipality = null, int? idProvince = null,
        int? idSite = null, float? latitude = null, float? longitude = null)
    {
        CHElementPost chElementPostData = new CHElementPost()
        {
            address = address,
            approximate_date = approximateDate,
            author = author,
            chelement_materials = chElementMaterials,
            cultural_sphere = culturalSphere,
            description = description,
            end_year = endYear,
            heritage_category = heritageCategory.ToString(),
            icon = icon,
            id_country = idCountry,
            id_municipality = idMunicipality,
            id_province = idProvince,
            id_site = idSite,
            latitude = latitude,
            longitude = longitude,
            name = name,
            scale = scale.ToString(),
            spatial_dimension = spatialDimension.ToString(),
            start_year = startYear,
        };

        string jsonData = JsonConvert.SerializeObject(chElementPostData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        UnityWebRequest request = new UnityWebRequest(ApiUrl, "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            CHElementData chElementData = JsonConvert.DeserializeObject<CHElementData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(chElementData.error))
            {
                //Debug.Log("CHElement added " + chElementData.name);
                onCompleted?.Invoke(chElementData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + chElementData.error);
                onCompleted?.Invoke(chElementData, false);
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
    /// Obtiene una lista de elementos patrimoniales (CH Elements) por sus IDs.
    /// </summary>
    /// <param name="ids">Lista de IDs de los elementos a obtener.</param>
    /// <param name="onCompleted">Acción a ejecutar cuando la solicitud se completa.</param>
    public static IEnumerator GetCHElementByIDList(List<int> ids, UnityAction<List<CHElementData>, bool> onCompleted)
    {
        List<CHElementData> results = new List<CHElementData>();
        bool hasError = false;
        int completedCount = 0;

        foreach (int id in ids)
        {
            yield return GetCHElementByID((data, success) =>
            {
                completedCount++;
                if (success && data != null)
                {
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
    /// Descarga y crea una textura a partir del ícono de un elemento patrimonial.
    /// </summary>
    /// <param name="id">ID del elemento.</param>
    /// <param name="onComplete">Acción a ejecutar cuando la descarga se completa.</param>
    public static IEnumerator GetCHElementIcon(int? id, UnityAction<Texture2D, bool> onComplete)
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
                    //Debug.LogWarning("Texture couldn't be created from downloaded data.");
                    onComplete?.Invoke(null, false);
                }
            }
            else
            {
                //Debug.LogWarning("Empty data or error in the download.");
                onComplete?.Invoke(null, false);
            }
        });
    }

    
    /// <summary>
    /// Elimina un elemento patrimonial (CH Element) de la API.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando la solicitud se completa.</param>
    /// <param name="id">ID del elemento a eliminar.</param>
    public static IEnumerator DeleteCHElementFromApi(UnityAction<ErrorMessage, bool> onCompleted, int id)
    {
        UnityWebRequest request = UnityWebRequest.Delete(ApiUrl + "/" + id);
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
           //Debug.Log("Successful elimination of CHElement: " + request.downloadHandler.text);
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
    /// Obtiene un elemento patrimonial (CH Element) por su ID.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando la solicitud se completa.</param>
    /// <param name="id">ID del elemento a obtener.</param>
    public static IEnumerator GetCHElementByID(UnityAction<CHElementData, bool> onCompleted, int id)
    {
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log("CHElement obtained successfully: " + request.downloadHandler.text);
            CHElementData chElementData = JsonConvert.DeserializeObject<CHElementData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(chElementData.error))
            {
                //Debug.Log("CHElement: " + chElementData.name);
                onCompleted?.Invoke(chElementData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + chElementData.error);
                onCompleted?.Invoke(chElementData, false);
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
    /// Actualiza un elemento patrimonial (CH Element) en la API.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando la solicitud se completa.</param>
    /// <param name="id">ID del elemento a actualizar.</param>
    /// <param name="approximateDate">Indica si la fecha es aproximada.</param>
    /// <param name="description">Descripción del elemento.</param>
    /// <param name="endYear">Año de finalización.</param>
    /// <param name="heritageCategory">Categoría de patrimonio.</param>
    /// <param name="name">Nombre del elemento.</param>
    /// <param name="scale">Tipo de escala.</param>
    /// <param name="spatialDimension">Dimensión espacial.</param>
    /// <param name="startYear">Año de inicio.</param>
    /// <param name="address">Dirección del elemento.</param>
    /// <param name="author">Autor del elemento.</param>
    /// <param name="chElementMaterials">Lista de materiales del elemento.</param>
    /// <param name="culturalSphere">Esfera cultural del elemento.</param>
    /// <param name="icon">ID del ícono del elemento.</param>
    /// <param name="idCountry">ID del país.</param>
    /// <param name="idMunicipality">ID del municipio.</param>
    /// <param name="idProvince">ID de la provincia.</param>
    /// <param name="idSite">ID del sitio.</param>
    /// <param name="latitude">Latitud del elemento.</param>
    /// <param name="longitude">Longitud del elemento.</param>
    public static IEnumerator PutCHElementFromApi(UnityAction<CHElementData, bool> onCompleted, int id,
        bool? approximateDate = null,  string description = null, int? endYear = null,
        HeritageCategory? heritageCategory = null,  string name = null, ScaleType? scale = null,
        SpatialDimension? spatialDimension = null, int? startYear = null,  string address = null,
         string author = null,  List<ChElementMaterial> chElementMaterials = null,
         string culturalSphere = null, int? icon = null, int? idCountry = null,
        int? idMunicipality = null, int? idProvince = null, int? idSite = null, float? latitude = null, float? longitude = null)
    {
        CHElementPost chElementPutData = new CHElementPost()
        {
            address = address,
            approximate_date = approximateDate,
            author = author,
            chelement_materials = chElementMaterials,
            cultural_sphere = culturalSphere,
            description = description,
            end_year = endYear,
            heritage_category = heritageCategory.ToString(),
            icon = icon,
            id_country = idCountry,
            id_municipality = idMunicipality,
            id_province = idProvince,
            id_site = idSite,
            latitude = latitude,
            longitude = longitude,
            name = name,
            scale = scale.ToString(),
            spatial_dimension = spatialDimension.ToString(),
            start_year = startYear,
        };
        
        string jsonData = JsonConvert.SerializeObject(chElementPutData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id, "PUT");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log("CHElement changed successfully: " + request.downloadHandler.text);
            CHElementData chElementData = JsonConvert.DeserializeObject<CHElementData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(chElementData.error))
            {
                //Debug.Log("CHElement changed: " + chElementData.name);
                onCompleted?.Invoke(chElementData, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + chElementData.error);
                onCompleted?.Invoke(chElementData, false);
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
    /// Obtiene una lista de modelos 3D (E3D Models) asociados a un elemento patrimonial.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando la solicitud se completa.</param>
    /// <param name="id">ID del elemento patrimonial.</param>
    /// <param name="page">Número de página para la paginación.</param>
    /// <param name="perPage">Número de elementos por página.</param>
    /// <param name="search">Cadena de búsqueda para filtrar modelos.</param>
    /// <param name="sort">Campo por el cual ordenar los resultados.</param>
    /// <param name="order">Orden de clasificación (ascendente/descendente).</param>
    /// <param name="alterationId">Lista de IDs de alteraciones para filtrar.</param>
    /// <param name="materialId">Lista de IDs de materiales para filtrar.</param>
    /// <param name="referenceDateFrom">Fecha de referencia de inicio para el filtro.</param>
    /// <param name="referenceDateTo">Fecha de referencia de fin para el filtro.</param>
    public static IEnumerator GetE3DModelListFromCHElement(UnityAction<E3DModelResponse, bool> onCompleted,
        int id, int? page = null, int? perPage = null,  string search = null,
         string sort = null,  string order = null,  List<int> alterationId = null,
         List<int> materialId = null,  string referenceDateFrom = null,
         string referenceDateTo = null)
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
            reference_date_from = referenceDateFrom,
            reference_date_to = referenceDateTo
        };
        
        string query = ApiUrl + "/" + id + "/e3ds" + QueryManagement.ToQueryString(queryParams);
        //Debug.Log("API URL: " + query);
        UnityWebRequest request = new UnityWebRequest(query, "GET");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log("E3DModels obtained successfully: " + request.downloadHandler.text);
            E3DModelResponse e3DModelResponse = JsonConvert.DeserializeObject<E3DModelResponse>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(e3DModelResponse.error))
            {
                //Debug.Log("List of E3DModel saved with the size of " + e3DModelResponse.items.Count);
                onCompleted?.Invoke(e3DModelResponse, true);
            }
            else
            {
                Debug.LogWarning("Error in server reply: " + e3DModelResponse.error);
                onCompleted?.Invoke(e3DModelResponse, false);
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
    /// Crea un nuevo modelo 3D (E3D Model) asociado a un elemento patrimonial.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando la solicitud se completa.</param>
    /// <param name="id">ID del elemento patrimonial.</param>
    /// <param name="description">Descripción del modelo.</param>
    /// <param name="height">Altura del modelo.</param>
    /// <param name="length">Longitud del modelo.</param>
    /// <param name="name">Nombre del modelo.</param>
    /// <param name="referenceDate">Fecha de referencia del modelo.</param>
    /// <param name="scale">Escala del modelo.</param>
    /// <param name="width">Anchura del modelo.</param>
    /// <param name="icon">ID del ícono del modelo.</param>
    public static IEnumerator PostNewE3DModelToCHElement(UnityAction<E3DModelData, bool> onCompleted,
        int id, string description, float height, float length, string name,
        string referenceDate, float scale, float width, int? icon = null)
    {
        E3DModelPost alterationFormPostData = new E3DModelPost()
        {
            description = description,
            height = height,
            length = length,
            name = name,
            reference_date = referenceDate,
            scale = scale,
            width = width,
            icon = icon
        };

        string jsonData = JsonConvert.SerializeObject(alterationFormPostData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        
        UnityWebRequest request = new UnityWebRequest(ApiUrl + "/" + id + "/e3ds", "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + GlobalManagement.Instance.token);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log("Successful creation of E3DModel: " + request.downloadHandler.text);
            E3DModelData e3DModelData = JsonConvert.DeserializeObject<E3DModelData>(request.downloadHandler.text);
            
            if (string.IsNullOrEmpty(e3DModelData.error))
            {
                //Debug.Log("E3DModel added " + e3DModelData.name);
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
    /// Obtiene una lista de datos anexos (Annex Data) asociados a un elemento patrimonial.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando la solicitud se completa.</param>
    /// <param name="id">ID del elemento patrimonial.</param>
    /// <param name="page">Número de página para la paginación.</param>
    /// <param name="perPage">Número de elementos por página.</param>
    /// <param name="search">Cadena de búsqueda para filtrar datos anexos.</param>
    /// <param name="sort">Campo por el cual ordenar los resultados.</param>
    /// <param name="order">Orden de clasificación (ascendente/descendente).</param>
    /// <param name="mimeType">Lista de tipos MIME para filtrar.</param>
    public static IEnumerator GetAnnexDataListFromCHElement(UnityAction<AnnexDataResponse, bool> onCompleted,
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
    /// Obtiene una lista de anotaciones (Annotations) asociadas a un elemento patrimonial.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando la solicitud se completa.</param>
    /// <param name="id">ID del elemento patrimonial.</param>
    /// <param name="page">Número de página para la paginación.</param>
    /// <param name="perPage">Número de elementos por página.</param>
    /// <param name="search">Cadena de búsqueda para filtrar anotaciones.</param>
    /// <param name="sort">Campo por el cual ordenar los resultados.</param>
    /// <param name="order">Orden de clasificación (ascendente/descendente).</param>
    public static IEnumerator GetAnnotationListFromCHElement(UnityAction<AnnotationResponse, bool> onCompleted,
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
    /// Obtiene una lista de informes de condición (Condition Reports) asociados a un elemento patrimonial.
    /// </summary>
    /// <param name="onCompleted">Acción a ejecutar cuando la solicitud se completa.</param>
    /// <param name="id">ID del elemento patrimonial.</param>
    /// <param name="page">Número de página para la paginación.</param>
    /// <param name="perPage">Número de elementos por página.</param>
    /// <param name="search">Cadena de búsqueda para filtrar informes de condición.</param>
    /// <param name="sort">Campo por el cual ordenar los resultados.</param>
    /// <param name="order">Orden de clasificación (ascendente/descendente).</param>
    public static IEnumerator GetConditionReportListFromCHElement(UnityAction<ConditionReportResponse, bool> onCompleted,
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
}