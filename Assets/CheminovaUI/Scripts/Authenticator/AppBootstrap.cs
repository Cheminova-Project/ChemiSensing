using UnityEngine;

public class AppBootstrap : MonoBehaviour
{
    void Awake()
    {
        if (!SingleInstance.Initialize())
        {
            UriForwarder.SendToExistingInstance(System.Environment.GetCommandLineArgs());
            Application.Quit();
        }
    }
}