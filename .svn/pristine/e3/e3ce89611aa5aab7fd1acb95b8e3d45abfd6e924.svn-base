using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using VInspector;

public class SceneHelper : MonoBehaviour
{
    [Header("Scene Loading Settings")]
    [SerializeField]
    private bool loadOtherSceneIfClient = true;
    
    [SerializeField]
    [ShowIf("loadOtherSceneIfClient", true)]
    private SceneField sceneToLoadIfClient;
    [EndIf]
    
    [SerializeField]
    private bool loadOtherSceneIfServer = false;

    [SerializeField]
    [ShowIf("loadOtherSceneIfServer", true)]
    private SceneField sceneToLoadIfServer;
    [EndIf]
    
    private void Start()
    {
        StartCoroutine("ChangeSceneTimer", 3f);
        
        //Aquí quiero imprimir a modo de debug todas las escenas añadidas a los build settings
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        }
    }
    
    private IEnumerator ChangeSceneTimer(float delay)
    {
        yield return new WaitForSeconds(delay);
        ChangeToScene();
    }

    public void ChangeToScene()
    {
        string sceneName = "";
        
        if(NetworkManager.Singleton == null)
            Debug.LogError("NetworkManager.Singleton is null. Ensure it is initialized before calling ChangeToScene.");

        if (MultiplayerRolesManager.ActiveMultiplayerRoleMask == MultiplayerRoleFlags.Client && loadOtherSceneIfClient)
        {
            if(sceneToLoadIfClient == null)
            {
                Debug.LogError("Scene null. Please check the sceneToLoad field.");
                return;
            }
            sceneName = System.IO.Path.GetFileNameWithoutExtension(sceneToLoadIfClient.ScenePath);
            if(sceneName == null || sceneName.Length == 0)
            {
                Debug.LogError("Scene name is empty or null. Please check the sceneToLoad field.");
                return;
            }
        }
        else if (MultiplayerRolesManager.ActiveMultiplayerRoleMask == MultiplayerRoleFlags.Server  && loadOtherSceneIfServer)
        {
            if(sceneToLoadIfServer == null)
            {
                Debug.LogError("Scene null. Please check the sceneToLoad field.");
                return;
            }
            sceneName = System.IO.Path.GetFileNameWithoutExtension(sceneToLoadIfServer.ScenePath);
            if(sceneName == null || sceneName.Length == 0)
            {
                Debug.LogError("Scene name is empty or null. Please check the sceneToLoad field.");
                return;
            }
        }
        
        if(sceneName.Length != 0)
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
