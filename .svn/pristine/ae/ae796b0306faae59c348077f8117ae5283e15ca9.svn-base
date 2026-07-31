using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class TextureManager : NetworkBehaviour
{
    private const int MAX_DOWNLOADED_TEXTURES = 3;
    
    private MeshRenderer modelRenderer; // Renderer of the model to apply textures 

    public NetworkList<RemoteTexture> remoteTextures = new NetworkList<RemoteTexture>();
    private Dictionary<int, LocalTextureData> localTextureData = new();
    private Dictionary<int, float> lastAccessTime = new(); // Para tracking LRU
    
    private static float lastTextureMessageTime = 0f;
    private static ulong lastTextureChangerId = ulong.MaxValue;
    
    public UnityAction<RemoteTexture> onRemoteTexturesValueChanged;
    public UnityAction<int> onTextureStartedDownload;
    public UnityAction<int> onMemoryLimitReached; // Notificacion cuando se alcanza el limite
    
    private TextureManagerUI currentUI;
    
    public readonly NetworkVariable<ulong> textureToolLockOwner = new(ulong.MaxValue);
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!NetworkManager.Singleton.IsServer)
        {
            // Suscribirse al evento de creación de UI
            TextureManagerUI.OnTextureManagerUICreated += OnTextureManagerUICreated;
            InspectedObjectController.Instance.OnInspectedObjectLoaded += Init;
            InspectedObjectController.Instance.onThreeDInstanceDataDownloaded += GetTextureLayers;
        }
        else
            NetworkManager.Singleton.OnClientDisconnectCallback += OnServerClientDisconnect;
    }

    public int GetDownloadedTexturesCount()
    {
        int count = 0;
        foreach (var textureData in localTextureData.Values)
        {
            if (textureData.isDownloaded || textureData.isDownloading)
                count++;
        }
        return count;
    }

    /// <summary>
    /// Registra el acceso a una textura para el tracking LRU
    /// </summary>
    private void RecordTextureAccess(int textureID)
    {
        lastAccessTime[textureID] = Time.realtimeSinceStartup;
    }

    /// <summary>
    /// Encuentra la textura descargada menos recientemente usada que NO esté siendo mostrada
    /// </summary>
    private int FindLeastRecentlyUsedNotInUse()
    {
        int lruTextureID = -1;
        float oldestAccessTime = float.MaxValue;

        foreach (var kvp in lastAccessTime)
        {
            if (localTextureData.ContainsKey(kvp.Key) && localTextureData[kvp.Key].isDownloaded)
            {
                // No eliminar texturas visibles
                RemoteTexture remoteTexture = GetRemoteTextureByID(kvp.Key);
                if (remoteTexture.isVisible)
                    continue;

                if (kvp.Value < oldestAccessTime)
                {
                    oldestAccessTime = kvp.Value;
                    lruTextureID = kvp.Key;
                }
            }
        }

        return lruTextureID;
    }

    /// <summary>
    /// Intenta descargar una textura. Devuelve el estado de descarga:
    /// -1: Puede proceder normalmente
    /// -2: Límite de memoria alcanzado (descarga manual, requiere confirmación)
    /// >=0: ID de textura que fue automáticamente eliminada (descarga de red)
    /// </summary>
    private int TryDownloadTexture(int textureID, bool isManualDownload)
    {
        int currentDownloadedCount = GetDownloadedTexturesCount();

        if (currentDownloadedCount >= MAX_DOWNLOADED_TEXTURES)
        {
            int lruTextureID = FindLeastRecentlyUsedNotInUse();

            if (lruTextureID == -1)
            {
                // No hay textura no-visible para eliminar
                if (isManualDownload)
                {
                    Debug.LogWarning($"[TextureManager] Memory limit reached. All {MAX_DOWNLOADED_TEXTURES} textures are in use. Cannot download.");
                    onMemoryLimitReached?.Invoke(-1);
                    return -2;
                }
                return -1; // En descargas automáticas, simplemente no proceder
            }

            if (isManualDownload)
            {
                onMemoryLimitReached?.Invoke(lruTextureID);
                return -2; // Requiere confirmación
            }
            else
            {
                // Para descarga automática (otro cliente), automáticamente eliminar la menos usada
                RemoveTexture(lruTextureID);
                return lruTextureID;
            }
        }

        return -1; // Puede proceder
    }

    private void GetTextureLayers()
    {
        int id = GlobalVariables.Instance.GetSelected3DInstanceData().id;
        StartCoroutine(ThreeDInstanceDB.GetE3DInstanceTextureLayers(id, OnDownloadComplete));
    }

    /// <summary>
    /// Se llama cuando se crea una instancia de TextureManagerUI
    /// </summary>
    private void OnTextureManagerUICreated(TextureManagerUI textureUI)
    {
        currentUI = textureUI;

        // Si ya tienes texturas cargadas, añadirlas a la nueva UI
        RefreshUIWithExistingTextures();
    }


    /// <summary>
    /// Refresca la UI con texturas ya existentes
    /// </summary>
    private void RefreshUIWithExistingTextures()
    {
        if (currentUI == null)
            return;
        
        foreach (var texture in remoteTextures)
        {
            currentUI.AddTextureItem(texture);
        }
        

        // Importante: sincronizar el estado de todos los elementos después de añadirlos
        currentUI.RefreshAllUIStates();
    }
    
    public void SetTextureManagerUI(TextureManagerUI textureManagerUI)
    {
        // Método mantenido para compatibilidad, pero ya no se usa
        // La conexión ahora se hace automáticamente via eventos
    }
    
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!NetworkManager.Singleton.IsServer)
        {
            // Desuscribirse del evento de UI
            TextureManagerUI.OnTextureManagerUICreated -= OnTextureManagerUICreated;
            InspectedObjectController.Instance.OnInspectedObjectLoaded -= Init;
            InspectedObjectController.Instance.onThreeDInstanceDataDownloaded -= GetTextureLayers;
        }
        else if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnServerClientDisconnect;

        remoteTextures.Dispose();
    }

    private void HandleTextureClick(int textureID)
    {
        if (NetworkManager.Singleton.IsServer)
            return;
            
        RemoteTexture remoteTexture = GetRemoteTextureByID(textureID);
        if (remoteTexture.id == -1)
        {
            Debug.LogError($"Remote texture with ID {textureID} not found.");
            return;
        }

        if (!localTextureData[remoteTexture.id].isDownloaded)
        {
            Debug.LogError($"Texture with ID {remoteTexture.id} is not downloaded.");
            return;
        }

        ChangeVisibilityServerRpc(!remoteTexture.isVisible, remoteTexture.id);
    }

    private void ApplyTexture(RemoteTexture remoteTexture)
    {
        if (NetworkManager.Singleton.IsServer)
            return;
        if (modelRenderer == null)
        {
            Debug.LogError("Model renderer is not set.");
            return;
        }

        if (remoteTexture.isVisible)
        {
            if (!IsTextureDownloaded(remoteTexture.id))
            {
                // Descarga automática (otro cliente la hizo visible)
                StartDownload(remoteTexture, () =>
                {
                    SyncTextureVisualState();
                }, isManualDownload: false);
            }
            else
            {
                SyncTextureVisualState();
            }
        }
        else
        {
            RemoveTextureModel(remoteTexture);
        }
    }

    public void StartDownload(RemoteTexture remoteTexture, UnityAction onComplete = null, bool isManualDownload = true)
    {
        if (NetworkManager.Singleton.IsServer)
            return;
        DownloadTextureCoroutine(remoteTexture, onComplete, isManualDownload);
    }
    
    public void StartDownload(int remoteTextureID, UnityAction onComplete = null, bool isManualDownload = true)
    {
        if (NetworkManager.Singleton.IsServer)
            return;
        RemoteTexture remoteTexture = GetRemoteTextureByID(remoteTextureID);
        DownloadTextureCoroutine(remoteTexture, onComplete, isManualDownload);
    }

    public void DownloadTextureCoroutine(RemoteTexture remoteTexture, UnityAction onComplete = null, bool isManualDownload = true)
    {
        // Verificar límite de memoria ANTES de iniciar descarga
        int memoryCheckResult = TryDownloadTexture(remoteTexture.id, isManualDownload);
        
        if (memoryCheckResult == -2)
        {
            // Límite de memoria alcanzado en descarga manual
            Debug.LogWarning($"[TextureManager] Download blocked: Memory limit reached. User must remove a texture first.");
            return;
        }
        
        if (memoryCheckResult >= 0 && !isManualDownload)
        {
            // Textura automáticamente evicted, proceder con descarga
        }

        // Marcar como descargando en el estado local
        if (localTextureData.ContainsKey(remoteTexture.id))
        {
            localTextureData[remoteTexture.id].isDownloading = true;
        }

        onTextureStartedDownload?.Invoke(remoteTexture.id);
        //Debemos descargar una textura sabiendo el ID
        float startTime = Time.time;
        StartCoroutine(DownloadDB.GetDownloadByID(remoteTexture.id, (rawFile, success) =>
        {
            OnTextureDownloadCompleted(rawFile, success, remoteTexture.id, onComplete, isManualDownload);
        }));
    }
    
    private void RemoveTextureModel(RemoteTexture remoteTexture)
    {
        if (NetworkManager.Singleton.IsServer)
            return;
        Texture2D texture = localTextureData[remoteTexture.id].localTexture2D;
        
        switch (remoteTexture.textureType)
        {
            case TextureTypeFake.AO:
                var ao = modelRenderer.material.GetTexture("_OcclusionMap");
                if(texture == ao)
                    modelRenderer.material.SetTexture("_OcclusionMap", null);
                break;
            case TextureTypeFake.ALBEDO:
                //var albedo = modelRenderer.material.GetTexture("_BaseMap");
                var albedo = modelRenderer.material.GetTexture("_AlphaLayer");;
                if(texture == albedo)
                    //modelRenderer.material.SetTexture("_BaseMap", null);
                    modelRenderer.material.SetTexture("_AlphaLayer", null);
                break;
            case TextureTypeFake.NORMAL:
                var normal = modelRenderer.material.GetTexture("_BumpMap");
                if(texture == normal)
                    modelRenderer.material.SetTexture("_BumpMap", null);
                break;
        }
    }

    private void ApplyTextureModel(RemoteTexture remoteTexture)
    {
        if (NetworkManager.Singleton.IsServer)
            return;

        if (!remoteTexture.isVisible)
            return;
        
        Texture2D texture = localTextureData[remoteTexture.id].localTexture2D;
        if (texture == null)
        {
            Debug.LogError($"Texture with ID {remoteTexture.id} is not downloaded or is null.");
            return;
        }
        
        // Registrar acceso cuando se aplica la textura
        RecordTextureAccess(remoteTexture.id);
        
        switch (remoteTexture.textureType)
        {
            case TextureTypeFake.AO:
                modelRenderer.material.SetTexture("_OcclusionMap", texture);
                break;
            case TextureTypeFake.ALBEDO:
                //modelRenderer.material.SetTexture("_BaseMap", texture);
                modelRenderer.material.SetTexture("_AlphaLayer", texture);
                break;
            case TextureTypeFake.NORMAL:
                modelRenderer.material.SetTexture("_BumpMap", texture);
                break;
        }
    }

    private void ChangeVisibilityInServer(bool visibility, int id)
    {
        if(!NetworkManager.Singleton.IsServer)
        {
            Debug.LogError("ChangeVisibilityInServer should be called on the server. Use ChangeVisibility instead.");
            return;
        }
        int index = GetIndexByID(id);
        var remoteTexture = remoteTextures[index];
        
        if(visibility)
            HideTexturesTypeDependent(remoteTexture);
        
        remoteTexture.ChangeVisibility(visibility);
        remoteTextures[index] = remoteTexture;
    }
    
    [ServerRpc (RequireOwnership = false)] 
    private void ChangeVisibilityServerRpc(bool visibility, int id, ServerRpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        ChangeVisibilityInServer(visibility, id);
        
        if (visibility)
            NotifyTextureChangeClientRpc(senderId, id);
    }
    
    [ClientRpc]
    private void NotifyTextureChangeClientRpc(ulong senderId, int textureId)
    {
        if (NetworkManager.Singleton.LocalClientId == NetworkManager.ServerClientId)
            return;

        if (NetworkManager.Singleton.LocalClientId == senderId)
            return;

        if (Time.time - lastTextureMessageTime < 5f && lastTextureChangerId == senderId)
            return;

        lastTextureMessageTime = Time.time;
        lastTextureChangerId = senderId;

        string changerName = LocalRegistry.Instance != null ? LocalRegistry.Instance.GetClientUsername(senderId) : "A user";

        string textureName = "a new texture";
        int index = GetIndexByID(textureId);
        if (index >= 0 && index < remoteTextures.Count)
            textureName = remoteTextures[index].name.ToString();

        if (ToolMessageHandler.Instance != null)
            ToolMessageHandler.Instance.ShowMessage($"{changerName} has applied the texture '{textureName}'.", 3.5f, MessageType.Info);
    }
    
    /// <summary>
    /// Versión modificada de ToggleTextureVisibility para modo debug
    /// </summary>
    public void ToggleTextureVisibility(int textureID)
    {
        // Código original para modo multijugador
        if (NetworkManager.Singleton.IsServer)
            return;
            
        RemoteTexture remoteTexture = GetRemoteTextureByID(textureID);
        if (remoteTexture.id == -1)
        {
            Debug.LogError($"Remote texture with ID {textureID} not found.");
            return;
        }

        bool newVisibility = !remoteTexture.isVisible;
        ChangeVisibilityServerRpc(newVisibility, textureID);
    }
    
    private void HideTexturesTypeDependent(RemoteTexture remoteTexture)
    {
        //Revisaremos todas las texturas del tipo actual, y solo dejaremos activa la actual
        var currentType = remoteTexture.textureType;
        foreach (var texture in remoteTextures)
        {
            if(texture.textureType != currentType)
                continue; // Skip textures of different type
            if (texture.id != remoteTexture.id && texture.isVisible)
            {
                texture.ChangeVisibility(false);
                remoteTextures[GetIndexByID(texture.id)] = texture; // Update the texture in the list
            }
        }
    }
    
    
    private int GetIndexByID(int id)
    {
        for (var index = 0; index < remoteTextures.Count; index++)
        {
            if (remoteTextures[index].id == id)
            {
                return index;
            }
        }
        return -1; // Not found
    }
    
    public RemoteTexture GetRemoteTextureByID(int id)
    {
        // En modo multijugador normal
        if (NetworkManager.Singleton != null && remoteTextures != null)
        {
            int index = GetIndexByID(id);
            if (index >= 0 && index < remoteTextures.Count)
                return remoteTextures[index];
        }
        
        Debug.LogError($"Remote texture with ID {id} not found.");
        return default;
    }

    private void Init()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            modelRenderer = InspectedObjectController.Instance.GetInspectedObject()?.GetComponentInChildren<MeshRenderer>();

            //We can deduce that the textures involved in the gltf are already downloaded. We suppose: Albedo, Ambient occlusion, Normal map (This can change in the future, it depends on E3D implementation)
            if (modelRenderer == null)
            {
                Debug.LogError("No MeshRenderer found on the inspected object.");
                return;
            }

            Material modelMaterial = modelRenderer.material;
            //We clear all previous textures

            /*
            modelMaterial.SetTexture("_BaseMap", null);
            modelMaterial.SetTexture("_BumpMap", null);
            modelMaterial.SetTexture("_OcclusionMap", null);
            */


            InitializeClientUI();
            remoteTextures.OnListChanged += OnRemoteTexturesChanged;

            // Now, we check wich textures are now active
            foreach (var tex in remoteTextures)
            {
                if (tex.isVisible)
                {
                    onRemoteTexturesValueChanged?.Invoke(tex);
                    ApplyTexture(tex);
                }
            }
        }
    }


    private void OnDownloadComplete(TextureLayerResponse response, bool success)
    {
        if(!success)
            Debug.LogError("Failed to download texture layers from the server.");
        
        if(response.items.Count == 0)
        {
            Debug.LogWarning("No texture layers found for the selected E3D model.");
            return;
        }

        List<RemoteTexture> temporalTextures = new List<RemoteTexture>();

        foreach (var item in response.items)
        {
           foreach (var tex in item.textures)
            {
                if (tex.name.ToLower().Contains("albedo"))
                    continue;

                string textureType = string.IsNullOrEmpty(item.type) ? "Uncategorized" : item.type;                
                RemoteTexture remoteTexture = new RemoteTexture(tex.id, item.name, TextureTypeFake.ALBEDO, textureType, false, false);
                //tempraltexture tiene un campo tempraltexture.icon que es un int?, con el que queremos rellenar el previewImage del TextureLayerItem
                //Si icon es null, ponemos una textura por defecto. Para ello usaremos 
                DownloadDB.GetDownloadByID(item.icon ?? -1, (rawFile, iconSuccess) =>
                {
                    if (iconSuccess && rawFile != null)
                    {
                        Texture2D iconTexture = new Texture2D(2, 2);
                        iconTexture.LoadImage(rawFile.data);
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to download icon for texture ID {tex.id}. Using default icon.");
                    }
                });
                temporalTextures.Add(remoteTexture);
                break;
            }
        }
        AddRemoteTexturesServerRpc(temporalTextures.ToArray());
    }

    [ServerRpc (RequireOwnership = false)]
    public void AddRemoteTexturesServerRpc(RemoteTexture[] textures)
    {
        if(remoteTextures == null || remoteTextures.Count == 0)
        {
            remoteTextures.Clear();
            foreach (var tex in textures)
            {
                remoteTextures.Add(tex);
            }
        }
       
    }

    private void InitializeClientUI()
    {
        foreach (var tex in remoteTextures)
        {
            AddTextureLayerItem(tex);
        }
    }
    
    private void AddTextureLayerItem(RemoteTexture remoteTexture)
    {
        if (NetworkManager.Singleton.IsServer)
            return;
        
        LocalTextureData localTextureData = new LocalTextureData(remoteTexture);
        this.localTextureData[remoteTexture.id] = localTextureData;
        
        // Inicializar registro de acceso para LRU
        lastAccessTime[remoteTexture.id] = Time.realtimeSinceStartup;
        
        // Use event to notify UI
        currentUI?.AddTextureItem(remoteTexture);
    }
    
    private void OnRemoteTexturesChanged(NetworkListEvent<RemoteTexture> changeEvent)
    {
        if (NetworkManager.Singleton.IsServer)
            return;
        switch (changeEvent.Type)
        {
            case NetworkListEvent<RemoteTexture>.EventType.Add:
                AddTextureLayerItem(changeEvent.Value);
                break;

            case NetworkListEvent<RemoteTexture>.EventType.Remove:
                break;

            case NetworkListEvent<RemoteTexture>.EventType.Value:
                onRemoteTexturesValueChanged?.Invoke(changeEvent.Value);
                ApplyTexture(changeEvent.Value);
                break;

            case NetworkListEvent<RemoteTexture>.EventType.Insert:
                break;

            case NetworkListEvent<RemoteTexture>.EventType.Clear:
                break;
        }
    }

    
    public bool IsTextureDownloaded(int textureID)
    {
        return localTextureData.ContainsKey(textureID) && localTextureData[textureID].isDownloaded;
    }
    
    /// <summary>
    /// Obtiene si una textura está en proceso de descarga
    /// </summary>
    public bool IsTextureDownloading(int textureID)
    {
        return localTextureData.ContainsKey(textureID) && localTextureData[textureID].isDownloading;
    }
    
    /// <summary>
    /// Obtiene los datos locales completos de una textura
    /// </summary>
    public LocalTextureData GetLocalTextureData(int textureID)
    {
        return localTextureData.ContainsKey(textureID) ? localTextureData[textureID] : null;
    }

    public void SetTextureDownloaded(int textureID, Texture2D texture)
    {
        if (NetworkManager.Singleton.IsServer)
            return;
        if (localTextureData.ContainsKey(textureID))
        {
            localTextureData[textureID].localTexture2D = texture;
            localTextureData[textureID].isDownloaded = true;
            localTextureData[textureID].isDownloading = false; // Ya no está descargando
            
            // Registrar acceso para LRU
            RecordTextureAccess(textureID);
            
            // Notificar a la UI que la descarga ha completado
            RemoteTexture remoteTexture = GetRemoteTextureByID(textureID);
            onRemoteTexturesValueChanged?.Invoke(remoteTexture);
        }
        else
        {
            Debug.LogError($"Texture with ID {textureID} not found in local data.");
        }
    }
    
    public void RemoveTexture(int textureID)
    {
        if (NetworkManager.Singleton.IsServer)
            return;
        if (localTextureData.ContainsKey(textureID))
        {
            // Obtener la textura antes de removerla
            RemoteTexture remoteTexture = GetRemoteTextureByID(textureID);
            bool wasVisible = remoteTexture.isVisible;
            
            // Desaplicar la textura del modelo si está visible
            if (remoteTexture.isVisible)
            {
                RemoveTextureModel(remoteTexture);
            }
            
            // Limpiar los datos locales
            localTextureData[textureID].isDownloaded = false;
            localTextureData[textureID].isDownloading = false; // También limpiar el estado de descarga
            localTextureData[textureID].localTexture2D = null;
            
            // Cambiar visibilidad a false en el servidor
            ChangeVisibilityServerRpc(false, textureID);
            
            // Si la textura no estaba visible, el evento del servidor no se disparará
            // así que necesitamos actualizar la UI manualmente
            if (!wasVisible)
            {
                // Disparar evento manual para que la UI se actualice
                onRemoteTexturesValueChanged?.Invoke(remoteTexture);
            }
        }
        else
        {
            Debug.LogError($"Texture with ID {textureID} not found in local data.");
        }
    }
    
    #region Texture Settings
    
    public void ResetModel()
    {
        foreach (var tex in remoteTextures)
        {
            ChangeVisibilityServerRpc(tex.isDefault, tex.id);
        }
    }

    #endregion

    public void AddLocalData(int remoteTextureID, LocalTextureData localData)
    {
        localTextureData.Add(remoteTextureID, localData);
    }


    /// <summary>
    /// Completa la descarga de una textura de forma asíncrona
    /// </summary>
    private void OnTextureDownloadCompleted(RawFileDownload rawFileDownload, bool success, int remoteTextureID,
        UnityAction onComplete, bool isManualDownload = true)
    {
        if (success)
        {
            if(XRFade.Instance == null)
            {
                LoadImageAndSync(rawFileDownload, remoteTextureID, onComplete);
                return;
            }
            XRFade.Instance?.FadeOut(() => LoadImageAndSync(rawFileDownload, remoteTextureID, onComplete));
        }
        // Marcar como no descargando en caso de fallo
        if (localTextureData.ContainsKey(remoteTextureID))
        {
            localTextureData[remoteTextureID].isDownloading = false;
        }
    }

    private void LoadImageAndSync(RawFileDownload rawFileDownload, int remoteTextureID, UnityAction onComplete = null)
    {
        // Cargar la imagen de forma asíncrona
        Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGB24, mipChain: false);
        texture2D.LoadImage(rawFileDownload.data);
        texture2D.Apply();

        if (texture2D != null)
        {
            SetTextureDownloaded(remoteTextureID, texture2D);
            RecordTextureAccess(remoteTextureID);
            
            // ✅ Sincronizar el estado visual después de descargar
            SyncTextureVisualState();
            onComplete?.Invoke();
            XRFade.Instance?.FadeIn();
        }
        else
        {
            Debug.LogError($"[TextureManager] Failed to load image for texture {remoteTextureID}");
            if (localTextureData.ContainsKey(remoteTextureID))
            {
                localTextureData[remoteTextureID].isDownloading = false;
            }
        }
        return;
    }

    /// <summary>
    /// Sincroniza el estado visual de las texturas después de descargas múltiples.
    /// Verifica cuál debería estar visible y la aplica correctamente.
    /// </summary>
    private void SyncTextureVisualState()
    {
        if (modelRenderer == null) return;

        // Iterar sobre todas las texturas remotas y aplicar el estado correcto
        foreach (var remoteTexture in remoteTextures)
        {
            bool isDownloaded = IsTextureDownloaded(remoteTexture.id);
            
            if (remoteTexture.isVisible && isDownloaded)
            {
                // Si debería estar visible y está descargada, aplicarla
                ApplyTextureModel(remoteTexture);
            }
            else if (!remoteTexture.isVisible && isDownloaded)
            {
                // Si no debería estar visible pero está descargada, removerla
                RemoveTextureModel(remoteTexture);
            }
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void RequestTextureToolLockServerRpc(ulong clientId)
    {
        if (textureToolLockOwner.Value == ulong.MaxValue)
        {
            textureToolLockOwner.Value = clientId;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReleaseTextureToolLockServerRpc(ulong clientId)
    {
        if (textureToolLockOwner.Value == clientId)
        {
            textureToolLockOwner.Value = ulong.MaxValue;
        }
    }

    private void OnServerClientDisconnect(ulong clientId)
    {
        if (textureToolLockOwner.Value == clientId)
        {
            textureToolLockOwner.Value = ulong.MaxValue;
        }
    }
    
    private void TryReleaseMutexBeforeQuit()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsConnectedClient) 
            return;

        ulong myId = NetworkManager.Singleton.LocalClientId;

        if (textureToolLockOwner.Value == myId)
            ReleaseTextureToolLockServerRpc(myId);
    }
    
    private void OnApplicationQuit()
    {
        TryReleaseMutexBeforeQuit();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            TryReleaseMutexBeforeQuit();
    }
}

public class LocalTextureData
{
    public RemoteTexture remoteTexture;
    public Texture2D localTexture2D;
    public bool isDownloaded;
    public bool isDownloading; // Nuevo campo para el estado de descarga
    
    public LocalTextureData(Texture2D localTexture2D = null, bool isDownloaded = false)
    {
        this.remoteTexture = new RemoteTexture();
        this.localTexture2D = localTexture2D;
        this.isDownloaded = isDownloaded;
        this.isDownloading = false;
    }
    
    public LocalTextureData(RemoteTexture remoteTexture, Texture2D localTexture2D = null, bool isDownloaded = false)
    {
        this.remoteTexture = remoteTexture;
        this.localTexture2D = localTexture2D;
        this.isDownloaded = isDownloaded;
        this.isDownloading = false;
    }
}

public enum TextureTypeFake
{
    NORMAL,
    ALBEDO,
    AO
}

[Serializable]
public struct RemoteTexture : INetworkSerializable, IEquatable<RemoteTexture>
{
    public int id;
    public bool isVisible; // This field is used to determine if the texture is applied or not
    public bool isDefault;
    public TextureTypeFake textureType;
    public FixedString32Bytes layerType;
    public FixedString128Bytes name;

    public RemoteTexture(int id, string name, TextureTypeFake textureType, string layerType, bool isVisible = false, bool isDefault = false)
    {
        this.id = id;
        this.textureType = textureType;
        this.layerType = new FixedString32Bytes(layerType);
        this.isVisible = isVisible;
        this.isDefault = isDefault;
        this.name = new FixedString128Bytes(name);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref id);
        serializer.SerializeValue(ref isDefault);
        serializer.SerializeValue(ref textureType);
        serializer.SerializeValue(ref layerType);
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref isVisible);
    }

    public void ChangeVisibility(bool visibility)
    {
        isVisible = visibility;
    }
    public bool Equals(RemoteTexture other)
    {
        return id == other.id && isDefault == other.isDefault && textureType == other.textureType && layerType == other.layerType && name.Equals(other.name) && isVisible == other.isVisible;
    }

    public override bool Equals(object obj)
    {
        return obj is RemoteTexture other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(id, isDefault, (int) textureType, layerType, name, isVisible);
    }
}

[System.Serializable]
public class TextureListWrapper
{
    public List<RemoteTexture> textures;
}