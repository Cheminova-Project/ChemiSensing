using System.Diagnostics;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine;

/// <summary>
/// Clase que extiende XRGrabInteractable para manejar la interacción de agarre en un entorno de red.
/// Verifica si el objeto está siendo manipulado por otro usuario antes de permitir el agarre
/// y gestiona los eventos de selección para actualizar el estado de manipulación.
/// </summary>
public class NetworkedGrabInteractable : XRGrabInteractable
{
    public ManipulableObject manipulableObject;

    /// <summary>
    /// Cuando un usuario intenta seleccionar (agarrar) el objeto.
    /// Si el objeto ya está siendo manipulado por otro usuario, se muestra un mensaje de error y se evita la selección.
    /// </summary>
    
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        // Comprobar si el objeto está siendo manipulado por otro usuario, PERO evitar mandar el mensaje si es el mismo usuario quien lo está manipulando
        if (manipulableObject.IsBeingManipulated() && manipulableObject.OwnerClientId != NetworkManager.Singleton.LocalClientId)
        {
            UnityEngine.Debug.Log("[ONSELECTENTERED] Object is being manipulated by another user. ClientId: " + manipulableObject.OwnerClientId + ", LocalClientId: " + NetworkManager.Singleton.LocalClientId);
            if (ToolMessageHandler.Instance != null)
                ToolMessageHandler.Instance.ShowMessage("This object is being manipulated by another user.", 4f, MessageType.Error);

            //Forzamos el release del objeto, a ver cómo se comporta
            base.OnSelectExited(new SelectExitEventArgs
            {
                interactorObject = args.interactorObject,
                interactableObject = args.interactableObject,
                manager = args.manager
            });

            return;
        }
        UnityEngine.Debug.Log("NetworkedGrabInteractable: OnSelectEntered called by client " + NetworkManager.Singleton.LocalClientId);
        manipulableObject.OnSelected();
        base.OnSelectEntered(args);
    }
    
    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        manipulableObject.OnDeselected();
        base.OnSelectExited(args);
    }

}