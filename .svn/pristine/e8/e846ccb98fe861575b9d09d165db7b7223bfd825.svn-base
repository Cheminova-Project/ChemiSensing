using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class GlobalRoomWarningManager : MonoBehaviour
{
    private UIDocument uiDocument;
    [SerializeField] private VisualTreeAsset warningTemplate; 
    
    private const string WARNING_PANEL_NAME = "room-warning-panel";
    private Coroutine countdownCoroutine;

    private void OnEnable()
    {
        UserControllerPlayer.OnRoomClosingWarning += ShowWarningPanel;
    }

    private void OnDisable()
    {
        UserControllerPlayer.OnRoomClosingWarning -= ShowWarningPanel;
    }

    private void ShowWarningPanel()
    {
        if (uiDocument == null)
            uiDocument = FindAnyObjectByType<UIDocument>();
        
        if (uiDocument == null || warningTemplate == null)
        {
            Debug.LogError("[WarningManager] Faltan referencias críticas en el script");
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;
        
        if (root.Q(WARNING_PANEL_NAME) != null) 
            return;

        VisualElement panel = warningTemplate.CloneTree();
        panel.name = WARNING_PANEL_NAME;
        panel.style.position = Position.Absolute;
        panel.style.width = Length.Percent(100);
        panel.style.height = Length.Percent(100);
        root.Add(panel);

        Label countdownLabel = panel.Q<Label>("countdown-message");
        if (countdownLabel != null)
        {
            if (countdownCoroutine != null) StopCoroutine(countdownCoroutine);
            countdownCoroutine = StartCoroutine(CountdownRoutine(countdownLabel, panel));
        }
    }

    private IEnumerator CountdownRoutine(Label label, VisualElement panel)
    {
        int timeRemaining = 3;

        while (timeRemaining > 0)
        {
            label.text = $"The room will close in {timeRemaining} seconds...";
            yield return new WaitForSeconds(1f);
            timeRemaining--;
        }

        label.text = "Disconnecting...";
        yield return new WaitForSeconds(0.5f);

        panel?.RemoveFromHierarchy();
    }
}