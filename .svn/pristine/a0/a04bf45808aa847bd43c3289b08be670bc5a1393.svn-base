using UnityEngine;

/// <summary>
/// Herramienta visual para señalar objetos o posiciones en la escena.
/// Permite mostrar un puntero gráfico en la interfaz o en el mundo 3D.
/// </summary>
public class VisualPointer : MonoBehaviour
{
    [Header("Marker Settings")]
    [SerializeField] private Material markerMaterial;
    [SerializeField] private float markerSize = 0.01f; // 1cm

    [Header("Auto-Hide Settings")]
    [SerializeField] private int frameTimeoutThreshold = 10; // Frames sin actualización antes de ocultar

    private GameObject markerQuad;
    private Material markerMaterialInstance;
    private int framesSinceLastUpdate = 0;

    private Camera mainCamera;
    public static VisualPointer Instance { get; private set; }

    private void Start()
    {
        SetupMarker();

        if (Instance != null && Instance != this)
        {
            if(Instance.isActiveAndEnabled)
            {
                Debug.LogError("Ya existe una instancia de VRPointer. Destruyendo duplicado.");
                Destroy(this.gameObject);
                return;
            }
        }

        Instance = this;
    }

    private void Update()
    {
        // Incrementar el contador de frames sin actualización
        framesSinceLastUpdate++;

        // Si ha pasado el threshold de frames, ocultar el marcador
        if (framesSinceLastUpdate >= frameTimeoutThreshold)
        {
            HideMarker();
        }
    }

    void OnDisable()
    {
        Instance = null;
    }

    private void SetupMarker()
    {
        // Crear un pequeño quad como marcador
        markerQuad = new GameObject("VRPointerMarker");
        markerQuad.transform.SetParent(transform);
        
        MeshFilter meshFilter = markerQuad.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = markerQuad.AddComponent<MeshRenderer>();
        
        // Crear un quad simple
        Mesh quad = CreateQuadMesh();
        meshFilter.mesh = quad;
        
        // Aplicar material (será asignado desde el inspector o usará uno por defecto)
        if (markerMaterial != null)
        {
            markerMaterialInstance = new Material(markerMaterial);
        }
        else
        {
            markerMaterialInstance = new Material(Shader.Find("Standard"));
        }
        meshRenderer.material = markerMaterialInstance;
        
        // Escala del marcador
        markerQuad.transform.localScale = Vector3.one * markerSize;
        
        // Desactivar al inicio
        markerQuad.SetActive(false);
    }

    private Mesh CreateQuadMesh()
    {
        Mesh mesh = new Mesh();
        
        // Vértices del quad (centrado en origen)
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(0.5f, 0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0)
        };
        
        // UVs
        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };
        
        // Triángulos (dos triángulos para formar el quad)
        int[] triangles = new int[6]
        {
            0, 2, 1,
            0, 3, 2
        };
        
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        
        return mesh;
    }

    /// <summary>
    /// Establece la posición del marcador y lo activa.
    /// </summary>
    public void SetMarkerPosition(Vector3 worldPosition)
    {
        // Resetear el contador de frames cuando se actualiza la posición
        framesSinceLastUpdate = 0;

        ShowMarker();
        if (markerQuad == null)
            return;

        // Posicionar el marcador en el punto
        markerQuad.transform.position = worldPosition;

        // Hacer que mire a la cámara si se proporciona
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if(mainCamera)
        {
            markerQuad.transform.LookAt(mainCamera.transform);
            //Invertimos, ya que el quad por defecto mira hacia adelante en su eje Z, en el espacio local
            markerQuad.transform.Rotate(0, 180f, 0, Space.Self);
        }
    }

    /// <summary>
    /// Oculta el marcador.
    /// </summary>
    public void HideMarker()
    {
        if (markerQuad != null && markerQuad.activeSelf)
            markerQuad.SetActive(false);
    }

    /// <summary>
    /// Muestra el marcador.
    /// </summary>
    public void ShowMarker()
    {
        if (markerQuad != null && !markerQuad.activeSelf)
            markerQuad.SetActive(true);
    }

    private void OnDestroy()
    {
        if (markerQuad != null)
            Destroy(markerQuad);
    }

    /// <summary>
    /// Referencia al objeto gráfico del puntero.
    /// </summary>
    public GameObject pointerGraphic;

    /// <summary>
    /// Muestra el puntero en la posición indicada.
    /// </summary>
    /// <param name="position">Posición donde mostrar el puntero.</param>
    public void ShowPointer(Vector3 position)
    {
        if (pointerGraphic != null)
        {
            pointerGraphic.transform.position = position;
            pointerGraphic.SetActive(true);
        }
    }

    /// <summary>
    /// Oculta el puntero visual.
    /// </summary>
    public void HidePointer()
    {
        if (pointerGraphic != null)
            pointerGraphic.SetActive(false);
    }
}
