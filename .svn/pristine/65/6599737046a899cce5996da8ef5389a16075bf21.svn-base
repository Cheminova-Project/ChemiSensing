using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

[RequireComponent(typeof(Canvas))]
public class CanvasToWorldConverter : MonoBehaviour
{
    [Header("Configuración")]
    public float distanceFromCamera = 2f;

    [Header("Auto convert on Start")]
    public bool convertOnStart = true;

    private Camera mainCamera;
    private Canvas canvas;

    void Start()
    {
        if (convertOnStart)
        {
            ConvertToWorldSpace();
        }
    }

    public void ConvertCanvas(PlayerCharacterType playerCharacterType)
    {
        if (playerCharacterType == PlayerCharacterType.VR)
        {
            Debug.Log("Convirtiendo Canvas a espacio mundial para VR");
            ConvertToWorldSpace();
        }
    }

    public void ConvertToWorldSpace()
    {
        mainCamera = Camera.main;
        if (!mainCamera) return;

        canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = mainCamera;

        RectTransform rt = canvas.GetComponent<RectTransform>();

        // Obtener la resolución del canvas (píxeles)
        float screenWidth = mainCamera.pixelWidth;
        float screenHeight = mainCamera.pixelHeight;

        // Calcular tamaño en metros que cubre la cámara a cierta distancia
        float verticalFOV = mainCamera.fieldOfView;
        float aspect = mainCamera.aspect;

        // Altura visible a cierta distancia
        float heightAtDistance = 2f * distanceFromCamera * Mathf.Tan(0.5f * verticalFOV * Mathf.Deg2Rad);
        float widthAtDistance = heightAtDistance * aspect;

        // Actualizar tamaño del RectTransform para que coincida con el área visible
        rt.sizeDelta = new Vector2(screenWidth, screenHeight);

        // Calcular escala para que 1 unidad = 1 metro
        float scaleX = widthAtDistance / screenWidth;
        float scaleY = heightAtDistance / screenHeight;
        rt.localScale = new Vector3(scaleX, scaleY, 1f);

        LazyFollow lazyFollow = canvas.gameObject.AddComponent<LazyFollow>();
        lazyFollow.targetOffset = new Vector3(0, 0, distanceFromCamera);
        
        //Añadiremos un box collider del tamaño del canvas
        //BoxCollider boxCollider = canvas.gameObject.AddComponent<BoxCollider>();
        //boxCollider.size = new Vector3(rt.sizeDelta.x * scaleX, rt.sizeDelta.y * scaleY, 0.1f); // Un poco de grosor para el collider
        
    }
    
    public void ConvertToOverlaySpace()
    {
        canvas = GetComponent<Canvas>();
        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = null; // No se necesita cámara para ScreenSpaceOverlay
            Debug.Log("Canvas convertido a espacio de superposición");
        }
        else
        {
            Debug.LogWarning("El Canvas ya está en modo ScreenSpaceOverlay");
        }
    }
}