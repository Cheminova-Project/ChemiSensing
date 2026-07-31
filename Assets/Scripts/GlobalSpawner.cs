using UnityEngine;
using Unity.Netcode;

public class GlobalSpawner : MonoBehaviour
{
    [Tooltip("Arrastra aquí tu prefab GlobalRulerSync")]
    public GameObject globalRulerPrefab;

    private void Start()
    {
        // Nos suscribimos al evento de cuando el servidor/host termina de arrancar
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted += SpawnGlobalObjects;
        }
    }

    private void SpawnGlobalObjects()
    {
        // Solo el servidor puede instanciar y spawnear objetos de red
        if (NetworkManager.Singleton.IsServer)
        {
            GameObject rulerInstance = Instantiate(globalRulerPrefab);
            rulerInstance.GetComponent<NetworkObject>().Spawn();
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= SpawnGlobalObjects;
        }
    }
}