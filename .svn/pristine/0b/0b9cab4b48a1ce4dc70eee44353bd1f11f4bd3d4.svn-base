using System.Linq;

using UnityEngine;
using UnityEngine.XR.Management;

public class MultiplayerHelper : MonoBehaviour
{
    #if UNITY_EDITOR
    void Awake()
    {
        bool isNotXRClient = Unity.Multiplayer.PlayMode.CurrentPlayer.ReadOnlyTags().Contains("Test_PC") || Unity.Multiplayer.PlayMode.CurrentPlayer.ReadOnlyTags().Contains("Test_Mobile");
        if (isNotXRClient)
        {
            DisableXR();
        }
    }
    
      
    private void DisableXR()
    {
        // Desactivar XR en tiempo de ejecución
        var xrManager = XRGeneralSettings.Instance?.Manager;
        if (xrManager != null)
        {
            xrManager.DeinitializeLoader();
            xrManager.StopSubsystems();
            Debug.Log("XR desactivado.");
        }
    }
    #endif
}