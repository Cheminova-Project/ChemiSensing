using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements; 

public class GlobalCall : ToolComponent
{
    private string buttonName = "call_users"; 
    private Toggle myToggle; 
    private bool isProcessing = false; 

    protected override void OnToolActivatedInternal()
    {
        base.OnToolActivatedInternal();

        if (uIDocument != null)
        {
            myToggle = uIDocument.rootVisualElement.Q<Toggle>(buttonName);

            if (myToggle != null)
            {
                myToggle.UnregisterValueChangedCallback(OnToggleValueChanged);
                myToggle.focusable = false;
                myToggle.RegisterValueChangedCallback(OnToggleValueChanged);
                if (myToggle.value == true && !isProcessing)
                {
                    StartCoroutine(ExecuteInvitationProcess());
                }
            }
            else
            {
                Debug.LogWarning($"[GlobalCall] No se encontró el Toggle: {buttonName}");
            }
        }
    }

    protected override void OnToolDeactivatedInternal()
    {
        base.OnToolDeactivatedInternal();
        
        if (myToggle != null)
        {
            myToggle.UnregisterValueChangedCallback(OnToggleValueChanged);
            myToggle = null;
        }
    }

    private void OnToggleValueChanged(ChangeEvent<bool> evt)
    {
        if (evt.newValue && !isProcessing)
            StartCoroutine(ExecuteInvitationProcess());
    }

    private IEnumerator ExecuteInvitationProcess()
    {
        isProcessing = true;
        
        if (myToggle != null)
            myToggle.SetEnabled(false);

        var localPlayerObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (localPlayerObj != null)
        {
            var localController = localPlayerObj.GetComponent<UserControllerPlayer>();
            if (localController != null)
                localController.SendGlobalTeleportInvitation();
        }

        yield return new WaitForSeconds(0.5f); 

        if (myToggle != null)
        {
            myToggle.SetEnabled(true);
            myToggle.SetValueWithoutNotify(false);
        }
        
        isProcessing = false;
        Destroy(gameObject);
    }
}