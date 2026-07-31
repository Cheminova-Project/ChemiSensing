using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Manager para el menú de mundo VR. Permite activar/desactivar y posicionar el menú en el espacio VR.
/// </summary>
public class VRWorldMenuManager : MonoBehaviour
{
    /// <summary>
    /// Objeto objetivo a gestionar en el menú.
    /// </summary>
    public Transform targetObject;
    /// <summary>
    /// Offset de posición para el menú respecto a la cámara principal.
    /// </summary>
    public Vector3 offset;
    /// <summary>
    /// Cámara principal de la escena.
    /// </summary>
    public Camera mainCamera;
    /// <summary>
    /// Indica si el menú inicia activo.
    /// </summary>
    public bool startActive = true;
    private bool isActive;

    /// <summary>
    /// Inicializa el menú y lo activa/desactiva tras un breve retraso.
    /// </summary>
    void Start()
    {
        Invoke("ToggleActive", 0.2f); // Esperar un frame para asegurar que todo esté inicializado
    }

    /// <summary>
    /// Activa o desactiva el menú y reposiciona el objeto objetivo en el espacio VR.
    /// </summary>
    public void ToggleActive()
    {
        //Activaremos o desactivaremos todos los objetos y componentes gestionados, reubicándolos cuando se activen
        isActive = !isActive;
        if (targetObject != null)
        {
            targetObject.gameObject.SetActive(isActive);
            if (isActive)
            {
                //Aplicamos el offset LOCAL, pero no quiero tener en cuenta si el usuario está mirando hacia arriba o abajo, así que uso la forward del main camera pero solo en el plano XZ
                Vector3 forwardXZ = new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z).normalized;
                targetObject.position = mainCamera.transform.position + forwardXZ * offset.z + mainCamera.transform.right * offset.x + mainCamera.transform.up * offset.y;
                targetObject.rotation = Quaternion.LookRotation(targetObject.position - mainCamera.transform.position);
            }
        }
    }
}
