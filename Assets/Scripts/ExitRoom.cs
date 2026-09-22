using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEngine.UIElements;
using UnityEngine.Networking;

public class ExitRoom : ToolComponent
{
    [SerializeField] private VisualTreeAsset exitPanelTemplate;
    [SerializeField] private string popupContainerName = "exit_popup_container";
    [SerializeField] private string btnCloseRoomName = "btn_close_room";
    [SerializeField] private string btnLeaveOpenName = "btn_leave_open";
    [SerializeField] private string txtCountdownName = "txt_countdown_warning";
    
    private VisualElement exitPopup;
    private Button btnCloseRoom;
    private Button btnLeaveOpen;
    private Label txtCountdown;
    private bool isProcessing = false;
    
    protected override void OnToolActivatedInternal()
    {
        base.OnToolActivatedInternal();

        if (isProcessing)
            return;
        
        if (IsRoomCreator())
            ShowCreatorWindow();
        else
            NormalExit();
    }
    
    private bool IsRoomCreator()
    {
        string currentUser = GlobalManagement.Instance != null ? GlobalManagement.Instance.username : "";
        string roomCreator = RoomState.CurrentRoomCreator != null ? RoomState.CurrentRoomCreator : ""; 
        
        return currentUser == roomCreator && !string.IsNullOrEmpty(currentUser);
    }
    
    private void ShowCreatorWindow()
    {
        if (uIDocument != null && exitPanelTemplate != null)
        {
            VisualElement root = uIDocument.rootVisualElement;

            if (root.Q(popupContainerName) != null)
                return;

            exitPopup = exitPanelTemplate.CloneTree();
            exitPopup.name = popupContainerName;
            exitPopup.style.position = Position.Absolute;
            exitPopup.style.width = Length.Percent(100);
            exitPopup.style.height = Length.Percent(100);
            root.Add(exitPopup);

            btnCloseRoom = exitPopup.Q<Button>(btnCloseRoomName);
            btnLeaveOpen = exitPopup.Q<Button>(btnLeaveOpenName);
            txtCountdown = exitPopup.Q<Label>(txtCountdownName);

            if (btnCloseRoom != null && btnLeaveOpen != null)
            {
                btnCloseRoom.clicked += OnCloseRoomClicked;
                btnLeaveOpen.clicked += OnLeaveOpenClicked;
            }
            else
            {
                Debug.LogWarning("[ExitRoom] Botones no encontrados en la plantilla. Forzando salida normal.");
                NormalExit();
            }
        }
        else
        {
            Debug.LogWarning("[ExitRoom] UXML o UIDocument no asignado. Forzando salida normal.");
            NormalExit();
        }
    }

    private void OnCloseRoomClicked()
    {
        if (!isProcessing)
            StartCoroutine(CloseRoomRoutine());
    }

    private void OnLeaveOpenClicked()
    {
        if (!isProcessing)
            NormalExit();
    }

    private IEnumerator CloseRoomRoutine()
    {
        isProcessing = true;
        
        if(btnCloseRoom != null)
            btnCloseRoom.style.display = DisplayStyle.None;
        
        if(btnLeaveOpen != null)
            btnLeaveOpen.style.display = DisplayStyle.None;

        var localPlayerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (localPlayerObj != null)
        {
            var localController = localPlayerObj.GetComponent<UserControllerPlayer>();
            if (localController != null)
            {
                localController.SendRoomCloseWarning();
            }
        }

        for (int i = 3; i > 0; i--)
        {
            if (txtCountdown != null)
                txtCountdown.text = $"Closing room in {i}...";
            
            yield return new WaitForSeconds(1f);
        }

        if (RoomsManager.Instance != null && RoomState.CurrentRoomPort != -1)
            RoomsManager.Instance.EnviarMensaje("CLOSE_ROOM", port: RoomState.CurrentRoomPort);

        NormalExit();
    }

    private void NormalExit()
    {
        isProcessing = true;
        
        if (exitPopup != null)
            exitPopup.style.display = DisplayStyle.None;

        GameObject lifeRaft = new GameObject("SceneCleaner_LifeRaft");
        var runner = lifeRaft.AddComponent<SceneCleanerRunner>();
        runner.StartExitRoutine();
    }
}

/// <summary>
/// Pequeño componente auxiliar que sobrevive al Shutdown de Netcode
/// para poder cargar la siguiente escena.
/// </summary>
public class SceneCleanerRunner : MonoBehaviour
{
    public void StartExitRoutine()
    {
        StartCoroutine(ExitProcess());
    }

    private IEnumerator ExitProcess()
    {
        if (GlobalVariables.Instance != null)
        {
            GlobalVariables.Instance.SetSelectedCHElementData(null);
            GlobalVariables.Instance.SetSelectedE3DModelData(null);
        }

        if (NetworkManager.Singleton != null)
        {
            GameObject temp = new GameObject("TempDDOLDetector");
            DontDestroyOnLoad(temp);
            Scene ddolScene = temp.scene;
            DestroyImmediate(temp);
            
            NetworkAudio.LocalGLTFLoaded = false; 

            if (RoomsManager.Instance != null)
            {
                RoomsManager.Instance.LeaveRoom();
            }
            else
            {
                if (RoomState.CurrentRoomPort != -1)
                {
                    string uname = GlobalManagement.Instance != null ? GlobalManagement.Instance.username : "Unknown";
                    string serverIP = NetworkServerConfiguration.Instance.GetServerIP();
                    string apiPrefix = NetworkServerConfiguration.Instance.GetApiPrefix();
                    string safeToken = GlobalManagement.Instance != null ? GlobalManagement.Instance.token : "";
                    string url = "https://" + serverIP + apiPrefix + "/room/exit";
                    string jsonSalida = $"{{\"port\":{RoomState.CurrentRoomPort},\"user\":\"{uname}\"}}";
                    
                    using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
                    {
                        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonSalida);
                        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                        request.downloadHandler = new DownloadHandlerBuffer();
                        request.SetRequestHeader("Content-Type", "application/json");
                        request.SetRequestHeader("Authorization", "Bearer " + safeToken);
                        
                        yield return request.SendWebRequest();

                        if (request.result != UnityWebRequest.Result.Success)
                            Debug.LogError($"[SceneCleaner] Error enviando EXIT_ROOM de emergencia: {request.error}");
                    }
                    
                    RoomState.CurrentRoomPort = -1; // Limpiamos el estado
                }

                NetworkManager.Singleton.Shutdown();
            }

            yield return new WaitWhile(() => NetworkManager.Singleton != null && NetworkManager.Singleton.ShutdownInProgress);

            if (NetworkManager.Singleton != null)
                Destroy(NetworkManager.Singleton.gameObject);
            
            foreach (GameObject go in ddolScene.GetRootGameObjects())
            {
                if (go.name == "Token")
                    continue;

                DestroyImmediate(go);
            }
        
            if (UIDocumentManager.Instance != null)
                DestroyImmediate(UIDocumentManager.Instance.gameObject);
        }

        SceneManager.LoadScene("MainScene");
    }
}