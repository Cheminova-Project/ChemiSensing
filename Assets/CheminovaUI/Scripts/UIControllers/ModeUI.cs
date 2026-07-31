using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Controlador de UI para la selección de modo (crear o unirse a sala).
/// Gestiona los botones para cambiar de contexto en la aplicación.
/// </summary>
public class ModeUI : BaseUI
{
    /// <summary>
    /// Nombre del botón para crear sala en el árbol visual.
    /// </summary>
    private string createRoomButtonName = "create-room";
    /// <summary>
    /// Nombre del botón para unirse a sala en el árbol visual.
    /// </summary>
    private string joinRoomButtonName = "join-room";
    /// <summary>
    /// Nombre del botón para salir de la aplicación.
    /// </summary>
    private string exitButtonName = "exit-button";

    /// <summary>
    /// Referencia al botón de crear sala.
    /// </summary>
    private Button createRoomButton;
    /// <summary>
    /// Referencia al botón de unirse a sala.
    /// </summary>
    private Button joinRoomButton;
    /// <summary>
    /// Referencia al botón de salir de la aplicación.
    /// </summary>
    private Button exitButton;
    /// <summary>
    /// Escena a la que se navega para listar salas.
    /// </summary>
    public SceneField roomListingScene;

    /// <summary>
    /// Inicializa la UI y configura los eventos de los botones.
    /// </summary>
    protected override void InitializeUI()
    {
        createRoomButton = rootElement.Q<Button>(createRoomButtonName);
        joinRoomButton = rootElement.Q<Button>(joinRoomButtonName);
        exitButton = rootElement.Q<Button>(exitButtonName);

        if (createRoomButton != null)
            createRoomButton.clicked += OnCreateRoomClicked;
        else
            Debug.LogError($"[ModeUI] Could not find button with name: {createRoomButtonName}");

        if (joinRoomButton != null)
            joinRoomButton.clicked += OnJoinRoomClicked;
        else
            Debug.LogError($"[ModeUI] Could not find button with name: {joinRoomButtonName}");

        if (exitButton != null)
            exitButton.clicked += OnExitButtonClicked;
        else
            Debug.LogError($"[ModeUI] Could not find button with name: {exitButtonName}");
    }

    /// <summary>
    /// Evento al hacer clic en el botón de crear sala.
    /// Cambia el contexto a la lista de elementos culturales.
    /// </summary>
    void OnCreateRoomClicked()
    {
        UIDocumentManager.Instance.SwitchContext("chElementList");
    }

    /// <summary>
    /// Evento al hacer clic en el botón de unirse a sala.
    /// Cambia el contexto a la gestión de salas.
    /// </summary>
    void OnJoinRoomClicked()
    {
        UIDocumentManager.Instance.SwitchContext("room-management");
    }

    /// <summary>
    /// Evento al hacer clic en el botón de salir de la aplicación.
    /// </summary>
    void OnExitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}