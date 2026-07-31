using System;
using Unity.Multiplayer;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Punto de entrada principal para la aplicación de red.
/// Inicializa y gestiona el ciclo de vida de la red y los sistemas principales.
/// </summary>
/// <remarks>
/// <para>
/// This is the application's entry point, where the configuration is read and the application is initialized
/// accordingly. This also keeps references to systems that must persist throughout the application's lifecycle.
/// </para>
/// </remarks>
[MultiplayerRoleRestricted]
public class ApplicationEntryPoint : MonoBehaviour
{
    const string k_DefaultServerListenAddress = "0.0.0.0";

    /// <summary>
    /// Instancia única de <see cref="ApplicationEntryPoint"/>.
    /// </summary>
    public static ApplicationEntryPoint Singleton { get; private set; }

#if UNITY_EDITOR
    public static bool s_AreTestsRunning = false;
    public bool AreTestsRunning => s_AreTestsRunning;
#endif


    [SerializeField]
    ConnectionManager m_ConnectionManager;
    /// <summary>
    /// Gestor de conexión de la aplicación.
    /// </summary>
    public ConnectionManager ConnectionManager => m_ConnectionManager;

    [SerializeField]
    internal int MinPlayers = 2;
    [SerializeField]
    internal int MaxPlayers = 2;
    [SerializeField]
    bool m_AutoconnectIfClient = false;
    [SerializeField]
    private SceneField sceneToLoad;
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (Singleton is null)
        {
            Singleton = this;
        }
        m_ConnectionManager.EventManager.AddListener<ConnectionEvent>(OnConnectionEvent);
    }

    void OnDestroy()
    {
        m_ConnectionManager.EventManager.RemoveListener<ConnectionEvent>(OnConnectionEvent);

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }

    /// <summary>
    /// Método llamado al iniciar la aplicación.
    /// Inicializa la lógica de red según la configuración.
    /// </summary>
    [RuntimeInitializeOnLoadMethod]
    static void OnApplicationStarted()
    {
        if (!Singleton) //this happens during PlayMode tests
        {
            Debug.LogError("ApplicationEntryPoint not found in the scene. Please add it to the scene before running tests or starting the application. " +
                           "This component is responsible for initializing the application and managing network connections. " +
                           "It should be present in the first scene loaded by the application.");
            return;
        }
        Singleton.InitializeNetworkLogic(); //note: this is the entry point for all autoconnected instances (including standalone servers)
    }
    

    /// <summary>
    /// Initializes the application's network-related behaviour according to the configuration. Servers load the main
    /// game scene and automatically start. Clients load the metagame scene and, if autonnect is set to true, attempt
    /// to connect to a server automatically based on the IP and port passed through the configuration or the command
    /// line arguments.
    /// </summary>
    void InitializeNetworkLogic()
    {
        var commandLineArgumentsParser = new CommandLineArgumentsParser();
        ushort listeningPort = (ushort) commandLineArgumentsParser.Port;
        string listeningIp = commandLineArgumentsParser.ListenIp;
        //Debug.Log($"Parsed command line arguments: Role={MultiplayerRolesManager.ActiveMultiplayerRoleMask}, IP={listeningIp}, Port={listeningPort}, TargetFramerate={commandLineArgumentsParser.TargetFramerate}");
        switch (MultiplayerRolesManager.ActiveMultiplayerRoleMask)
        {
            case MultiplayerRoleFlags.Server:
                //lock framerate on dedicated servers
                Application.targetFrameRate = commandLineArgumentsParser.TargetFramerate;
                QualitySettings.vSyncCount = 0;
                if(m_ConnectionManager == null)
                    Debug.LogError("m_ConnectionManager is null");
                m_ConnectionManager.StartServerIP(listeningIp, listeningPort);
                break;
            case MultiplayerRoleFlags.Client:
            {
                break;
            }
            case MultiplayerRoleFlags.ClientAndServer:
                throw new ArgumentOutOfRangeException("MultiplayerRole", "ClientAndServer is an invalid multiplayer role in this sample. Please select the Client or Server role.");
        }
    }

    void OnConnectionEvent(ConnectionEvent evt)
    {
        if (MultiplayerRolesManager.ActiveMultiplayerRoleMask == MultiplayerRoleFlags.Server)
        {
            switch (evt.status)
            {
                case ConnectStatus.GenericDisconnect:
                case ConnectStatus.ServerEndedSession:
                case ConnectStatus.StartServerFailed:
                    Debug.LogError($"Server ended session with status: {evt.status}");
                    Quit();
                    break;
                case ConnectStatus.Success:
                    // If server successfully starts, load game scene
                    string sceneName = System.IO.Path.GetFileNameWithoutExtension(sceneToLoad.ScenePath);
                    if(sceneName == null || sceneName.Length == 0)
                    {
                        Debug.LogError("Scene name is empty or null. Please check the sceneToLoad field.");
                        return;
                    }
                    NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
                    break;
            }
        }
        else
        {
            switch (evt.status)
            {
                case ConnectStatus.GenericDisconnect:
                case ConnectStatus.UserRequestedDisconnect:
                case ConnectStatus.ServerEndedSession:
                    break;
                case ConnectStatus.Success:
                    break;
            }
        }
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

[System.Serializable]
public class SceneField
{
    [SerializeField]
    private string scenePath = "";

    public string ScenePath => scenePath;
    
#if UNITY_EDITOR
    public void SetScene(UnityEditor.SceneAsset sceneAsset)
    {
        scenePath = UnityEditor.AssetDatabase.GetAssetPath(sceneAsset);
    }

    public UnityEditor.SceneAsset GetSceneAsset()
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.SceneAsset>(scenePath);
    }
#endif
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SceneField))]
public class SceneFieldPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty scenePathProp = property.FindPropertyRelative("scenePath");

        EditorGUI.BeginProperty(position, label, property);

        Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, position.height);
        Rect fieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, position.height);

        EditorGUI.LabelField(labelRect, label);

        SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePathProp.stringValue);
        SceneAsset newScene = (SceneAsset)EditorGUI.ObjectField(fieldRect, sceneAsset, typeof(SceneAsset), false);

        if (newScene != sceneAsset)
        {
            string path = AssetDatabase.GetAssetPath(newScene);
            scenePathProp.stringValue = path;
        }

        EditorGUI.EndProperty();
    }
}
#endif