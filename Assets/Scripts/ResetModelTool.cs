using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class ResetModelTool : ToolComponent
{
    [SerializeField] private string toolId = "reset_model"; 

    private Toggle myToggle; 
    private bool isProcessing;

    protected override void OnToolActivatedInternal()
    {
        base.OnToolActivatedInternal();

        if (uIDocument != null)
        {
            myToggle = uIDocument.rootVisualElement.Q<Toggle>(toolId);

            if (myToggle != null)
            {
                myToggle.UnregisterValueChangedCallback(OnToggleValueChanged);
                myToggle.focusable = false;
                myToggle.RegisterValueChangedCallback(OnToggleValueChanged);
                
                if (myToggle.value && !isProcessing)
                    StartCoroutine(ExecuteResetProcess());
            }
            else
            {
                Debug.LogWarning($"[ResetModelTool] No se encontró el Toggle con ID: {toolId}");
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
        if (evt.newValue == true && !isProcessing)
        {
            StartCoroutine(ExecuteResetProcess());
        }
    }

    private IEnumerator ExecuteResetProcess()
    {
        isProcessing = true;
        
        if (myToggle != null) myToggle.SetEnabled(false);

        Transform modelToReset = GetInspectedObject();

        if (modelToReset != null)
        {
            modelToReset.localPosition = Vector3.zero;
            modelToReset.localRotation = Quaternion.identity;
            modelToReset.localScale = Vector3.one;

            if (ToolMessageHandler.Instance != null)
            {
                ToolMessageHandler.Instance.ShowMessage("Model position and scale reset.", 3f, MessageType.Info);
            }
        }
        else
        {
            Debug.LogWarning("[ResetModelTool] No hay modelo activo para resetear.");
        }

        yield return new WaitForSeconds(0.25f); 

        if (myToggle != null)
        {
            myToggle.SetEnabled(true);
            myToggle.SetValueWithoutNotify(false);
        }
        
        isProcessing = false;
        
        // Cierra la herramienta tras ejecutarse
        Destroy(gameObject);
    }

    private Transform GetInspectedObject()
    {
        if (InspectedObjectController.Instance != null)
        {
            var inspectedObj = InspectedObjectController.Instance.GetInspectedObject();
            if (inspectedObj != null) 
                return inspectedObj.transform;
        }
        
        return GameObject.FindGameObjectWithTag("InspectedObject")?.transform;
    }
}