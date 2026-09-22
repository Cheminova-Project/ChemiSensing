using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Herramienta que gestiona el láser del usuario en la escena.
/// Permite activar y desactivar el láser para interacción y selección de objetos.
/// </summary>
public class LaserTool : ToolComponent
{
    /// <summary>
    /// Raycaster para pantalla.
    /// </summary>
    private ScreenRaycaster screenRaycaster;
    
    /// <summary>
    /// Raycaster para VR.
    /// </summary>
    private VRRaycaster vrRaycaster;

    /// <summary>
    /// Referencia guardada para poder desuscribirnos del evento del color.
    /// </summary>
    private NetworkPlayerName currentNetworkName;
    
    private Transform inspectedObject;
    private Camera raycastCamera;
    private bool isLaserCurrentlyVisible = true;

    /// <summary>
    /// Se llama al habilitar la herramienta. Activa el láser y muestra mensajes informativos.
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();

        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening) return;
        if (LocalRegistry.Instance == null) return;

        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj == null) netObj = GetComponentInParent<NetworkObject>();

        if (netObj != null && !netObj.IsSpawned) return;

        ulong ownerId = netObj != null ? netObj.OwnerClientId : NetworkManager.Singleton.LocalClientId;
        GameObject userGameobject = LocalRegistry.Instance.GetPlayerGameObject(ownerId);
        
        if (userGameobject == null)
        {
            Debug.LogWarning("[LaserTool] El GameObject del jugador aún no se ha encontrado.");
            return;
        }

        screenRaycaster = userGameobject.GetComponentInChildren<ScreenRaycaster>();
        vrRaycaster = userGameobject.GetComponentInChildren<VRRaycaster>();

        if(screenRaycaster == null && vrRaycaster == null)
        {
            Debug.LogWarning("[LaserTool] No se encontró ScreenRaycaster o VRRaycaster en el jugador.");
            return;
        }
        
        currentNetworkName = userGameobject.GetComponentInChildren<NetworkPlayerName>();
        if (currentNetworkName == null)
            currentNetworkName = userGameobject.GetComponentInParent<NetworkPlayerName>();

        Color userColor = Color.red;
        
        if (currentNetworkName != null)
        {
            userColor = currentNetworkName.playerColor.Value;
            currentNetworkName.playerColor.OnValueChanged += OnPlayerColorChanged;
        }
        else
        {
            Debug.LogWarning("[LaserTool] No se encontró NetworkPlayerName en el jugador.");
        }
        
        ApplyLaserColor(userColor);
        raycastCamera = Camera.main;
        isLaserCurrentlyVisible = true;

        if(vrRaycaster != null)
        {
            vrRaycaster.EnableLaser(true);
            ToolMessageHandler.Instance.ShowMessage("Your laser pointer is being shown to others", 4f, MessageType.Info);
            return;
        }

        if (screenRaycaster != null)
        {
            screenRaycaster.SelectInspectedObject();
            screenRaycaster.EnableLaser(true);
            ToolMessageHandler.Instance.ShowMessage("Your laser pointer is being shown to others", 4f, MessageType.Info);
        }
    }

    /// <summary>
    /// Se llama al deshabilitar la herramienta. Desactiva el láser y limpia referencias.
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        
        if (currentNetworkName != null)
        {
            currentNetworkName.playerColor.OnValueChanged -= OnPlayerColorChanged;
        }

        if (vrRaycaster != null) vrRaycaster.EnableLaser(false);
        if (screenRaycaster != null) screenRaycaster.EnableLaser(false);
    }

    /// <summary>
    /// Evento que se dispara automáticamente si el color del jugador cambia en la red.
    /// </summary>
    private void OnPlayerColorChanged(Color previousValue, Color newValue)
    {
        ApplyLaserColor(newValue);
    }

    /// <summary>
    /// Función auxiliar para aplicar el color a los raycasters que existan.
    /// </summary>
    private void ApplyLaserColor(Color color)
    {
        if (vrRaycaster != null) vrRaycaster.SetLaserColor(color);
        if (screenRaycaster != null) screenRaycaster.SetLaserColor(color);
    }
}