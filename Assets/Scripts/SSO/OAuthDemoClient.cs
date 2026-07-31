using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;

public class OAuthDemoClient : MonoBehaviour
{
    [Header("OAuth Server Config")]
    
    private string authServerBaseUrl;
    private string clientId; //"vTCfl9mPwipn83fwTs8icEaw"; //->(chemisensing)//"F0WItXEZFNwNEe0SYjHOzUsp"; //->(cheminova) // "1b4Qw0Nww3kby7YY7LfHy97j"//->(chemiinspection)
    private string redirectUri = "chemisensing://oauth/callback";//"cheminova://oauth/callback"; "chemiinspection://oauth/callback"

    [Header("Referencias UI Toolkit")]
    private Label statusText;

    private string state;
    private string codeVerifier;
    
    private NamedPipeServerStream server;
    private Thread listenerThread;

    public Label StatusText
    {
        get => statusText;
        set => statusText = value;
    }
    public SigninResponse signinResponse { get; private set; }
    public event UnityAction<SigninResponse, bool> onSSOLoginCompleted; 

    private void Awake()
    {
        if (GlobalManagement.Instance != null)
        {
            authServerBaseUrl = GlobalManagement.Instance.urlBase;
            clientId = GlobalManagement.Instance.clientId;
        }
        else
            Debug.LogWarning("Global Management Instance Not Found - OAthDemoClient");
        
        Application.deepLinkActivated += OnDeepLinkActivated;

        if (!string.IsNullOrEmpty(Application.absoluteURL))
        {
            OnDeepLinkActivated(Application.absoluteURL);
        }
    }

    public void StartLogin()
    {
        state = Guid.NewGuid().ToString("N");
        codeVerifier = GenerateCodeVerifier();

        PlayerPrefs.SetString("oauth_state", state);
        PlayerPrefs.SetString("oauth_code_verifier", codeVerifier);
        PlayerPrefs.Save();

        string codeChallenge = GenerateCodeChallenge(codeVerifier);

        string authorizeUrl =
            authServerBaseUrl + "/oauth/authorize" +
            "?response_type=code" +
            "&client_id=" + Uri.EscapeDataString(clientId) +
            "&redirect_uri=" + Uri.EscapeDataString(redirectUri) +
            "&scope=" + Uri.EscapeDataString("openid profile") +
            "&state=" + Uri.EscapeDataString(state) +
            "&nonce=" + Uri.EscapeDataString(Guid.NewGuid().ToString("N")) +
            "&code_challenge=" + Uri.EscapeDataString(codeChallenge) +
            "&code_challenge_method=S256";

        statusText.text = "Opening browser...";
        Debug.Log("OAuth URL: " + authorizeUrl);

        StartListener();
        
        Application.OpenURL(authorizeUrl);
    }

    private void OnDeepLinkActivated(string url)
    {
        Debug.Log("Deep link received: " + url);

        Uri uri = new Uri(url);
        Dictionary<string, string> queryParams = ParseQueryString(uri.Query);

        string code = queryParams.ContainsKey("code") ? queryParams["code"] : null;
        string returnedState = queryParams.ContainsKey("state") ? queryParams["state"] : null;
        string error = queryParams.ContainsKey("error") ? queryParams["error"] : null;

        if (!string.IsNullOrEmpty(error))
        {
            statusText.text = "OAuth error: " + error;
            return;
        }

        string expectedState = PlayerPrefs.GetString("oauth_state", "");

        if (returnedState != expectedState)
        {
            statusText.text =
                "Invalid state. Possible CSRF.\n" +
                "Expected: " + expectedState + "\n" +
                "Received: " + returnedState;
            return;
        }

        if (string.IsNullOrEmpty(code))
        {
            statusText.text = "No authorization code received.";
            return;
        }

        string savedCodeVerifier = PlayerPrefs.GetString("oauth_code_verifier", "");

        if (string.IsNullOrEmpty(savedCodeVerifier))
        {
            statusText.text = "Missing PKCE code_verifier.";
            return;
        }

        statusText.text = "Code received. Exchanging for token with PKCE...";
        StartCoroutine(ExchangeCodeForToken(code, savedCodeVerifier));
    }

    private IEnumerator ExchangeCodeForToken(string code, string savedCodeVerifier)
    {

        string tokenUrl = authServerBaseUrl + "/oauth/token";

        WWWForm form = new WWWForm();
        form.AddField("grant_type", "authorization_code");
        form.AddField("client_id", clientId);
        form.AddField("code", code);
        form.AddField("redirect_uri", redirectUri);
        form.AddField("code_verifier", savedCodeVerifier);

        using (UnityWebRequest request = UnityWebRequest.Post(tokenUrl, form))
        {

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                statusText.text =
                    "Token request failed:\n" +
                    request.error + "\n" +
                    request.downloadHandler.text;

                //Token request failed - Llamar a funcion login error. Cambiar a vista de login
                signinResponse = new SigninResponse ("", request.error);
                onSSOLoginCompleted?.Invoke(signinResponse, false); //Lanzar evento de proceso completado

                yield break;
            }

            string tokenResponseJson = request.downloadHandler.text;
            TokenResponse tokenResponse = JsonUtility.FromJson<TokenResponse>(tokenResponseJson);

            if (string.IsNullOrEmpty(tokenResponse.access_token))
            {
                statusText.text =
                    "Token response received, but access_token was empty:\n" +
                    tokenResponseJson;
                yield break;
            }
            else
            {
                //Get username from token
                /*JwtParser.JwtPayload info = JwtParser.ParsePayload(tokenResponse.access_token);
                GlobalManagement.Instance.userID = int.Parse(info.sub);
                GlobalManagement.Instance.username = info.name;
                GlobalManagement.Instance.userRole = info.role;
                GlobalManagement.Instance.token = tokenResponse.access_token;

                Debug.Log("User Info username: ID=" + info.name + ", GlobalManagement.Instance.username=" + GlobalManagement.Instance.username );*/
                signinResponse = new SigninResponse (tokenResponse.access_token, "Login successful");
                onSSOLoginCompleted?.Invoke(signinResponse, true); //Lanzar evento de proceso completado correctamente
            }
            
        }
    }

    private string GenerateCodeVerifier()
    {
        byte[] randomBytes = new byte[64];

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        return Base64UrlEncode(randomBytes);
    }

    private string GenerateCodeChallenge(string verifier)
    {
        byte[] verifierBytes = Encoding.ASCII.GetBytes(verifier);

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(verifierBytes);
            return Base64UrlEncode(hash);
        }
    }

    private string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private Dictionary<string, string> ParseQueryString(string query)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();

        if (query.StartsWith("?"))
        {
            query = query.Substring(1);
        }

        string[] pairs = query.Split('&');

        foreach (string pair in pairs)
        {
            if (string.IsNullOrEmpty(pair)) continue;

            string[] parts = pair.Split('=');
            string key = Uri.UnescapeDataString(parts[0]);
            string value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : "";

            result[key] = value;
        }

        return result;
    }

    private void OnDestroy()
    {
        Application.deepLinkActivated -= OnDeepLinkActivated;
    }

    private void StartListener()
    {
        listenerThread = new Thread(() =>
        {
            while (true)
            {
                using (server = new NamedPipeServerStream("ChemiAnalysisPipe"))
                {
                    server.WaitForConnection();
                    using (var reader = new StreamReader(server))
                    {
                        string data = reader.ReadLine();
                        OnDeepLinkActivated(data);
                    }
                }
            }
        });

        listenerThread.IsBackground = true;
        listenerThread.Start();
    }
    
    [Serializable]
    private class TokenResponse
    {
        public string access_token;
        public int expires_in;
        public string id_token;
        public string scope;
        public string token_type;
    }
}