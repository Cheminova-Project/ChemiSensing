using UnityEngine;

public class WorldUIBehaviour : MonoBehaviour
{
    [Header("Configuración de seguimiento")]
    [Tooltip("Ángulo (grados) desde forward del jugador tras el cual la UI empezará a recentrarse")]
    public float activationAngle = 70f;
    [Tooltip("Distancia radial desde el player (eje horizontal)")]
    public float radius = -1;
    [Tooltip("Offset vertical relativo a la cabeza (metros)")]
    public float heightOffset = -0.15f;

    [Header("Suavizado")]
    public float positionSmoothTime = 0.25f; // suavizado para posición
    public float rotationSmoothTime = 0.12f; // suavizado para yaw
    public float minFollowSpeed = 0.5f;      // velocidad mínima cuando se empieza a mover
    public float maxFollowSpeed = 4f;        // velocidad máxima

    // estado interno
    Vector3 velocity = Vector3.zero;
    float currentYawVelocity = 0f;
    float targetYaw; // en grados

    BoxCollider boxCollider;
    Camera cam => Camera.main;
    bool isRadiusCalculated;
        Transform playerHead; // Asigna la cámara del XR Rig (Main Camera)

    void Start()
    {
        isRadiusCalculated = false;
    }
    void Update()
    {
        if (playerHead == null) return;
        if (!isRadiusCalculated)
        {
            boxCollider = GetComponent<BoxCollider>();
            radius = CalculateAutoDistance(boxCollider, cam);
            isRadiusCalculated = true;
        }
            
        // 1) ángulo entre forward (solo yaw) y la dirección hacia la UI (solo en XZ)
        Vector3 headForward = playerHead.forward;
        headForward.y = 0;
        headForward.Normalize();

        Vector3 dirToUI = (transform.position - playerHead.position);
        dirToUI.y = 0;
        if (dirToUI.sqrMagnitude < 0.0001f) dirToUI = headForward; // fallback
        dirToUI.Normalize();

        float angle = Vector3.Angle(headForward, dirToUI);

        // 2) calcula yaw objetivo (en grados) = yaw absoluto del player (world) -> + 0 para centrar en frente
        float playerYaw = Mathf.Atan2(playerHead.forward.x, playerHead.forward.z) * Mathf.Rad2Deg;
        float desiredYaw = playerYaw; // si quisieras un offset lateral podrías sumar aquí

        // Si estamos fuera del ángulo, actualizamos targetYaw; si no, mantenemos la posición actual
        if (angle > activationAngle)
        {
            // target yaw = player yaw (mantendremos UI delante del jugador en XZ)
            targetYaw = desiredYaw;
        }
        // si angle <= activationAngle no tocamos targetYaw -> la UI se queda donde está y no se pega

        // 3) convertir targetYaw -> target position en XY
        float yawRad = targetYaw * Mathf.Deg2Rad;
        Vector3 targetOffset = new Vector3(Mathf.Sin(yawRad), 0f, Mathf.Cos(yawRad)) * radius;
        Vector3 targetPos = playerHead.position + targetOffset;
        targetPos.y = playerHead.position.y + heightOffset;

        // 4) suavizar movimiento de posición (suavizado por smoothedamp)
        // para que la velocidad dependa de cuánto hay que moverse, mezclamos un factor
        // compute distance ratio
        float dist = Vector3.Distance(transform.position, targetPos);
        float t = Mathf.InverseLerp(0f, 2.5f, dist); // cuando esté lejos, más rápido (0..1)
        float smoothTime = Mathf.Lerp(positionSmoothTime, positionSmoothTime * 0.5f, t);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime, Mathf.Infinity, Time.deltaTime);

        // 5) rotación: look-at pero solo yaw, suavizado con SmoothDampAngle
        Vector3 lookDir = (transform.position - playerHead.position);
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.0001f)
        {
            float desiredLookYaw = Mathf.Atan2(lookDir.x, lookDir.z) * Mathf.Rad2Deg;
            float smoothYaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, desiredLookYaw, ref currentYawVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0, smoothYaw, 0);
        }
    }
    float CalculateAutoDistance(BoxCollider box, Camera cam, float padding = 1.2f)
    {
        // Medidas del UI en world space
        Vector3 size = box.bounds.size;
        float maxSize = Mathf.Max(size.x, size.y); // el lado más grande (ancho o alto)

        // Convertir FOV de la cámara a radianes
        float fovRad = cam.fieldOfView * Mathf.Deg2Rad;

        // Distancia necesaria para que ese tamaño quepa en la vista vertical
        float requiredDist = maxSize / (2f * Mathf.Tan(fovRad / 2f));

        return requiredDist * padding; // añade un pequeño margen
    }

    public void SetPlayerHead(GameObject playerObject)
    {
        Camera playerCamera = playerObject.GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            Debug.LogError("No se encontró una cámara en el objeto del jugador.");
            return;
        }
        else
        {
            Debug.Log("Cámara del jugador asignada correctamente.");
        }
        Transform headTransform = playerCamera.transform;
        if (headTransform != null)
            playerHead = headTransform;
    }

}
