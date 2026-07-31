using System;
using UnityEngine;

/// <summary>
/// Permite desasociar el objeto de su padre en la jerarquía de la escena.
/// Útil para mover objetos a la raíz de la escena.
/// </summary>
public class NullParenter : MonoBehaviour
{
    /// <summary>
    /// Desasocia el objeto de su padre actual.
    /// </summary>
    public void Unparent()
    {
        transform.parent = null;
    }

    private void Awake()
    {
        transform.SetParent(null, true);
    }
}
