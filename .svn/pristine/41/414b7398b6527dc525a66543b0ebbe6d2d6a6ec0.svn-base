using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Controlador de UI para el inicio de sesión y acceso al modo de la aplicación.
/// Gestiona los campos de usuario, contraseña y el flujo de autenticación.
/// </summary>
public class SignInToMode : BaseUI
{
    /// <summary>
    /// Referencia al gestor de login.
    /// </summary>
    private LoginManagement loginManagement;
    /// <summary>
    /// Campo de texto para el nombre de usuario.
    /// </summary>
    private TextField usernameField;
    /// <summary>
    /// Campo de texto para la contraseña.
    /// </summary>
    private TextField passwordField;
    /// <summary>
    /// Botón para iniciar sesión.
    /// </summary>
    private Button loginButton;
    /// <summary>
    /// Etiqueta para mostrar errores de contraseña.
    /// </summary>
    private Label passwordErrorLabel;
    
    /// <summary>
    /// Script que lanza la autentificacion
    /// </summary>
    private OAuthDemoClient _OAuthDemoClient;
    private VisualElement visitorErrorPanel;
    private Button btnVisitorBack;

    private void OnEnable()
    {
        base.OnEnable();
        _OAuthDemoClient.onSSOLoginCompleted += OnSignin;
    }

    private void OnDisable()
    {
        _OAuthDemoClient.onSSOLoginCompleted -= OnSignin;
    }
    
    /// <summary>
    /// Callback que se ejecuta al recibir la respuesta de inicio de sesión.
    /// </summary>
    /// <param name="data">Respuesta de inicio de sesión.</param>
    /// <param name="success">Indica si la autenticación fue exitosa.</param>
    private void OnSignin(SigninResponse data, bool success)
    {        
        if (success)
        {
            GlobalManagement.Instance.token = data.access_token;
            
            try
            {
                JwtParser.JwtPayload info = JwtParser.ParsePayload(data.access_token);
                if (info != null)
                {
                    GlobalManagement.Instance.userID = int.Parse(info.sub);
                    GlobalManagement.Instance.userRole = info.role;
                    GlobalManagement.Instance.username = info.name;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[SignInToMode] No se pudo decodificar el JWT en Editor: {ex.Message}");
            }
            
            OnSuccessfulLogin();
        }
        else
        {
            Debug.LogWarning("Error when signin.");
            string error;
            if (data == null)
                error = "Unrecognized data";
            else
                error = data.error;
            
            OnFailedLogin(error);  
        }
    }

    /// <summary>
    /// Inicializa la UI y configura los campos y botones de login.
    /// </summary>
    protected override void InitializeUI()
    {
        if (loginManagement == null)
            loginManagement = FindAnyObjectByType<LoginManagement>();
        
        if(_OAuthDemoClient == null)
            _OAuthDemoClient = GetComponent<OAuthDemoClient>();
        
        visitorErrorPanel = rootElement.Q<VisualElement>("visitor-error-panel");
        btnVisitorBack = rootElement.Q<Button>("btn-visitor-back");
        
        if (btnVisitorBack != null)
            btnVisitorBack.clicked += CloseVisitorErrorPanel;
      
        if (GlobalManagement.Instance != null && !string.IsNullOrEmpty(GlobalManagement.Instance.token))
        {
            OnSuccessfulLogin();
            return;
        }
        
        VisualElement editorContent = rootElement.Q<VisualElement>("LoginClassic");
        VisualElement buildContent = rootElement.Q<VisualElement>("LoginSSO");
        
        if (editorContent != null && buildContent != null)
        {
            if (Application.isEditor)
            {
                editorContent.style.display = DisplayStyle.Flex;
                buildContent.style.display = DisplayStyle.None;

                usernameField = editorContent.Q<TextField>("username-field");
                passwordField = editorContent.Q<TextField>("password-field");
                passwordErrorLabel = editorContent.Q<Label>("password-error");
                loginButton = editorContent.Q<Button>("continue-button");

                ShowCredentialsError(false);

                if (loginButton != null)
                    loginButton.clicked += OnLoginClicked;
                else
                    Debug.LogWarning("The continue button was not found in Editor mode.");

                if (loginManagement.autoLogin)
                    LoginWithCredentials(loginManagement.GetPassword(), loginManagement.GetUsername());
            }
            else
            {
                editorContent.style.display = DisplayStyle.None;
                buildContent.style.display = DisplayStyle.Flex;

                loginButton = buildContent.Q<Button>("LogInAuth-button");
                _OAuthDemoClient.StatusText = buildContent.Q<Label>("label-status");

                if (loginButton != null)
                    loginButton.clicked += OnLoginClickedAuth;
                else
                    Debug.LogWarning("The LogInAuth button was not found in Build mode.");
            }
        }
        else
        {
            Debug.LogError("[SignInToMode] UI no encontrada. Revisa que el UXML tenga 'LoginClassic' y 'LoginSSO'.");
        }
    }
    /// <summary>
    /// Login mediante SSO Metodo unico para development
    /// </summary>
    private void OnLoginClickedAuth()
    {
        _OAuthDemoClient.StartLogin();
    }

    /// <summary>
    /// Evento al hacer clic en el botón de login clasico mediante API /login
    /// </summary>
    void OnLoginClicked()
    {
        string password = passwordField?.value;
        string username = usernameField?.value;

        loginManagement.SetPassword(password);
        loginManagement.SetUsername(username);
        LoginWithCredentials(password, username);
    }

    /// <summary>
    /// Muestra u oculta el mensaje de error de credenciales.
    /// </summary>
    /// <param name="visible">Si es true, muestra el error.</param>
    /// <param name="err">Mensaje de error a mostrar.</param>
    public void ShowCredentialsError(bool visible, string err = "Unrecognized error")
    {
        if(passwordErrorLabel == null)
            return;
        passwordErrorLabel.text = err;
        passwordErrorLabel.style.display =visible? DisplayStyle.Flex : DisplayStyle.None;
    }
    
    /// <summary>
    /// Inicia el proceso de login con las credenciales proporcionadas.
    /// </summary>
    /// <param name="password">Contraseña del usuario.</param>
    /// <param name="username">Nombre de usuario.</param>
    public void LoginWithCredentials(string password, string username)
    {
        StartCoroutine(SigninDB.Signin(OnSignin, password, username));
    }
    
    /// <summary>
    /// Lógica tras un login exitoso.
    /// </summary>
    public void OnSuccessfulLogin()
    {
        string role = GlobalManagement.Instance.userRole;
        
        if (string.IsNullOrEmpty(role) || role.Equals("visitor", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning("[SignInToMode] Login rechazado. El rol del usuario es 'visitor'.");
            
            ClearUserData();
            
            if (visitorErrorPanel != null)
                visitorErrorPanel.style.display = DisplayStyle.Flex;
                
            return;
        }
        
        ShowCredentialsError(false);
        LoadModeWindow();
    }
    
    private void ClearUserData()
    {
        if (GlobalManagement.Instance != null)
        {
            GlobalManagement.Instance.token = "";
            GlobalManagement.Instance.username = "";
            GlobalManagement.Instance.userID = 0;
            GlobalManagement.Instance.userRole = "";
        }
        
        PlayerPrefs.DeleteKey("oauth_state");
        PlayerPrefs.DeleteKey("oauth_code_verifier");
        PlayerPrefs.Save();
        
        if (usernameField != null)
            usernameField.value = "";
        
        if (passwordField != null)
            passwordField.value = "";
        
        if (_OAuthDemoClient != null && _OAuthDemoClient.StatusText != null)
            _OAuthDemoClient.StatusText.text = "Not logged in";
    }
    
    private void CloseVisitorErrorPanel()
    {
        if (visitorErrorPanel != null)
            visitorErrorPanel.style.display = DisplayStyle.None;
    }
    
    /// <summary>
    /// Lógica tras un login fallido.
    /// </summary>
    /// <param name="err">Mensaje de error.</param>
    public void OnFailedLogin(string err = "Unrecognized error")
    {
        // Here you can handle the logic after a failed login.
        ShowCredentialsError(true, err);
    }
    
    /// <summary>
    /// Cambia el contexto a la ventana de selección de modo.
    /// </summary>
    void LoadModeWindow()
    {
        UIDocumentManager.Instance.SwitchContext("mode-selector");
    }
}