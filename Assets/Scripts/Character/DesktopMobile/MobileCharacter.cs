using UnityEngine;
using Unity.Netcode;


[RequireComponent(typeof(NetworkedPlayerCharacter))]
/// <summary>
/// Clase que maneja el comportamiento del personaje en dispositivos móviles.
/// Extiende GeneralCharacter para personalizar la gestión de objetos del personaje
/// según si es el jugador local o un jugador remoto.
/// </summary>
public class MobileCharacter : GeneralCharacter
{
    protected override void Awake()
    {
        base.Awake();
        if(NetworkManager.Singleton == null)
            this.enabled = false; // Disable if no NetworkManager is present
    }
    public override void ManageCharacterObjects()
    {
        if(NetworkManager.Singleton == null)
            return;
        if (m_NetworkedPlayerCharacter.IsLocalPlayer)
        {
            //Cosas que hacer cuando es el jugador local
        }
        else
        {
            //Cosas que hacer cuando no es el jugador local
        }
        base.ManageCharacterObjects();
    }
}

