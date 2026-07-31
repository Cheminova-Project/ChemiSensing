using UnityEngine;

/// <summary>
/// Controla el shader de la caja de feedback en la UI.
/// Permite modificar efectos visuales y animaciones del feedback.
/// </summary>
public class UIFeedbackBoxShaderController : MonoBehaviour
{
    [Header("Height Fade")]
    [SerializeField] [Range(0, 100)] private float heightFadeThreshold = 100f;
    
    private Material shaderMaterial;
    private MeshRenderer meshRenderer;
    private float objectHeight;

    public void Initialize(float fadeThreshold = 100f)
    {
        InitializeMaterial();
        CalculateObjectHeight();
        SetHeightFadeThreshold(fadeThreshold);
    }

    private void InitializeMaterial()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("[UIFeedbackBoxShaderController] No se encontró MeshRenderer");
            return;
        }

        // ✅ Obtener el material ya asignado
        shaderMaterial = meshRenderer.material;
        Debug.Log("[UIFeedbackBoxShaderController] Material obtenido correctamente");
    }

    private void CalculateObjectHeight()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.mesh == null)
        {
            Debug.LogError("[UIFeedbackBoxShaderController] No se encontró Mesh");
            return;
        }

        Bounds bounds = meshFilter.mesh.bounds;
        objectHeight = bounds.size.y;
        Debug.Log($"[UIFeedbackBoxShaderController] Altura calculada: {objectHeight:F3}m");
    }

    private void OnValidate()
    {
        if (!isActiveAndEnabled || shaderMaterial == null)
            return;

        shaderMaterial.SetFloat("_HeightFadeThreshold", heightFadeThreshold);
    }

    public void SetHeightFadeThreshold(float threshold)
    {
        heightFadeThreshold = Mathf.Clamp(threshold, 0, 100);
        if (shaderMaterial != null)
            shaderMaterial.SetFloat("_HeightFadeThreshold", heightFadeThreshold);
    }

    public float GetHeightFadeThreshold() => heightFadeThreshold;
    public float GetObjectHeight() => objectHeight;
}