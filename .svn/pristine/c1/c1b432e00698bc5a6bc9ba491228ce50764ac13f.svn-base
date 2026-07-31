using System;
using Unity.Netcode;
using UnityEngine;

public class PlayerTransformCopyForNetwork : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private GameObject visualChild;
    [SerializeField] private NetworkedPlayerCharacter networkedPlayerCharacter;
    
    private float tickInterval;
    private float tickTimer;

    private void Awake()
    {
        if (targetTransform == null)
        {
            Debug.LogError("Target Transform is not assigned in the inspector.");
        }
    }

    private void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager is not initialized. Ensure it is set up in the scene.");
            return;
        }
       
        if (!networkedPlayerCharacter.IsLocalPlayer)
        {
            Debug.Log("PlayerTransformCopyForNetwork disabled for non-local player.");
            this.enabled = false; // Disable this script if not the local player
        }
        else
        {
            Debug.Log("Visualchild disabled.");
            visualChild.SetActive(false); // Optionally disable visual child for non-local players
        }
        
        tickInterval = 1f / NetworkManager.Singleton.NetworkTickSystem.TickRate;
        tickTimer = tickInterval;
    }

    private void Update()
    {
        tickTimer -= Time.deltaTime;

        if (tickTimer <= 0f)
        {
            CopyTransformToNetwork();
            tickTimer += tickInterval;  // += to catch up in case of frame drops
        }
    }

    public void CopyTransformToNetwork()
    {
        if (targetTransform != null)
        {
            SendTransformData(targetTransform.position, targetTransform.rotation);
        }
    }

    private void SendTransformData(Vector3 position, Quaternion rotation)
    {
        bool active = targetTransform.gameObject.activeSelf;

        if (active)
        {
            transform.position = position;
            transform.rotation = rotation;
        }
        else
        {
            transform.position = new Vector3(0, -1000, 0); // Move out of view
        }
    }
}