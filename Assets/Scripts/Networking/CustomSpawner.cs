using System;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Multiplayer;
using Unity.Netcode.Components;
using UnityEngine.Events;

/// <summary>
/// Permite la creación personalizada de objetos de red en la escena.
/// Gestiona el proceso de spawn y sincronización de objetos en red.
/// </summary>
public class CustomPlayerSpawner : NetworkBehaviour
{
    [SerializeField] private List<CharacterPrefab> characterPrefabs = new List<CharacterPrefab>();
    [SerializeField] private List<Transform> spawnPoints;

    public static CustomPlayerSpawner Instance { get; private set; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (NetworkManager.Singleton.IsClient)
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }
    
    
    public override void OnDestroy()
    {
        if (Instance == this) Instance = null;

        if (NetworkManager.Singleton)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnMyselfConnected(ulong clientId)
    {
        PlayerCharacterType playerType = PlatformController.Instance.GetPlayerCharacterType();
        RequestCharacterSpawnServerRpc(playerType, clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestCharacterSpawnServerRpc(PlayerCharacterType playerType, ulong clientId)
    {
        GameObject playerPrefab = null;

        foreach (var characterPrefab in characterPrefabs)
        {
            if (characterPrefab.type == playerType)
            {
                playerPrefab = characterPrefab.prefab;
                break;
            }
        }

        if (!playerPrefab)
        {
            Debug.LogError($"No se encontró un prefab para el tipo de jugador {playerType}");
            return;
        }
        
        Transform spawnPoint = GetRandomSpawnPoint();
        
        
        Vector3 spawnPosition = spawnPoint.position;
        Quaternion spawnRotation = spawnPoint.rotation;

        if(spawnPosition == Vector3.zero)
        {
            Debug.LogError("Spawn position is zero, using default position (0,0,0)");
            spawnPosition = Vector3.zero;
            spawnRotation = Quaternion.identity;
        }
        GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, spawnRotation);
        
        //Comprobamos la altura del playerPrefab
        //float playerHeight = playerInstance.GetComponent<CharacterController>()?.height ?? 1.8f; // Default height if not found
        //playerInstance.transform.position += new Vector3(0, playerHeight / 2, 0); // Adjust position to account for player height
        
        NetworkObject netObj = playerInstance.GetComponent<NetworkObject>();

        netObj.SpawnAsPlayerObject(clientId);
    }
    
    public Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("No hay puntos de spawn disponibles");
            return null;
        }

        int randomIndex = UnityEngine.Random.Range(0, spawnPoints.Count);
        
        return spawnPoints[randomIndex];
    }
    
    public void OnClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            OnMyselfConnected(clientId);
        }
    }
    
}

[Serializable]
public struct CharacterPrefab
{
    public PlayerCharacterType type;
    public GameObject prefab;
}
