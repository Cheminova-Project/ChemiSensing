using UnityEngine;

public class ClickDebugger : MonoBehaviour
{
    [Header("Camera")]
    public Camera mainCamera;
    
    [Header("Input Actions")]
    public PointerInputActionReferences pointerInputActions;
    
    [Header("Raycast Settings")]
    public LayerMask layerMask = -1; // Todas las capas por defecto
    public float maxDistance = 1000f;
    public bool showRayInScene = true;
    
    [Header("Debug Options")]
    public bool logAllHits = false; // Si quieres ver todos los hits del raycast
    public bool logRayInfo = true;  // Si quieres ver info del rayo

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            Debug.LogError("[ClickDebugger] No se encontró cámara principal!");

        if (pointerInputActions == null)
            Debug.LogError("[ClickDebugger] No se asignaron las acciones de entrada para el puntero!");

        pointerInputActions.m_pointerPositon.action.Enable();
        pointerInputActions.m_pointerPress.action.Enable();

        pointerInputActions.m_pointerPositon.action.performed += ctx =>
        {
            Vector2 pos = ctx.ReadValue<Vector2>();
            Debug.Log($"[Input] Pointer Position: {pos}");
        };
         
        pointerInputActions.m_pointerPress.action.performed += ctx =>
        {
            Debug.Log("[Input] Pointer Pressed");
        
        };
    }

    void Update()
    {
        // Verificar que tenemos todo lo necesario
        if (pointerInputActions == null ||
            pointerInputActions.PointerPress.action == null ||
            pointerInputActions.PointerPosition.action == null ||
            mainCamera == null)
            return;

        bool pointerPressed = pointerInputActions.PointerPress.action.WasPressedThisFrame();
        Vector2 pointerPosition = pointerInputActions.PointerPosition.action.ReadValue<Vector2>();

        if (pointerPressed)
        {
            Debug.Log("=== CLICK DETECTED (New Input System) ===");
            Debug.Log($"Pointer Position: {pointerPosition}");

            PerformDetailedPhysicsRaycast(pointerPosition);
        }
        
        // Opcional: mostrar raycast continuo para debugging visual
        if (showRayInScene)
        {
            Vector2 currentPointerPos = pointerInputActions.PointerPosition.action.ReadValue<Vector2>();
            Ray ray = mainCamera.ScreenPointToRay(currentPointerPos);
            Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red, 0.1f);
        }
    }

    void PerformDetailedPhysicsRaycast(Vector2 pointerPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(pointerPosition);
        
        if (logRayInfo)
        {
            Debug.Log($"[Ray Info] Origin: {ray.origin}, Direction: {ray.direction}");
            Debug.Log($"[Ray Info] LayerMask: {layerMask.value}, MaxDistance: {maxDistance}");
        }

        if (logAllHits)
        {
            // Obtener TODOS los hits del raycast
            RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, layerMask);
            
            if (hits.Length > 0)
            {
                Debug.Log($"[Physics] Total hits: {hits.Length}");
                
                // Ordenar por distancia
                System.Array.Sort(hits, (hit1, hit2) => hit1.distance.CompareTo(hit2.distance));
                
                for (int i = 0; i < hits.Length; i++)
                {
                    var hit = hits[i];
                    Debug.Log($"[Physics Hit {i+1}] " +
                             $"Object: '{hit.collider.gameObject.name}' | " +
                             $"Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)} ({hit.collider.gameObject.layer}) | " +
                             $"Distance: {hit.distance:F2} | " +
                             $"Point: {hit.point} | " +
                             $"Collider Type: {hit.collider.GetType().Name}");
                             
                    if (hit.rigidbody != null)
                        Debug.Log($"    └─ Rigidbody: {hit.rigidbody.name}");
                    if (hit.transform.parent != null)
                        Debug.Log($"    └─ Parent: {hit.transform.parent.name}");
                }
            }
            else
            {
                Debug.Log("[Physics] No hits detected with RaycastAll.");
            }
        }
        else
        {
            // Solo el primer hit
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, layerMask))
            {
                Debug.Log($"[Physics Hit] " +
                         $"Object: '{hit.collider.gameObject.name}' | " +
                         $"Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)} ({hit.collider.gameObject.layer}) | " +
                         $"Distance: {hit.distance:F2} | " +
                         $"Point: {hit.point} | " +
                         $"Collider Type: {hit.collider.GetType().Name}");
                         
                if (hit.rigidbody != null)
                    Debug.Log($"    └─ Rigidbody: {hit.rigidbody.name}");
                if (hit.transform.parent != null)
                    Debug.Log($"    └─ Parent: {hit.transform.parent.name}");
                    
                // Información adicional del GameObject
                var components = hit.collider.GetComponents<Component>();
                Debug.Log($"    └─ Components: {string.Join(", ", System.Array.ConvertAll(components, c => c.GetType().Name))}");
            }
            else
            {
                Debug.Log("[Physics] No hit detected.");
            }
        }
        
        Debug.Log("=== END CLICK DEBUG ===\n");
    }
}
