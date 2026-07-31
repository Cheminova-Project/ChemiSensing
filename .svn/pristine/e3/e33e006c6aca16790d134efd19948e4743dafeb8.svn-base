using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Clase base para personajes generales en la aplicación.
/// Proporciona funcionalidad común para personajes en diferentes plataformas (AR, VR, Desktop, Mobile).
/// </summary>
public class GeneralCharacter : MonoBehaviour
{
    [SerializeField] protected NetworkedPlayerCharacter m_NetworkedPlayerCharacter;
    [SerializeField] protected CharacterController m_CharacterController;

    [Header("Objects and components Management")]
    [Header("Not Owner")]
    [SerializeField] protected List<GameObject> m_ObjectsToDisableWhenNotOwner = new List<GameObject>();
    [SerializeField] protected List<Behaviour> m_ComponentsToDisableWhenNotOwner = new List<Behaviour>();
    [Header("Owner")]
    [SerializeField] protected List<GameObject> m_ObjectsToDisableWhenOwner = new List<GameObject>();
    [SerializeField] protected List<Behaviour> m_ComponentsToEnableWhenOwner = new List<Behaviour>();
    [SerializeField] protected List<GameObject> m_ObjectsToEnableWhenOwner = new List<GameObject>();
    
    public EventSystem ownEventSystem;
    public PlayerInput ownPlayerInput;

    protected virtual void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        m_NetworkedPlayerCharacter.OnPlayerSpawned.AddListener(OnNetworkSpawn);
    }

    private void OnDestroy()
    {
        m_NetworkedPlayerCharacter.OnPlayerSpawned.RemoveListener(OnNetworkSpawn);
    }

    public bool IsLocalPlayer => m_NetworkedPlayerCharacter.IsLocalPlayer;
    public virtual void ManageCharacterObjects()
    {
        //Si no es el dueño del personaje, manejo de objetos y componentes
        if (!m_NetworkedPlayerCharacter.IsLocalPlayer)
        {
            if (m_CharacterController)
                m_CharacterController.enabled = false;

            //Desactivar objetos
            foreach (var go in m_ObjectsToDisableWhenNotOwner)
            {
                if(go != null)
                    go.SetActive(false);
            }
                
            //Desactivar componentes
            foreach (var component in m_ComponentsToDisableWhenNotOwner)
            {
                if(component != null)
                    component.enabled = false;
            }


            enabled = false;
            if (ownPlayerInput)
            {
                ownPlayerInput.enabled = false;
                ownPlayerInput.DeactivateInput();
            }
            this.enabled = false;
        }
        //Si es el dueño del personaje, manejo de objetos y componentes
        else
        {
            //Desactivar objetos
            foreach (var go in m_ObjectsToDisableWhenOwner)
            {
                if(go != null)
                    go.SetActive(false);
            }
                

            if (m_CharacterController)
                m_CharacterController.enabled = true;
            
            //Activar objetos
            foreach (var go in m_ObjectsToEnableWhenOwner)
            {
                if(go != null)
                    go.SetActive(true);
            }

            //Activar componentes
            foreach (var component in m_ComponentsToEnableWhenOwner)
            {
                if(component != null)
                    component.enabled = true;
            }
                

            var eventSystems = FindObjectsByType<EventSystem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var eventSystem in eventSystems)
            {
                if(eventSystem != ownEventSystem)
                    eventSystem.enabled = false;
            }
            
            gameObject.layer = LayerMask.NameToLayer("OwnCharacter");
            if (ownPlayerInput)
            {
                ownPlayerInput.enabled = true;
                ownPlayerInput.ActivateInput();
            }
            
        }
    }
    
    public void OnNetworkSpawn()
    {
        ManageCharacterObjects();
    }
}

