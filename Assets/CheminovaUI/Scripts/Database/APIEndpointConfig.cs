using UnityEngine;

[CreateAssetMenu(menuName = "Config/API Endpoint Config")]
public class APIEndpointConfig : ScriptableObject
{
    [Header("API URL Endpoint")]
    public string endpointURL = "";
}