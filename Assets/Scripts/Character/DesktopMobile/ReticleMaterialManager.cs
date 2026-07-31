using UnityEngine;

public class ReticleMaterialManager : MonoBehaviour
{
    /// <summary>
    /// Material del retículo que se va a gestionar.
    /// </summary>
    [SerializeField] private Material reticleMaterial;
    /// <summary>
    /// Color por defecto del retículo.
    /// </summary>
    [SerializeField] private Color defaultColor = Color.white;
    /// <summary>
    /// Color de resaltado del retículo.
    /// </summary>
    [SerializeField] private Color highlightColor = Color.yellow;

    private void Start()
    {
        if (reticleMaterial == null)
        {
            Debug.LogError("Reticle material is not assigned.");
            return;
        }
        SetDefaultColor();
    }

    /// <summary>
    /// Establece el color por defecto del retículo.
    /// </summary>
    public void SetDefaultColor()
    {
        if (reticleMaterial != null)
        {
            //Cambiamos SurfaceColor en el shader
            reticleMaterial.SetColor("_SurfaceColor", defaultColor);
        }
    }

    /// <summary>
    /// Establece el color de resaltado del retículo.
    /// </summary>
    public void SetHighlightColor()
    {
        if (reticleMaterial != null)
        {
            reticleMaterial.SetColor("_SurfaceColor", highlightColor);
        }
    }
}
