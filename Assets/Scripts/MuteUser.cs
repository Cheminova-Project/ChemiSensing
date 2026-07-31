using System.Linq;
using Adrenak.UniMic;
using UnityEngine;

/// <summary>
/// Componente de herramienta que gestiona la interfaz de control de silenciar al usuario.
/// </summary>
public class MuteUser : ToolComponent
{
    private Mic mic;
    protected override void OnToolActivatedInternal()
    {
        base.OnToolActivatedInternal();
        mic = Resources.FindObjectsOfTypeAll<Mic>().FirstOrDefault();
        if (mic != null)
            mic.enabled = false;
    }

    protected override void OnToolDeactivatedInternal()
    {
        base.OnToolDeactivatedInternal();
        if (mic != null)
            mic.enabled = true;
    }
}