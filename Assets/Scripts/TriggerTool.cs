using UnityEngine;

public class TriggerTool : ToolComponent
{
    protected override void OnToolActivatedInternal()
    {
        base.OnToolActivatedInternal();
        
        StartCoroutine(AutoDeactivate()); 
    }

    private System.Collections.IEnumerator AutoDeactivate()
    {
        yield return null;
        base.OnToolDeactivatedInternal();
    }
}