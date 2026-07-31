/*using TransformHandles.Utils;
using UnityEngine;

/// <summary>
/// Controlador para el collider de tipo tubo utilizado en los handles personalizados.
/// Permite gestionar eventos y propiedades específicas del collider de tubo.
/// </summary>
public class TubeColliderController : MonoBehaviour
{
    #if UNITY_EDITOR
    [SerializeField] private float height;
    [SerializeField] private int sideCount;
    [SerializeField] private float topRadius;
    [SerializeField] private float bottomThickness;
    [SerializeField] private float topThickness;
    [SerializeField] private float bottomRadius;

    [SerializeField] private bool save;

    private MeshCollider _meshCollider;
    private MeshFilter _meshFilter;

    #endif

    /// <summary>
    /// Referencia al componente Collider asociado.
    /// </summary>
    public Collider tubeCollider;

    private void Awake()
    {
        tubeCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        UpdateCollider();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            UpdateCollider();
        }
    }

    private void UpdateCollider()
    {
        var newMesh = MeshUtils.CreateTube(height, sideCount, bottomRadius, bottomThickness, topRadius, topThickness);
		
        newMesh.name = "tube";
		
        _meshFilter.sharedMesh = newMesh;
        //_meshCollider.sharedMesh = newMesh;

        if (save)
        {
            AssetDatabase.CreateAsset(newMesh, "Assets/tube.asset");
        }
    }

    /// <summary>
    /// Activa el collider de tubo.
    /// </summary>
    public void EnableCollider()
    {
        if (tubeCollider != null)
            tubeCollider.enabled = true;
    }

    /// <summary>
    /// Desactiva el collider de tubo.
    /// </summary>
    public void DisableCollider()
    {
        if (tubeCollider != null)
            tubeCollider.enabled = false;
    }
}*/