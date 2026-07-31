using System;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode.Components;
using UnityEngine.Events;

/// <summary>
/// Permite la creación personalizada de objetos en modo offline (sin red).
/// Gestiona el proceso de spawn local de objetos en la escena.
/// </summary>
public class OfflineCustomSpawner : MonoBehaviour
{
    [SerializeField] private List<CharacterPrefab> characterPrefabs = new List<CharacterPrefab>();
    [SerializeField] private List<Transform> spawnPoints;
    
    [SerializeField] public UnityEvent<GameObject> onVRPlayerSpawned;
    [SerializeField] public UnityEvent<GameObject> on2DPlayerSpawned;
    private void Awake()
    {
       
    }

    private void Start()
    {
        OnMyselfConnected();
    }

    public void OnDestroy()
    {
        
    }

    private void OnMyselfConnected()
    {
        PlayerCharacterType playerType = PlatformController.Instance.GetPlayerCharacterType();
        SpawnPlayer(playerType);
    }
    
    private void SpawnPlayer(PlayerCharacterType playerType)
    {
        GameObject playerPrefab = null;
        if(playerType == PlayerCharacterType.None)
        {
            Debug.LogError("Player type is None, cannot spawn player.");
            return;
        }
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
        
        if(playerType == PlayerCharacterType.VR)
            onVRPlayerSpawned?.Invoke(playerInstance);
        else
            on2DPlayerSpawned?.Invoke(playerInstance);
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
    
    
}

