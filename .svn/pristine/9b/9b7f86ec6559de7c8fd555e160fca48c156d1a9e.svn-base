using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class AngleRulerUI : ToolComponent
{
    string addAngleButtonName = "add-angle-button";
    string deletePointsButtonName = "delete-points-button";
    string anglesContainerName = "angles-container";
    string angleStartDropdown = "angle-start";
    string angleVertexDropdown = "angle-vertex";
    string angleEndDropdown = "angle-end";
    string angleValueLabel = "angle-value";
    string highlightButtonName = "highlight-button";
    string deleteAngleButtonName = "delete-button";
    string cycleVertexButtonName = "cycle-vertex-button";
    string shareAngleToggleName = "share-segment-toggle";

    [SerializeField] private VisualTreeAsset angleEntryTemplate;
    
    private AngleRuler angleRuler;
    private Button addAngleButton;
    private Button deletePointsButton;
    private bool isRefreshingUI;

    private class AngleEntryData
    {
        public VisualElement entry;
        public DropdownField startDropdown;
        public DropdownField vertexDropdown;
        public DropdownField endDropdown;
        public AutoSizeLabel angleLabel;
        
        public Button highlightButton;
        public Button deleteButton;
        public Button cycleVertexButton;
        public Toggle shareAngleToggle;
        
        public EventCallback<ChangeEvent<string>> dropdownCallback;
        public EventCallback<ChangeEvent<bool>> shareCallback;
        
        public Action highlightAction;
        public Action deleteAction;
        public Action cycleVertexAction;
        
        public Vector3Int currentAngle = new Vector3Int(-1, -1, -1); 
        public int startPointId = -1;
        public int vertexPointId = -1;
        public int endPointId = -1;
    }
    
    private AngleRuler GetActiveAngleRuler()
    {
        if (angleRuler != null && angleRuler.isActiveAndEnabled) 
            return angleRuler;

        var allRulers = FindObjectsByType<AngleRuler>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        angleRuler = allRulers.FirstOrDefault(r => r.isActiveAndEnabled) ?? allRulers.FirstOrDefault(r => r.enabled);
        
        return angleRuler;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        
        AngleRuler currentRuler = GetActiveAngleRuler();

        if (currentRuler == null)
        {
            Debug.LogWarning("[AngleRulerUI] Esperando a que un AngleRuler se active...");
            return;
        }

        Ruler.OnPointAdded += OnRulerPointAdded;
        Ruler.OnPointsReset += OnRulerPointsReset;

        addAngleButton = uIDocument.rootVisualElement.Q<Button>(addAngleButtonName);
        if (addAngleButton != null)
        {
            addAngleButton.clicked -= OnAddAngleButtonClicked;
            addAngleButton.clicked += OnAddAngleButtonClicked;
            
            bool canAddAngles = angleRuler.GetPointCount() >= 3;
            addAngleButton.SetEnabled(canAddAngles);
            
            if (!canAddAngles && ToolMessageHandler.Instance != null)
                ToolMessageHandler.Instance.ShowMessage("Add at least 3 points to create angles.", 4f, MessageType.Error);
        }

        deletePointsButton = uIDocument.rootVisualElement.Q<Button>(deletePointsButtonName);
        if (deletePointsButton != null)
        {
            deletePointsButton.clicked -= OnDeletePointsClicked;
            deletePointsButton.clicked += OnDeletePointsClicked;
        }

        var savedAngles = currentRuler.GetSavedAngles();
        FillAngles(savedAngles);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
            
        Ruler.OnPointAdded -= OnRulerPointAdded;
        Ruler.OnPointsReset -= OnRulerPointsReset;
        
        if (deletePointsButton != null)
            deletePointsButton.clicked -= OnDeletePointsClicked;
    }
    
    private void OnDeletePointsClicked()
    {
        AngleRuler current = GetActiveAngleRuler();
        if (current != null)
            current.ResetRuler();
    }

    private void OnRulerPointAdded()
    {
        UpdateAllAngleDropdowns();

        AngleRuler current = GetActiveAngleRuler();
        if (current != null && addAngleButton.enabledSelf == false && current.GetPointCount() >= 3)
        {
            addAngleButton.SetEnabled(true);
            ToolMessageHandler.Instance.ShowMessage("You can now measure angles.", 4f, MessageType.Info);
        }
    }

    private void OnRulerPointsReset()
    {
        VisualElement anglesContainer = uIDocument.rootVisualElement.Q<VisualElement>(anglesContainerName);
        if (anglesContainer == null) 
            return;
        
        anglesContainer.Clear();
        addAngleButton.SetEnabled(false);
        ToolMessageHandler.Instance.ShowMessage("Add at least 3 points to create angles.", 4f, MessageType.Error);
    }

    private void OnAddAngleButtonClicked()
    {
        AddAngleEntry();
    }

    private void FillAngles(List<AngleRuler.AngleData> angles)
    {
        if (uIDocument == null || uIDocument.rootVisualElement == null || angles == null || angles.Count == 0)
            return;

        VisualElement anglesContainer = uIDocument.rootVisualElement.Q<VisualElement>(anglesContainerName);
        if (anglesContainer == null) return;

        isRefreshingUI = true;
    
        foreach (var child in anglesContainer.Children().ToList())
        {
            child.RemoveFromHierarchy();
        }

        foreach (var angle in angles)
        {
            AddAngleEntry(new Vector3Int(angle.pointA, angle.vertex, angle.pointC));
        }
    
        isRefreshingUI = false;
    }

    private void AddAngleEntry(Vector3Int angle = default)
    {
        VisualElement anglesContainer = uIDocument.rootVisualElement.Q<VisualElement>(anglesContainerName);
        if (anglesContainer == null || angleEntryTemplate == null) return;

        VisualElement newEntry = angleEntryTemplate.CloneTree();
        anglesContainer.Add(newEntry);

        var entryData = new AngleEntryData();
        entryData.entry = newEntry;
        entryData.startDropdown = newEntry.Q<DropdownField>(angleStartDropdown);
        entryData.vertexDropdown = newEntry.Q<DropdownField>(angleVertexDropdown);
        entryData.endDropdown = newEntry.Q<DropdownField>(angleEndDropdown);
        entryData.angleLabel = newEntry.Q<AutoSizeLabel>(angleValueLabel);
        entryData.angleLabel.text = "0°";

        if (entryData.startDropdown != null && entryData.vertexDropdown != null && entryData.endDropdown != null)
        {
            entryData.dropdownCallback = evt => OnDropdownIndexChanged(entryData);
            if (angle != default)
            {
                entryData.startPointId = angle.x + 1;
                entryData.vertexPointId = angle.y + 1;
                entryData.endPointId = angle.z + 1;
            }

            RefreshRowChoicesByIndex(entryData);

            entryData.startDropdown.RegisterValueChangedCallback(entryData.dropdownCallback);
            entryData.vertexDropdown.RegisterValueChangedCallback(entryData.dropdownCallback);
            entryData.endDropdown.RegisterValueChangedCallback(entryData.dropdownCallback);
        }

        var localHighlight = newEntry.Q<Button>(highlightButtonName);
        if (localHighlight != null)
        {
            entryData.highlightButton = localHighlight;
            entryData.highlightAction = () => OnHighlightButtonClicked(newEntry);
            localHighlight.userData = newEntry;
            localHighlight.clicked += entryData.highlightAction;
        }

        var localDelete = newEntry.Q<Button>(deleteAngleButtonName);
        if (localDelete != null)
        {
            entryData.deleteButton = localDelete;
            entryData.deleteAction = () => OnDeleteAngleButtonClicked(newEntry);
            localDelete.userData = newEntry;
            localDelete.clicked += entryData.deleteAction;
        }
        
        var localCycle = newEntry.Q<Button>(cycleVertexButtonName);
        if (localCycle != null)
        {
            entryData.cycleVertexButton = localCycle;
            entryData.cycleVertexAction = () => OnCycleVertexButtonClicked(newEntry);
            localCycle.userData = newEntry;
            localCycle.clicked += entryData.cycleVertexAction;
        }
        
        var localShare = newEntry.Q<Toggle>(shareAngleToggleName);
        if (localShare != null)
        {
            entryData.shareAngleToggle = localShare;
            entryData.shareCallback = evt => OnShareAngleToggleChanged(localShare);
            localShare.RegisterValueChangedCallback(entryData.shareCallback);
            localShare.userData = newEntry;
        }

        newEntry.userData = entryData;

        bool previousState = isRefreshingUI;
        isRefreshingUI = true; 
        ProcessAngleCalculation(entryData);
        isRefreshingUI = previousState;
        ForceFontSizeRecalculation();
    }

    private void RefreshRowChoicesByIndex(AngleEntryData entryData)
    {
        AngleRuler current = GetActiveAngleRuler();
        if (current == null)
            return;
        
        int pCount = current.GetPointCount();

        if (entryData.dropdownCallback != null)
        {
            entryData.startDropdown.UnregisterValueChangedCallback(entryData.dropdownCallback);
            entryData.vertexDropdown.UnregisterValueChangedCallback(entryData.dropdownCallback);
            entryData.endDropdown.UnregisterValueChangedCallback(entryData.dropdownCallback);
        }

        entryData.startDropdown.choices = GetFilteredChoices(pCount, entryData.startPointId, entryData.vertexPointId, entryData.endPointId);
        entryData.vertexDropdown.choices = GetFilteredChoices(pCount, entryData.vertexPointId, entryData.startPointId, entryData.endPointId);
        entryData.endDropdown.choices = GetFilteredChoices(pCount, entryData.endPointId, entryData.startPointId, entryData.vertexPointId);

        entryData.startDropdown.index = FindPointIndexInDropdown(entryData.startDropdown, entryData.startPointId);
        entryData.vertexDropdown.index = FindPointIndexInDropdown(entryData.vertexDropdown, entryData.vertexPointId);
        entryData.endDropdown.index = FindPointIndexInDropdown(entryData.endDropdown, entryData.endPointId);

        if (entryData.dropdownCallback != null)
        {
            entryData.startDropdown.RegisterValueChangedCallback(entryData.dropdownCallback);
            entryData.vertexDropdown.RegisterValueChangedCallback(entryData.dropdownCallback);
            entryData.endDropdown.RegisterValueChangedCallback(entryData.dropdownCallback);
        }
    }

    private List<string> GetFilteredChoices(int pointCount, int myId, int exclude1, int exclude2)
    {
        List<string> choices = new List<string>();
        for (int i = 1; i <= pointCount; i++)
        {
            bool isExcluded1 = (exclude1 != -1 && i == exclude1);
            bool isExcluded2 = (exclude2 != -1 && i == exclude2);

            if (i == myId || (!isExcluded1 && !isExcluded2))
            {
                choices.Add(i.ToString());
            }
        }
        return choices;
    }

    private int FindPointIndexInDropdown(DropdownField dropdown, int pointNumber)
    {
        if (dropdown == null || dropdown.choices == null || pointNumber == -1) 
            return -1;
        
        for (int i = 0; i < dropdown.choices.Count; i++)
        {
            if (int.TryParse(dropdown.choices[i], out int choice) && choice == pointNumber) 
                return i;
        }
        return -1;
    }

    private void UpdateAllAngleDropdowns()
    {
        if (uIDocument == null || uIDocument.rootVisualElement == null) 
            return;
        
        VisualElement anglesContainer = uIDocument.rootVisualElement.Q<VisualElement>(anglesContainerName);
        if (anglesContainer == null) 
            return;

        foreach (var entry in anglesContainer.Children())
        {
            var data = entry.userData as AngleEntryData;
            if (data != null)
            {
                RefreshRowChoicesByIndex(data);
            }
        }
    }

    private void OnDropdownIndexChanged(AngleEntryData entryData)
    {
        if (entryData.startDropdown.index != -1)
            int.TryParse(entryData.startDropdown.choices[entryData.startDropdown.index], out entryData.startPointId);
            
        if (entryData.vertexDropdown.index != -1)
            int.TryParse(entryData.vertexDropdown.choices[entryData.vertexDropdown.index], out entryData.vertexPointId);
            
        if (entryData.endDropdown.index != -1)
            int.TryParse(entryData.endDropdown.choices[entryData.endDropdown.index], out entryData.endPointId);

        RefreshRowChoicesByIndex(entryData);
        ProcessAngleCalculation(entryData);
    }

    private void ProcessAngleCalculation(AngleEntryData entryData)
    {
        AngleRuler current = GetActiveAngleRuler();
        if (current == null || entryData.startDropdown.index == -1 || entryData.vertexDropdown.index == -1 || entryData.endDropdown.index == -1) 
            return;

        int idxA = entryData.startPointId - 1;
        int idxV = entryData.vertexPointId - 1;
        int idxC = entryData.endPointId - 1;

        Vector3Int newAngle = new Vector3Int(idxA, idxV, idxC);
        Vector3Int oldAngle = entryData.currentAngle;

        if (oldAngle != newAngle)
        {
            if (oldAngle != new Vector3Int(-1, -1, -1)) 
                current.RemoveSavedAngle(oldAngle.x, oldAngle.y, oldAngle.z);
                
            current.AddSavedAngle(newAngle.x, newAngle.y, newAngle.z);
            entryData.currentAngle = newAngle;
        }

        float angleValue = current.GetAngle(idxA, idxV, idxC);
        current.HighlightAngle(idxA, idxV, idxC);

        if (entryData.angleLabel != null)
        {
            entryData.angleLabel.text = $"{angleValue:F2}°";
            entryData.angleLabel.ForceRecalculate();
        }
    }

    private void OnHighlightButtonClicked(VisualElement container)
    {
        AngleRuler current = GetActiveAngleRuler();
        if (current == null)
            return;
        
        var data = container.userData as AngleEntryData;
        if (data != null && data.currentAngle != new Vector3Int(-1, -1, -1))
        {
            bool isShared = data.shareAngleToggle != null ? data.shareAngleToggle.value : false;

            if (isShared)
                current.HighlightAngleNetworked(data.currentAngle.x, data.currentAngle.y, data.currentAngle.z);
            else
                current.HighlightAngle(data.currentAngle.x, data.currentAngle.y, data.currentAngle.z);
        }
    }
    
    private void OnShareAngleToggleChanged(Toggle shareAngleToggle)
    {
        AngleRuler current = GetActiveAngleRuler();
        if (current == null)
            return;
        
        VisualElement segmentContainer = (VisualElement)shareAngleToggle.userData;
        if (segmentContainer == null) return;
        
        if (shareAngleToggle.value)
        {
            VisualElement anglesContainer = segmentContainer.parent; 
            if (anglesContainer != null)
            {
                foreach (var entry in anglesContainer.Children())
                {
                    if (entry == segmentContainer) continue; 

                    Toggle otherToggle = entry.Q<Toggle>(shareAngleToggleName);
                    if (otherToggle != null && otherToggle.value)
                    {
                        otherToggle.SetValueWithoutNotify(false);
                    }
                }
            }
        }

        var data = segmentContainer.userData as AngleEntryData;
        if (data != null && data.currentAngle != new Vector3Int(-1, -1, -1))
        {
            current.SyncSharedAngle(data.currentAngle.x, data.currentAngle.y, data.currentAngle.z, shareAngleToggle.value);
        }
    }

    private void OnDeleteAngleButtonClicked(VisualElement container)
    {
        AngleRuler current = GetActiveAngleRuler();
        if (current == null)
            return;
        
        if (container == null)
            return;
        
        var entryData = container.userData as AngleEntryData;
    
        if (entryData.cycleVertexButton != null) 
            entryData.cycleVertexButton.clicked -= entryData.cycleVertexAction;
        
        if (entryData != null && entryData.currentAngle != new Vector3Int(-1, -1, -1))
        {
            current.RemoveSavedAngle(entryData.currentAngle.x, entryData.currentAngle.y, entryData.currentAngle.z);
        
            if (entryData.startDropdown != null && entryData.dropdownCallback != null) 
                entryData.startDropdown.UnregisterValueChangedCallback(entryData.dropdownCallback);
            if (entryData.vertexDropdown != null && entryData.dropdownCallback != null) 
                entryData.vertexDropdown.UnregisterValueChangedCallback(entryData.dropdownCallback);
            if (entryData.endDropdown != null && entryData.dropdownCallback != null) 
                entryData.endDropdown.UnregisterValueChangedCallback(entryData.dropdownCallback);
            
            if (entryData.shareAngleToggle != null && entryData.shareCallback != null)
                entryData.shareAngleToggle.UnregisterValueChangedCallback(entryData.shareCallback);
                
            if (entryData.highlightButton != null) 
                entryData.highlightButton.clicked -= entryData.highlightAction;
            if (entryData.deleteButton != null) 
                entryData.deleteButton.clicked -= entryData.deleteAction;
        
            container.RemoveFromHierarchy();
        }
    }
    
    private void OnCycleVertexButtonClicked(VisualElement container)
    {
        var entryData = container.userData as AngleEntryData;
        if (entryData == null || entryData.startPointId == -1 || entryData.vertexPointId == -1 || entryData.endPointId == -1) 
            return;

        int tempStart = entryData.startPointId;
        entryData.startPointId = entryData.vertexPointId;
        entryData.vertexPointId = entryData.endPointId;
        entryData.endPointId = tempStart;

        RefreshRowChoicesByIndex(entryData);
        ProcessAngleCalculation(entryData);
    }
    
    private void ForceFontSizeRecalculation()
    {
        if (uIDocument == null || uIDocument.rootVisualElement == null) return;
        
        uIDocument.rootVisualElement.schedule.Execute(() =>
        {
            var allLabels = uIDocument.rootVisualElement.Query<AutoSizeLabel>().ToList();
            foreach (var label in allLabels)
            {
                label.ScheduleFontRecalculation();
            }
        });
    }

    void OnDestroy()
    {
        if (addAngleButton != null) addAngleButton.clicked -= OnAddAngleButtonClicked;
        
        if (uIDocument != null && uIDocument.rootVisualElement != null)
        {
            var anglesContainer = uIDocument.rootVisualElement.Q<VisualElement>(anglesContainerName);
            if (anglesContainer != null)
            {
                foreach (var entry in anglesContainer.Children())
                {
                    var data = entry.userData as AngleEntryData;
                    if (data != null)
                    {
                        if (data.cycleVertexButton != null) data.cycleVertexButton.clicked -= data.cycleVertexAction;
                        if (data.startDropdown != null) data.startDropdown.UnregisterValueChangedCallback(data.dropdownCallback);
                        if (data.vertexDropdown != null) data.vertexDropdown.UnregisterValueChangedCallback(data.dropdownCallback);
                        if (data.endDropdown != null) data.endDropdown.UnregisterValueChangedCallback(data.dropdownCallback);
                        if (data.shareAngleToggle != null) data.shareAngleToggle.UnregisterValueChangedCallback(data.shareCallback);
                        if (data.highlightButton != null) data.highlightButton.clicked -= data.highlightAction;
                        if (data.deleteButton != null) data.deleteButton.clicked -= data.deleteAction;
                    }
                }
            }
        }
    }
}