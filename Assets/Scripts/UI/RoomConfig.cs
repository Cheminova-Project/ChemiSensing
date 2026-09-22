using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RoomConfig : BaseUI
{
    [SerializeField] private UserSearchController userSearchController;
    private TextField roomNameInput;
    private Toggle audioToggle;
    private Label errorLabel;
    private Button createRoomButton;
    private Button backButton;

    private Button quickTestButton;

    void Start()
    {
        InitializeUI();
    }

    private void OnCreateRoomClicked()
    {
        string finalRoomName = roomNameInput.value;
        
        if (string.IsNullOrWhiteSpace(finalRoomName))
        {
            Debug.LogWarning("El nombre de la sala no puede estar vacío. Por favor, introduce un nombre.");
            
            if (errorLabel != null)
                errorLabel.style.display = DisplayStyle.Flex;
            
            roomNameInput.Focus(); 
            
            return; 
        }
        
        if (errorLabel != null)
            errorLabel.style.display = DisplayStyle.None;
        
        bool isAudioEnabled = audioToggle.value;
        
        List<string> usuariosAceptados = new List<string>();
        
        if (userSearchController != null)
        {
            var selected = userSearchController.GetSelectedUsernames();
            if (selected != null)
                usuariosAceptados = new List<string>(selected);
        }
        
        if (GlobalManagement.Instance != null && !string.IsNullOrEmpty(GlobalManagement.Instance.username))
        {
            string miUsuario = GlobalManagement.Instance.username;
            if (!usuariosAceptados.Contains(miUsuario))
                usuariosAceptados.Add(miUsuario);
        }
        
        GlobalVariables.Instance.SetAcceptedUsers(usuariosAceptados);
        GlobalVariables.Instance.SetIsAudioRoom(isAudioEnabled);
        GlobalVariables.Instance.SetRoomName(finalRoomName);
        
        UIDocumentManager.Instance.SwitchContext("room-management");
    }
    
    private void OnBackButtonClicked()
    {
        GlobalVariables.Instance.SetSelectedCHElementData(null);
        GlobalVariables.Instance.SetSelectedE3DModelData(null);
        UIDocumentManager.Instance.SwitchContext("mode-selector");
    }
    
    private void OnDestroy()
    {
        if (createRoomButton != null)
            createRoomButton.clicked -= OnCreateRoomClicked;
        
        if (backButton != null)
            backButton.clicked -= OnBackButtonClicked;

        if (roomNameInput != null)
            roomNameInput.UnregisterValueChangedCallback(HideErrorOnTyping);
        
        if (quickTestButton != null)
            quickTestButton.clicked -= OnQuickTestClicked;
    }

    protected override void InitializeUI()
    {
        roomNameInput = rootElement.Q<TextField>("room-name-input");
        audioToggle = rootElement.Q<Toggle>("audio-toggle");
        createRoomButton = rootElement.Q<Button>("create-room-btn");
        backButton = rootElement.Q<Button>("back-button");
        errorLabel = rootElement.Q<Label>("error-label");
        
        quickTestButton = rootElement.Q<Button>("quick-test-btn");

        if (roomNameInput == null || audioToggle == null || createRoomButton == null || backButton == null || errorLabel == null)
        {
            Debug.LogError("Faltan elementos de la UI. Revisa los nombres en el UXML.");
            return;
        }

        roomNameInput.maxLength = 30;
        
        if (errorLabel != null)
            errorLabel.style.display = DisplayStyle.None;

        roomNameInput.RegisterValueChangedCallback(HideErrorOnTyping);
        createRoomButton.clicked += OnCreateRoomClicked;
        backButton.clicked += OnBackButtonClicked;
        
        if (quickTestButton != null)
            quickTestButton.clicked += OnQuickTestClicked;
    }
    
    private void HideErrorOnTyping(ChangeEvent<string> evt)
    {
        if (errorLabel != null && !string.IsNullOrWhiteSpace(evt.newValue))
        {
            errorLabel.style.display = DisplayStyle.None;
        }
    }
    
    private void OnQuickTestClicked()
    {
        string testRoomName = "Test_" + Random.Range(100, 999);
        
        List<string> testUsers = new List<string> { "lola_flores", "super_mario", "paganini_1", "johndoe_admin" };
        GlobalVariables.Instance.SetAcceptedUsers(testUsers);
        GlobalVariables.Instance.SetIsAudioRoom(true);
        GlobalVariables.Instance.SetRoomName(testRoomName);
        
        UIDocumentManager.Instance.SwitchContext("room-management");
    }
}