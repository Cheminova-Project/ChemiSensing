using System;
using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class ShutdownManager : MonoBehaviour
{
    public static string DisconnectReason = null;
    
    private void OnApplicationPause(bool isPaused)
    {
#if UNITY_ANDROID || UNITY_IOS
        if (isPaused && NetworkManager.Singleton != null)
        {
            DisconnectReason =
                "You were kicked out of the room due to a connection loss. Please re-enter the room by clicking 'Find room' or create a new room by clicking 'Create room'.";
            
            ExecuteExit(keepToken: true);
        }
#endif
    }

    private void OnApplicationQuit()
    {
        if (NetworkManager.Singleton != null)
        {
            if (RoomState.CurrentRoomPort != -1)
            {
                try
                {
                    string uname = GlobalManagement.Instance != null ? GlobalManagement.Instance.username : "Unknown";
                    string serverIP = NetworkServerConfiguration.Instance.GetServerIP();
                    string apiPrefix = NetworkServerConfiguration.Instance.GetApiPrefix();
                    string safeToken = GlobalManagement.Instance != null ? GlobalManagement.Instance.token : "";
                    
                    string url = "https://" + serverIP + apiPrefix + "/room/exit";
                    string jsonSalida = $"{{\"port\":{RoomState.CurrentRoomPort},\"user\":\"{uname}\"}}";

                    using (var client = new WebClient())
                    {
                        client.Headers[HttpRequestHeader.ContentType] = "application/json";
                        if (!string.IsNullOrEmpty(safeToken))
                            client.Headers[HttpRequestHeader.Authorization] = "Bearer " + safeToken;
                        
                        client.UploadString(url, "POST", jsonSalida);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[ShutdownManager] Error enviando salida síncrona: " + e.Message);
                }
            }
            
            PlayerPrefs.DeleteAll();
            NetworkManager.Singleton.Shutdown();
        }
    }

    private void ExecuteExit(bool keepToken)
    {
        // Seguimos usando la "balsa" para la limpieza de Netcode y el cambio de escena
        GameObject lifeRaft = new GameObject("Lifecycle_SceneCleaner_LifeRaft");
        var runner = lifeRaft.AddComponent<LifecycleSceneCleanerRunner>();
        runner.StartExitRoutine(keepToken);
    }
}

/// <summary>
/// Componente auxiliar que hace la limpieza asíncrona.
/// </summary>
public class LifecycleSceneCleanerRunner : MonoBehaviour
{
    public void StartExitRoutine(bool keepToken)
    {
        // Ya no necesitamos pasarle la referencia del manager
        StartCoroutine(ExitProcess(keepToken));
    }

    private IEnumerator ExitProcess(bool keepToken)
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
            
            NetworkManager.Singleton.Shutdown();
            
            yield return null; 

            if (NetworkManager.Singleton != null)
            {
                Destroy(NetworkManager.Singleton.gameObject);
            }
            
            foreach (GameObject go in ddolScene.GetRootGameObjects())
            {
                if (go == gameObject)
                    continue;
                
                if (keepToken && go.name == "Token")
                    continue;

                DestroyImmediate(go);
            }
        
            if (UIDocumentManager.Instance != null)
                DestroyImmediate(UIDocumentManager.Instance.gameObject);
        }

        SceneManager.LoadScene("MainScene");

        Destroy(gameObject);
    }
}