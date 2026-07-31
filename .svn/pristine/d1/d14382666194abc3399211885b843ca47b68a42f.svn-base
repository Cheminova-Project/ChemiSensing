using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem.XR;



[RequireComponent(typeof(NetworkedPlayerCharacter))]
/// <summary>
/// Clase que maneja el comportamiento del personaje en AR.
/// Extiende GeneralCharacter para personalizar la gestión de objetos del personaje
/// según si es el jugador local o un jugador remoto.
public class ARCharacter : GeneralCharacter
{
    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// Gestiona los objetos del personaje según si es el jugador local o un jugador remoto.
    /// </summary>
    public override void ManageCharacterObjects()
    {
        if (m_NetworkedPlayerCharacter.IsLocalPlayer)
        {
            //Cosas que hacer cuando es el jugador local
        }
        else
        {
            //Cosas que hacer cuando no es el jugador local
            var trackedposedrivers_inputsystem = GetComponentsInChildren<TrackedPoseDriver>();
            var trackedposedrivers = GetComponentsInChildren<UnityEngine.SpatialTracking.TrackedPoseDriver>();
        
            int trackedPoseDriversCount = trackedposedrivers_inputsystem.Length + trackedposedrivers.Length;

            foreach (var driver in trackedposedrivers_inputsystem)
            {
                trackedPoseDriversCount++;
                driver.enabled = false;
            }
            
            foreach (var driver in trackedposedrivers)
            {
                trackedPoseDriversCount++;
                driver.enabled = false;
            }
        
            DisableXRScriptsInChildren();
        }
        base.ManageCharacterObjects();
    }
    
    /// <summary>
    /// Desactiva todos los scripts relacionados con XR en los hijos del objeto.
    /// </summary>
    void DisableXRScriptsInChildren()
    {
        // Obtiene todos los MonoBehaviour en los hijos, incluyendo desactivados
        MonoBehaviour[] allScripts = GetComponentsInChildren<MonoBehaviour>(true);

        foreach (var script in allScripts)
        {
            var ns = script.GetType().Namespace;
            if (ns != null && script != null && ns.Contains("UnityEngine.XR.ARFoundation"))
            {
                script.enabled = false;
                //Debug.Log($"Desactivado: {script.GetType().Name} en {script.gameObject.name}");
            }
        }
    }
}

