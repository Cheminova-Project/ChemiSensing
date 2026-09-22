using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class UserSearchController : BaseUI
{
    public VisualTreeAsset userTemplate;
    
    private string usersListName = "user-list";
    private VisualElement usersList;
    
    private TextField searchField;
    private ScrollView dropdown;
    private Label userErrorLabel;
    private List<UserData> userResponseItems = new List<UserData>();
    private List<UserData> userData  = new List<UserData>();
    private List<Action> userActions = new List<Action>();
    
    private Coroutine searchCoroutine;
    private float searchDelay = 0.5f;

    private void OnSearchChanged(UserResponse userResponse, bool success)
    {
        if (success)
        {
            userResponseItems.Clear();

            foreach (var user in userResponse.items)
            {
                if (!user.username.Equals(GlobalManagement.Instance.username) && !IsUserSelected(user))
                    userResponseItems.Add(user);
            }
            
            UpdateDropdown();
        }
        else
        {
            Debug.LogWarning("Error when obtaining users.");
        }
    }

    private void OnSearchChanged(string newText)
    {
        if (string.IsNullOrEmpty(newText))
        {
            dropdown.Clear();
            dropdown.style.display = DisplayStyle.None;
            
            if (searchCoroutine != null)
            {
                StopCoroutine(searchCoroutine);
                searchCoroutine = null;
            }
            return;
        }
        
        if (searchCoroutine != null)
        {
            StopCoroutine(searchCoroutine);
        }

        searchCoroutine = StartCoroutine(DebouncedSearch());
    }
    
    private IEnumerator DebouncedSearch()
    {
        yield return new WaitForSeconds(searchDelay);
        GetUsers();
        searchCoroutine = null;
    }

    public void UpdateDropdown()
    {
        dropdown.style.display = DisplayStyle.Flex;
        dropdown.Clear();

        if (userResponseItems.Count == 0)
        {
            dropdown.style.display = DisplayStyle.None;
            return;
        }

        foreach (var user in userResponseItems)
        {
            var option = new Button(() => OnOptionSelected(user))
            {
                text = user.username
            };
            dropdown.Add(option);
        }
    }

    private void OnOptionSelected(UserData user)
    {
        dropdown.Clear();
        dropdown.style.display = DisplayStyle.None;
        searchField.value = String.Empty;
        CreateNewUser(user);
    }

    private void CreateNewUser(UserData user)
    {
        if (userData.Count >= 4)
        {
            Debug.LogWarning("Se ha alcanzado el límite máximo de 4 invitados.");
            searchField.value = String.Empty;
            
            if (userErrorLabel != null)
                userErrorLabel.style.display = DisplayStyle.Flex;
            
            return;
        }
        
        if (usersList == null || usersListName == null)
        {
            Debug.LogError("Falta asignar container o itemTemplate en el inspector.");
            return;
        }
        
        VisualElement newItem = userTemplate.CloneTree();
        newItem.style.flexGrow = 0;
        newItem.style.flexShrink = 0;
        newItem.style.width = StyleKeyword.Auto;
        newItem.style.flexDirection = FlexDirection.Row;

        Label label = newItem.Q<Label>("UserText");
        Button button = newItem.Q<Button>("UserButton");

        if (label != null)
            label.text = user.username;

        userData.Add(user);
        
        Debug.Log($"[DEBUG UI] Añadido '{user.username}'. Total en userData ({userData.Count}): {string.Join(", ", GetSelectedUsernames())}");

        if (button != null)
        {
            button.clicked += () => 
            {
                userData.Remove(user);
                usersList.Remove(newItem);
                
                Debug.Log($"[DEBUG UI] Borrado '{user.username}'. Total en userData ({userData.Count}): {string.Join(", ", GetSelectedUsernames())}");
                
                if (userData.Count < 4 && userErrorLabel != null)
                    userErrorLabel.style.display = DisplayStyle.None;
            };
        }

        usersList.Add(newItem);
    }

    public void GetUsers()
    {
        StartCoroutine(UserDB.GetUserList(OnSearchChanged, 3, searchField.value));
    }
    
    public List<int> GetUsersId()
    {
        List<int> id = new List<int>();

        foreach (var user in userData)
        {
            id.Add(user.id);
        }
        
        return id;
    }
    
    protected override void InitializeUI()
    {
        usersList = rootElement.Q<VisualElement>(usersListName);
        usersList.style.flexDirection = FlexDirection.Row;
        usersList.style.flexWrap = Wrap.Wrap;
        
        dropdown = rootElement.Q<ScrollView>("dropdown");

        VisualElement searchRoot = rootElement.Q<VisualElement>("search-root");
        if (searchRoot != null)
        {
            searchRoot.style.overflow = Overflow.Visible;
            dropdown.style.position = Position.Absolute;
            dropdown.style.top = 70;
        }

        searchField = rootElement.Q<TextField>("searchField");
        userErrorLabel = rootElement.Q<Label>("user-error-label");
        if (userErrorLabel != null)
            userErrorLabel.style.display = DisplayStyle.None;
        searchField.RegisterValueChangedCallback(evt => OnSearchChanged(evt.newValue));
    }

    public void FillUsersList(List<UserData> existingUsers)
    {
        if (existingUsers != null && existingUsers.Count > 0)
        {
            foreach (var item in existingUsers)
            {
                CreateNewUser(item);
            }
        }
    }

    public bool IsUserSelected(UserData user)
    {
        foreach (var item in userData)
        {
            if (item.id == user.id)
                return true;
        }
        
        return false;
    }
    
    public List<string> GetSelectedUsernames()
    {
        List<string> names = new List<string>();
        foreach (var user in userData)
        {
            names.Add(user.username);
        }
        
        return names;
    }
}