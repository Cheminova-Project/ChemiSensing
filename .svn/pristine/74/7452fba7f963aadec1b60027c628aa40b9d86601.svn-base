using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Netcode;

public class AudioSettingsTool : ToolComponent
{
    [SerializeField] private string micDropdownName = "mic-dropdown";
    private DropdownField micDropdown;
    private NetworkAudio localPlayerAudio;

    protected override void OnToolActivatedInternal()
    {
        base.OnToolActivatedInternal();

        if (uIDocument == null)
        {
            Debug.LogError($"[{GetType().Name}] No UIDocument found in parent.");
            return;
        }

        var root = uIDocument.rootVisualElement;
        micDropdown = root.Q<DropdownField>(micDropdownName);

        if (micDropdown == null)
        {
            Debug.LogError($"[{GetType().Name}] Could not find one or both Dropdowns in UXML. Check names.");
            return;
        }

        FindLocalPlayerAndInitialize();
    }

    private void FindLocalPlayerAndInitialize()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient)
        {
            Debug.LogWarning($"[{GetType().Name}] NetworkManager not ready or not connected.");
            return;
        }

        var localPlayerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        
        if (localPlayerObj != null)
        {
            localPlayerAudio = localPlayerObj.GetComponent<NetworkAudio>();
            if (localPlayerAudio != null)
            {
                InitializeMicDropdown();
            }
            else
            {
                Debug.LogError($"[{GetType().Name}] Local player does not have NetworkAudio component.");
            }
        }
        else
        {
            Debug.LogWarning($"[{GetType().Name}] Local player object not found yet.");
        }
    }

    private void InitializeMicDropdown()
    {
        micDropdown.UnregisterValueChangedCallback(OnMicChanged);

        List<string> deviceNames = localPlayerAudio.GetMicrophoneNames();

        if (deviceNames.Count > 0)
        {
            micDropdown.choices = deviceNames;

            string currentMicName = localPlayerAudio.CurrentDeviceName;
            
            int currentIndex = -1;

            if (!string.IsNullOrEmpty(currentMicName))
            {
                currentIndex = deviceNames.FindIndex(name => name.Trim() == currentMicName.Trim());
            }

            if (currentIndex >= 0)
            {
                micDropdown.index = currentIndex; 
                micDropdown.SetValueWithoutNotify(deviceNames[currentIndex]);
            }
            else
            {
                micDropdown.index = 0;
                micDropdown.SetValueWithoutNotify(deviceNames[0]);
                
                if(!string.IsNullOrEmpty(currentMicName))
                    Debug.LogWarning($"[UI] El micro activo '{currentMicName}' no se encontró en la lista. Se seleccionó el primero.");
            }

            micDropdown.RegisterValueChangedCallback(OnMicChanged);
        }
        else
        {
            micDropdown.choices = new List<string> { "No Mic Found" };
            micDropdown.SetEnabled(false);
        }
    }

    private void OnMicChanged(ChangeEvent<string> evt)
    {
        string selectedName = evt.newValue;
        
        if (localPlayerAudio != null)
            localPlayerAudio.SetMicrophone(selectedName);
    }
}