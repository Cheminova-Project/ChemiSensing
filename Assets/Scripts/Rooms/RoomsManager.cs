using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Unity.Netcode;
using System;
using Unity.Netcode.Transports.UTP;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// Gestiona el conjunto de salas disponibles en la aplicación.
/// Permite crear, eliminar y acceder a salas.
/// </summary>
// ==================== Clases de message ====================

public class message
{
    public string command { get; set; }
    public string creator { get; set; }
    public string roomName { get; set; }
    public int chElementId { get; set; }
    public int e3dModelId { get; set; }
    public string bearerToken { get; set; }
    public bool hasAudio { get; set; }
    public string user { get; set; }
    public List<string> acceptedUsers { get; set; }
    public int port {  get; set; }
}

/// <summary>
/// Respuesta genérica del servidor con status y message
/// </summary>
public class RespuestaServidor
{
    public string status { get; set; }
    public string message { get; set; }
    public int port { get; set; }
    public List<string> validCommands { get; set; }
}

/// <summary>
/// Información de una sala activa en el servidor
/// </summary>
public class Servicio
{
    public string creator { get; set; }
    public string roomName { get; set; }
    public int chElementId { get; set; }
    public int e3dModelId { get; set; }
    public int port { get; set; }
    public bool hasAudio { get; set; }
    public List<string> acceptedUsers { get; set; }
    public int actualPlayers { get; set; }
}

public class ErrorFastAPI
{
    public string detail { get; set; }
}

/// <summary>
/// Gestor de salas para conectar con el servidor Python de ChemiSensing
/// Maneja la creación, actualización y conexión a salas
/// </summary>
public class RoomsManager : MonoBehaviour
{
    [Header("UI Controllers")]
    public RoomManagerUIDocController _roomManagerUIDocController; // New UI Toolkit Controller
    
    [Header("Configuración de conexión")]
    [SerializeField] private float tiempoTimeoutSocket = 30f;
    [SerializeField] private int tamanoBufferRespuesta = 4096;
    [SerializeField] private float delayActualizacionSalas = 0.5f;
    private string lastRoomsJson = "";
    private Coroutine _serverSyncCoroutine;
    
    private const int port_SERVIDOR_MANAGER = 7776;
    
    public static RoomsManager Instance { get; private set; }

    ConnectionManager connectionManager => ConnectionManager.Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Instance._roomManagerUIDocController = this._roomManagerUIDocController;
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(this.gameObject);
    }
    
    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted += SetupServerCallbacks;
            
            if (NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsListening) 
                SetupServerCallbacks();
            
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientLocalDisconnected;
        }
    }
    
    private void SetupServerCallbacks()
    {
        //Debug.Log("[RoomsManager] Servidor Headless de Netcode iniciado correctamente.");
    }
    
    private void OnClientLocalDisconnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer) return;

        // Si se nos cae internet, Netcode nos echa y nos devuelve nuestro propio ID o 0
        if (clientId == 0 || clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.LogWarning("[RoomsManager] ¡Caída de red detectada (WiFi)! Volviendo al menú principal...");
            StartCoroutine(ForcedDisconnectRoutine());
        }
    }

    private IEnumerator ForcedDisconnectRoutine()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            yield return new WaitWhile(() => NetworkManager.Singleton != null && NetworkManager.Singleton.ShutdownInProgress);
        }

        if (GlobalVariables.Instance != null)
        {
            GlobalVariables.Instance.SetSelectedCHElementData(null);
            GlobalVariables.Instance.SetSelectedE3DModelData(null);
        }

        if (this != null && this.gameObject != null)
            Destroy(this.gameObject);
    
        if (Application.isPlaying)
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainScene");
    }

    // Añade esto al final de la clase para limpiar cuando salgas de la escena
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Conecta a una sala seleccionada
    /// </summary>
    public void ConnectToRoom(int port)
    {
        RoomState.CurrentRoomPort = port;
        if (NetworkServerConfiguration.Instance.connectMode == ConnectMode.LOCAL)
        {
            StartClientDelayed("127.0.0.1", (ushort)port);
            return;
        }
        EnviarMensaje("ENTER_ROOM", port: port, user: GlobalManagement.Instance.username);
    }
    
    public void LeaveRoom()
    {
        if (RoomState.CurrentRoomPort != -1)
        {
            string uname = GlobalManagement.Instance != null ? GlobalManagement.Instance.username : "Unknown";
            if (NetworkServerConfiguration.Instance.connectMode != ConnectMode.LOCAL)
                EnviarMensaje("EXIT_ROOM", port: RoomState.CurrentRoomPort, user: uname);
            RoomState.CurrentRoomPort = -1;
        }

        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.Shutdown();
    }

    private void StartClientDelayed(string ip, ushort port)
    {
        // Acción encapsulada para conectar
        Action conectarAccion = () => 
        {
            if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer))
            {
                Debug.LogWarning("[RoomsManager] NetworkManager estaba sucio. Forzando Shutdown...");
                NetworkManager.Singleton.Shutdown();
            }

            connectionManager.StartClient(ip, port);
        };

        if(XRFade.Instance == null)
            conectarAccion.Invoke();
        else
            StartCoroutine(WaitFadeAndConnect(conectarAccion));
    }
    
    private IEnumerator WaitFadeAndConnect(System.Action onConnect)
    {
        bool fadeCompleted = false;
        
        XRFade.Instance?.FadeOut(() => { fadeCompleted = true; });

        float timer = 0f;
        while (!fadeCompleted && timer < 2.0f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!fadeCompleted) Debug.LogWarning("[RoomsManager] FadeOut timeout. Conectando forzosamente.");
        
        onConnect.Invoke();
    }

    
    /// <summary>
    /// Crea una nueva sala en el servidor
    /// </summary>
    public void CreateRoom(UnityAction<RespuestaServidor> onRoomCreated = null)
    {
        CHElementData selectedElement = GlobalVariables.Instance.GetSelectedCHElementData();
        E3DModelData selectedModel = GlobalVariables.Instance.GetSelectedE3DModelData();

        if (selectedElement == null)
        {
            Debug.LogError("[RoomsManager] No hay CH element seleccionado para crear la sala");
            return;
        }
        
        if (selectedModel == null)
        {
            Debug.LogError("[RoomsManager] No hay modelo seleccionado para crear la sala");
            return;
        }
        
        if (string.IsNullOrEmpty(GlobalManagement.Instance.username))
        {
            Debug.LogError("[RoomsManager] Username vacío");
            return;
        }
        
        //Comprobación de modo development local
        if(NetworkServerConfiguration.Instance.connectMode == ConnectMode.LOCAL)
        {
            // Simulamos una respuesta exitosa para desbloquear la UI
            RespuestaServidor mockResponse = new RespuestaServidor 
            { 
                status = "active", 
                port = 7777, // Puerto por defecto en local
                message = "Local room active" 
            };

            if (onRoomCreated != null)
                onRoomCreated.Invoke(mockResponse);

            return;
        }
        
        bool isAudioRoom = GlobalVariables.Instance.GetIsAudioRoom();
        List<string> acceptedUsers = GlobalVariables.Instance.GetAcceptedUsers();
        
        if(XRFade.Instance == null)
        {
            EnviarMensaje("OPEN_ROOM", selectedElement.id, selectedModel.id, GlobalManagement.Instance.username, GlobalVariables.Instance.GetRoomName(), hasAudio:isAudioRoom, acceptedUsers: acceptedUsers, onRoomCreated: onRoomCreated);
        }
        else
        {
            XRFade.Instance?.FadeOut(() =>
            {
                EnviarMensaje("OPEN_ROOM", selectedElement.id, selectedModel.id, GlobalManagement.Instance.username, GlobalVariables.Instance.GetRoomName(), hasAudio:isAudioRoom, acceptedUsers: acceptedUsers, onRoomCreated: onRoomCreated);
            });
        }
        
    }
    
    /// <summary>
    /// Actualiza la lista de salas disponibles desde el servidor
    /// </summary>
    public void UpdateRooms()
    {
        EnviarMensaje("LIST_ROOMS", user: GlobalManagement.Instance.username);
    }

    /// <summary>
    /// Envía un message al servidor de gestión de salas
    /// </summary>
    /// <param name="comando">Comando a enviar (ABRIR_SALA, CONSULTAR_SALAS, CERRAR_SALA)</param>
    /// <param name="chelementID">ID del elemento (opcional)</param>
    /// <param name="e3dmodelID">ID del modelo (opcional)</param>
    /// <param name="creator">Nombre del creator de la sala (opcional)</param>
    /// <param name="nombreSala">Nombre de la sala (opcional)</param>
    /// <param name="port">port de la sala (para CERRAR_SALA)</param>
    public void EnviarMensaje(string comando, int chelementID = -1, int e3dmodelID = -1, string creator = "", string nombreSala = "", int port = -1, bool hasAudio = false, List<string> acceptedUsers = null, string user = "", UnityAction<RespuestaServidor> onRoomCreated = null, string customHeaderKey = null, string customHeaderValue = null)
    {
        StartCoroutine(EnviarMensajeRoutine(comando, chelementID, e3dmodelID, creator, nombreSala, port, hasAudio,
            acceptedUsers, user, onRoomCreated, customHeaderKey, customHeaderValue));
    }
    
    private IEnumerator EnviarMensajeRoutine(string comando, int chelementID, int e3dmodelID, string creator, string nombreSala, int port, bool hasAudio, List<string> acceptedUsers, string user, UnityAction<RespuestaServidor> onRoomCreated, string customHeaderKey, string customHeaderValue)
    {
        // Validación de entrada
        if (string.IsNullOrEmpty(comando))
        {
            Debug.LogError("[RoomsManager] Comando vacío");
            yield break;
        }
        
        string safeToken = GlobalManagement.Instance != null ? GlobalManagement.Instance.token : "";
        string safeUser = string.IsNullOrEmpty(user) && GlobalManagement.Instance != null ? GlobalManagement.Instance.username : user;
        string serverIP = Application.isBatchMode ? "127.0.0.1" : NetworkServerConfiguration.Instance.GetServerIP();
        string apiPrefix = NetworkServerConfiguration.Instance.GetApiPrefix();
        string endpoint = GetEndpoint(comando);
        string url = "https://" + serverIP + apiPrefix + endpoint;
        
        UnityWebRequest request;
        string jsonBody = "";
        
        if (comando == "LIST_ROOMS")
        {
            url += $"?user={safeUser}";
            request = UnityWebRequest.Get(url);
        }
        else
        {
            var message = new message
            {
                creator = creator,
                roomName = nombreSala,
                chElementId = chelementID,
                e3dModelId = e3dmodelID,
                hasAudio = hasAudio,
                acceptedUsers = acceptedUsers,
                user = safeUser,
                port = port
            };
            jsonBody = JsonConvert.SerializeObject(message, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });

            if (comando.Equals("OPEN_ROOM"))
                RoomState.CurrentRoomCreator = creator;
            
            request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
        }
        
        if (!string.IsNullOrEmpty(customHeaderKey))
            request.SetRequestHeader(customHeaderKey, customHeaderValue);
        else
        {
            request.SetRequestHeader("Authorization", "Bearer " + safeToken);
        }
        
        request.timeout = (int)tiempoTimeoutSocket;
        
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            ProcesarRespuesta(comando, request.downloadHandler.text, serverIP, onRoomCreated);
        }
        else
        {
            string errorDetallado = request.downloadHandler != null ? request.downloadHandler.text : "Sin detalles";
            Debug.LogError($"[RoomsManager-HTTP] ERROR ({comando}): {request.error} | Detalles: {errorDetallado}");
            
            // Si hay error de red, mostramos message en la UI
            if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
            {
                StartCoroutine(ShowErrorMessage("Cannot connect to the server."));
            }
        }
    }

    /// <summary>
    /// Procesa la respuesta del servidor según el tipo de comando
    /// </summary>
    private void ProcesarRespuesta(string comando, string respuestaJson, string serverIP, UnityAction<RespuestaServidor> onRoomCreated = null)
    {
        try
        {
            try
            {
                string jsonLimpio = respuestaJson.TrimStart();
        
                // Comprobar si el servidor FastAPI ha devuelto un error HTTP estándar (422, 401, etc.)
                if (jsonLimpio.StartsWith("{") && jsonLimpio.Contains("\"detail\""))
                {
                    var errorAPI = JsonConvert.DeserializeObject<ErrorFastAPI>(jsonLimpio);
                    if (errorAPI != null && !string.IsNullOrEmpty(errorAPI.detail))
                    {
                        Debug.LogError($"[RoomsManager] Error de FastAPI: {errorAPI.detail}");
                        StartCoroutine(ShowErrorMessage(errorAPI.detail));
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    "[RoomsManager] La respuesta no es un message de error genérico, procediendo a procesar el comando específico. Excepción: " +
                    ex.Message);
            }

            switch (comando)
            {
                case "OPEN_ROOM":
                    ProcesarRespuestaAbrirSala(respuestaJson, onRoomCreated);
                    break;
                    
                case "LIST_ROOMS":
                    ProcesarRespuestaConsultarSalas(respuestaJson, serverIP);
                    break;
                    
                case "CLOSE_ROOM":
                    ProcesarRespuestaCerrarSala(respuestaJson);
                    break;
                
                case "ENTER_ROOM":
                    ProcesarRespuestaSolicitarUnirse(respuestaJson);
                    break;
                
                case "EXIT_ROOM":
                case "UPDATE_USERS":
                    break;
                    
                default:
                    Debug.LogWarning($"[RoomsManager] Comando no manejado: {comando}");
                    break;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[RoomsManager] Error procesando respuesta: {ex.Message}");
        }
    }

    /// <summary>
    /// Procesa la respuesta de ABRIR_SALA
    /// </summary>
    private void ProcesarRespuestaAbrirSala(string respuestaJson, UnityAction<RespuestaServidor> onRoomCreated = null)
    {
        try
        {
            var respuesta = JsonConvert.DeserializeObject<RespuestaServidor>(respuestaJson);
            
            if (respuesta == null)
            {
                Debug.LogError("[RoomsManager] Respuesta deserializada es nula");
                return;
            }

            if (respuesta.status == "error")
            {
                Debug.LogError($"[RoomsManager] Error al abrir sala: {respuesta.message}");
                StartCoroutine(ShowErrorMessage("Error creating the room."));
                return;
            }

            if (respuesta.status == "active")
            {
                RoomState.CurrentRoomPort = respuesta.port;
                if(onRoomCreated != null)
                {
                    onRoomCreated.Invoke(respuesta);
                }
            }
            else if (respuesta.status == "inactive")
                Debug.LogWarning($"[RoomsManager] Sala creada pero está inactiva: {respuesta.message}");
            else
                Debug.LogWarning($"[RoomsManager] Status desconocido: {respuesta.status}");
        }
        catch (JsonException ex)
        {
            Debug.LogError($"[RoomsManager] Error al deserializar respuesta ABRIR_SALA: {ex.Message}");
        }
    }

    /// <summary>
    /// Procesa la respuesta de CONSULTAR_SALAS
    /// </summary>
    private void ProcesarRespuestaConsultarSalas(string respuestaJson, string serverIP)
    {
        // Es un array, lista de salas
        if (respuestaJson == lastRoomsJson)
            return;
        
        lastRoomsJson = respuestaJson;
        
        var salas = JsonConvert.DeserializeObject<List<Servicio>>(respuestaJson);
        if (salas == null)
        {
            Debug.LogWarning("[RoomsManager] No hay salas disponibles");
            salas = new List<Servicio>();
        }
        
        // Actualizar UI si está disponible
        if (_roomManagerUIDocController != null)
        {
            _roomManagerUIDocController.ClearRooms();
            foreach (var sala in salas)
            {
                _roomManagerUIDocController.AddRoom(
                    sala.roomName,
                    sala.creator,
                    sala.port,
                    serverIP,
                    sala.chElementId,
                    sala.e3dModelId,
                    sala.hasAudio,
                    sala.acceptedUsers,
                    sala.actualPlayers
                );
            }
        }
        else
        {
            Debug.LogWarning("[RoomsManager] RoomManagerUIDocController no está asignado");
        }
    }

    /// <summary>
    /// Procesa la respuesta de CERRAR_SALA
    /// </summary>
    private void ProcesarRespuestaCerrarSala(string respuestaJson)
    {
        try
        {
            var respuesta = JsonConvert.DeserializeObject<RespuestaServidor>(respuestaJson);
            
            if (respuesta == null)
            {
                Debug.LogError("[RoomsManager] Respuesta deserializada es nula");
                return;
            }

            if (respuesta.status == "error")
            {
                Debug.LogError($"[RoomsManager] Error al cerrar sala: {respuesta.message}");
                return;
            }

            if (respuesta.status == "ok")
            {
                // Actualizar lista de salas
                Invoke("UpdateRooms", delayActualizacionSalas);
            }
        }
        catch (JsonException ex)
        {
            Debug.LogError($"[RoomsManager] Error al deserializar respuesta CERRAR_SALA: {ex.Message}");
        }
    }
    
    private void ProcesarRespuestaSolicitarUnirse(string respuestaJson)
    {
        var respuesta = JsonConvert.DeserializeObject<RespuestaServidor>(respuestaJson);
        
        if (respuesta != null && respuesta.status == "ok")
        {
            string ip = NetworkServerConfiguration.Instance.GetServerIP();
            StartClientDelayed(ip, (ushort)respuesta.port);
        }
        else
        {
            Debug.LogWarning($"[RoomsManager] Permiso denegado: {respuesta?.message}");
            StartCoroutine(ShowErrorMessage(respuesta?.message));
        }
    }

    public IEnumerator ShowErrorMessage(string customMsg = null)
    {
        _roomManagerUIDocController.ShowErrorMessage(customMsg);
        yield return new WaitForSeconds(3f);
        _roomManagerUIDocController.BackToSelectRoom();
    }

    /// <summary>
    /// Corutina que refresca la lista de salas periódicamente
    /// </summary>
    public IEnumerator RefreshRooms()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            UpdateRooms();
        }
    }
    
    private string GetEndpoint(string comando)
    {
        switch(comando)
        {
            case "OPEN_ROOM":
                return "/create";
            case "LIST_ROOMS":
                return "/list";
            case "CLOSE_ROOM":
                return "/close";
            case "ENTER_ROOM":
                return "/room/enter";
            case "EXIT_ROOM":
                return "/room/exit";
            case "UPDATE_USERS":
                return "/room/update";
            default:
                return "";
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            LeaveRoom();
    }

    private void OnApplicationQuit()
    {
        LeaveRoom();
    }
}