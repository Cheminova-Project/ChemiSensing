using System.Collections;
using UnityEngine;
using System;
using UnityEngine.UI;
using Unity.XR.CoreUtils;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public struct MovementData
{
    public PlayerCharacterType platform;
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 scale;
    public Vector3 offset;
    public Vector3 posCamera;
    public Quaternion rotCamera;
}

public class UserControllerPlayer : NetworkBehaviour
{
    [Header("Network Clone Setup")]
    [Tooltip("Arrastra aquí los scripts de tracking VR del prefab (TrackedPoseDriver, Locomotion, RayInteractors) para que se apaguen en los clones.")]
    public Behaviour[] componentsToDisableForClones;
    
    [Tooltip("Arrastra aquí los GameObjects que solo debe tener tu jugador (Rayos láser, menús atados a la mano, etc.)")]
    public GameObject[] objectsToDisableForClones;
    
    private GameObject actualUser;
    public Transform actualCamera;
    public Transform playerCapsule;
    private PlayerCharacterType platform;
    private Vector3 lastUserScale, lastCameraPosition, lastCameraUp, lastCameraForward, lastUserPosition = Vector3.one;
    private Quaternion lastUserRotation = Quaternion.identity, lastCameraRotation = Quaternion.identity;
    private XROrigin xrOrigin;
    public Image fadeImage;
    public float fadeDuration = 1.0f;
    public static event EventHandler<bool> OnMove2User;
    public static event Action<ulong> OnInvitationReceived;
    public static event Action OnRoomClosingWarning;
    public static event Action<ulong, int> OnAnnotationInviteReceived;

    private void Awake()
    {
        actualUser = gameObject;
        CheckPlatform();

        if (platform.Equals(PlayerCharacterType.VR))
        {
            xrOrigin = GetComponentInChildren<XROrigin>(true);
            if (xrOrigin == null)
                Debug.LogWarning("No XROrigin found.");
        }
    }

    void Start()
    {
        //Init last pos reference system
        Transform refT = GameObject.Find("SpawnPoint")?.transform;

        if (refT != null)
        {
            lastUserPosition = refT.position;
            lastUserRotation = refT.rotation;
            lastUserScale = Vector3.one;
            lastCameraPosition = new Vector3(refT.position.x, 1.6f, refT.position.z);
        }
        
        if (playerCapsule == null)
            playerCapsule = GameObject.Find("PlayerCapsule")?.transform;
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        CharacterController cc = GetComponent<CharacterController>();
        XRInputModalityManager modalityManager = GetComponent<XRInputModalityManager>();
        
        if (IsOwner)
        {
            if (actualCamera != null)
            {
                actualCamera.gameObject.SetActive(true);
                Camera cam = actualCamera.GetComponent<Camera>();
                if (cam != null)
                    cam.enabled = true;

                AudioListener audio = actualCamera.GetComponent<AudioListener>();
                if (audio != null)
                    audio.enabled = true;

                var legacyTpd = actualCamera.GetComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>();
                if (legacyTpd != null)
                    legacyTpd.enabled = true;

                var newTpd = actualCamera.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
                if (newTpd != null)
                    newTpd.enabled = true;
            }

            if (xrOrigin != null)
            {
                xrOrigin.gameObject.SetActive(true);
                xrOrigin.enabled = true;
                xrOrigin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Floor;
            }
            
            if (cc != null)
                cc.enabled = true;
            
            if (modalityManager != null)
                modalityManager.enabled = true;

            if (objectsToDisableForClones != null)
            {
                foreach (var obj in objectsToDisableForClones)
                    if (obj != null)
                        obj.SetActive(true);
            }

            if (componentsToDisableForClones != null)
            {
                foreach (var comp in componentsToDisableForClones)
                    if (comp != null)
                        comp.enabled = true;
            }
            
        }
        else
        {
            if (actualCamera != null)
            {
                Camera cam = actualCamera.GetComponent<Camera>();
                if (cam != null)
                    cam.enabled = false;

                AudioListener audio = actualCamera.GetComponent<AudioListener>();
                if (audio != null)
                    audio.enabled = false;
                
                var legacyTpd = actualCamera.GetComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>();
                if (legacyTpd != null)
                    legacyTpd.enabled = false;

                var newTpd = actualCamera.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
                if (newTpd != null)
                    newTpd.enabled = false;
            }

            if (xrOrigin != null)
            {
                xrOrigin.Camera.enabled = false;
                AudioListener xrAudio = xrOrigin.Camera.GetComponent<AudioListener>();
                if (xrAudio != null) xrAudio.enabled = false;
            }
            
            if (cc != null)
                cc.enabled = false;
            
            if (modalityManager != null)
                modalityManager.enabled = false;

            if (componentsToDisableForClones != null)
            {
                foreach (var comp in componentsToDisableForClones)
                    if (comp != null)
                        comp.enabled = false;
            }

            if (objectsToDisableForClones != null)
            {
                foreach (var obj in objectsToDisableForClones)
                    if (obj != null)
                        obj.SetActive(false);
            }
        }
    }
    
    void CheckPlatform()
    {
        if (name.Contains("Desktop"))
            platform = PlayerCharacterType.Desktop;
        else if (name.Contains("Mobile"))
            platform = PlayerCharacterType.Mobile;
        else if(name.Contains("AR"))
            platform = PlayerCharacterType.AR;
        else if (name.Contains("VR"))
            platform = PlayerCharacterType.VR;
    }
    
    [ContextMenu("MoveToUser")]
    public void MoveToUser(Transform userTransform, Transform cameraTransform, PlayerCharacterType userPlat)
    {
        bool amIXR = PlatformController.Instance.GetPlayerCharacterType() == PlayerCharacterType.VR ||
                     PlatformController.Instance.GetPlayerCharacterType() == PlayerCharacterType.AR;
        
        if (amIXR)
        {
            if (xrOrigin == null)
                return;

            lastUserScale = xrOrigin.transform.localScale;
            
            Vector3 myFloorPos = xrOrigin.Camera.transform.position;
            myFloorPos.y = xrOrigin.transform.position.y;
            lastUserPosition = myFloorPos;
            
            Vector3 myFwd = xrOrigin.Camera.transform.forward;
            myFwd.y = 0;
            lastCameraForward = myFwd.normalized != Vector3.zero ? myFwd.normalized : xrOrigin.transform.forward;
            
            Vector3 targetForwardFlattened = cameraTransform.forward;
            targetForwardFlattened.y = 0; 
            targetForwardFlattened.Normalize();

            if (targetForwardFlattened == Vector3.zero)
                targetForwardFlattened = userTransform.forward;
            
            Vector3 targetPosition = cameraTransform.position;
            targetPosition.y = userTransform.position.y;

            StartCoroutine(MoveUserCoroutine(userTransform.localScale, targetPosition,
                Vector3.up, targetForwardFlattened)); 
        }
        else
        {
            // Lógica para PC / Móvil (Yo soy PC/Móvil)
            lastUserScale = playerCapsule.localScale;
            lastUserPosition = playerCapsule.position;
            lastUserRotation = playerCapsule.rotation;
            lastCameraRotation = actualCamera.localRotation;
        
            playerCapsule.localScale = userTransform.localScale;

            if (userPlat == PlayerCharacterType.VR || userPlat == PlayerCharacterType.AR)
            {
                Vector3 forwardProjected = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
            
                if (forwardProjected != Vector3.zero)
                {
                    playerCapsule.rotation = Quaternion.LookRotation(forwardProjected);
                }
                
                actualCamera.rotation = cameraTransform.rotation;

                Vector3 targetFloorPos = cameraTransform.position;
                targetFloorPos.y = userTransform.position.y;

                transform.position = targetFloorPos;
                playerCapsule.position = targetFloorPos;
            }
            else
            {
                playerCapsule.rotation = userTransform.rotation;
                playerCapsule.position = cameraTransform.position + (playerCapsule.position - actualCamera.position);
            }
            
            FirstPersonController fpc = GetComponentInChildren<FirstPersonController>();
            if (fpc != null)
                fpc.ForcePitchRotation(cameraTransform.eulerAngles.x);
            else
                actualCamera.rotation = cameraTransform.rotation;
        }
        
        OnMove2User?.Invoke(this, true);
    }
    
    public IEnumerator MoveUserCoroutine(Vector3 userScale, Vector3 posFeet, Vector3 cameraUp, Vector3 cameraForward)
    {
        MoveXR(userScale, cameraUp, cameraForward, posFeet);
        yield break;
    }
    
    void MoveXR(Vector3 scale, Vector3 up, Vector3 forward, Vector3 posFeet)
    {
        if (xrOrigin == null)
            return;
    
        xrOrigin.transform.localScale = scale;
    
        float targetYaw = Quaternion.LookRotation(forward, Vector3.up).eulerAngles.y;
        float cameraLocalYaw = xrOrigin.Camera.transform.localEulerAngles.y;
        float desiredOriginYaw = targetYaw - cameraLocalYaw;
        Quaternion targetRotation = Quaternion.Euler(0, desiredOriginYaw, 0);
    
        Vector3 cameraWorldOffset = xrOrigin.Camera.transform.position - xrOrigin.transform.position;
    
        Vector3 targetOriginPos = posFeet;
        targetOriginPos.x -= cameraWorldOffset.x;
        targetOriginPos.z -= cameraWorldOffset.z;

        TeleportationProvider teleportProvider = xrOrigin.GetComponentInChildren<TeleportationProvider>();
        if (teleportProvider == null)
            teleportProvider = FindAnyObjectByType<TeleportationProvider>();

        if (teleportProvider != null)
        {
            TeleportRequest request = new TeleportRequest()
            {
                destinationPosition = targetOriginPos,
                destinationRotation = targetRotation,
                matchOrientation = MatchOrientation.TargetUpAndForward
            };
        
            teleportProvider.QueueTeleportRequest(request);
        }
        else
        {
            xrOrigin.transform.rotation = targetRotation;
            xrOrigin.transform.position = targetOriginPos;
        }
    }
    
    public void BackToLastPos()
    {
        bool isXR = PlatformController.Instance.GetPlayerCharacterType() == PlayerCharacterType.VR ||
                    PlatformController.Instance.GetPlayerCharacterType() == PlayerCharacterType.AR;
        
        if (isXR && xrOrigin != null)
            StartCoroutine(MoveUserCoroutine(lastUserScale, lastUserPosition, Vector3.up, lastCameraForward));
        else
        {
            playerCapsule.localScale = lastUserScale;
            transform.position = lastUserPosition;
            playerCapsule.position = lastUserPosition;
            playerCapsule.rotation = lastUserRotation;
            
            FirstPersonController fpc = GetComponentInChildren<FirstPersonController>();
            if (fpc != null)
                fpc.ForcePitchRotation(lastCameraRotation.eulerAngles.x);
            else
                actualCamera.localRotation = lastCameraRotation;
        }
    }
    
    void CopyXRTransform()
    {
        if (xrOrigin)
        {
            playerCapsule.position = xrOrigin.transform.position;
            playerCapsule.rotation = xrOrigin.transform.rotation;
            playerCapsule.localScale = xrOrigin.transform.localScale;
        }
    }
    
    public void SendTeleportInvitation(ulong targetClientId)
    {
        if (IsOwner)
            SendInvitationServerRpc(targetClientId);
    }

    [ServerRpc]
    private void SendInvitationServerRpc(ulong targetClientId)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { targetClientId } }
        };
        ReceiveInvitationClientRpc(OwnerClientId, clientRpcParams);
    }

    [ClientRpc]
    private void ReceiveInvitationClientRpc(ulong senderClientId, ClientRpcParams clientRpcParams = default)
    {
        OnInvitationReceived?.Invoke(senderClientId);
    }
    
    public void SendGlobalTeleportInvitation()
    {
        if (IsOwner)
            SendGlobalInvitationServerRpc();
    }

    [ServerRpc]
    private void SendGlobalInvitationServerRpc()
    {
        ReceiveGlobalInvitationClientRpc(OwnerClientId);
    }

    [ClientRpc]
    private void ReceiveGlobalInvitationClientRpc(ulong senderClientId)
    {
        ulong myClientId = NetworkManager.Singleton.LocalClientId;
        if (senderClientId == myClientId)
            return;

        InvokeOnInvitationReceived(senderClientId); 
    }

    private void InvokeOnInvitationReceived(ulong senderId)
    {
        OnInvitationReceived?.Invoke(senderId);
    }
    
    public void SendRoomCloseWarning()
    {
        if (IsOwner)
            SendRoomCloseWarningServerRpc();
    }

    [ServerRpc]
    private void SendRoomCloseWarningServerRpc()
    {
        ReceiveRoomCloseWarningClientRpc();
    }

    [ClientRpc]
    private void ReceiveRoomCloseWarningClientRpc()
    {
        OnRoomClosingWarning?.Invoke(); 
    }
    
    public void SendGlobalAnnotationInvitation(int annotationId)
    {
        if (IsOwner)
            SendGlobalAnnotationInvitationServerRpc(annotationId);
    }

    [ServerRpc]
    private void SendGlobalAnnotationInvitationServerRpc(int annotationId)
    {
        ReceiveGlobalAnnotationInvitationClientRpc(OwnerClientId, annotationId);
    }

    [ClientRpc]
    private void ReceiveGlobalAnnotationInvitationClientRpc(ulong senderClientId, int annotationId)
    {
        ulong myClientId = NetworkManager.Singleton.LocalClientId;
        if (senderClientId == myClientId)
            return;

        OnAnnotationInviteReceived?.Invoke(senderClientId, annotationId);
    }
    
    public PlayerCharacterType GetPlatform()
    {
        return platform;
    }
}