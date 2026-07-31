using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class UIDocumentPointerFeedback : MonoBehaviour
{
    [SerializeField]
    private UIDocument uiDocument;

    private bool isPointerOverUI = false;

    // Convertimos Start en Corrutina para esperar un frame
    private IEnumerator Start()
    {
        if (uiDocument == null)
        {
            Debug.LogError("UIDocumentPointerFeedback: No UIDocument asignado en el inspector.", this);
            yield break;
        }

        // ESPERA: Damos tiempo a que UIDocumentManager se despierte y configure su Instance
        yield return null; 

        if (UIDocumentManager.Instance == null)
        {
            Debug.LogError("UIDocumentPointerFeedback: UIDocumentManager.Instance es NULL tras esperar un frame. ¿Está el Manager en la escena?", this);
            yield break;
        }

        RegisterCallbacks();

        // CORRECCIÓN: Usamos un método con nombre, no una lambda
        UIDocumentManager.Instance.onContextSwitched.AddListener(OnContextSwitchedHandler);
    }

    private void OnDestroy()
    {
        UnregisterCallbacks();

        // IMPORTANTE: Chequear null antes de desuscribir
        // Al cerrar el juego, el Manager puede haber muerto antes que este objeto
        if (UIDocumentManager.Instance != null)
        {
            UIDocumentManager.Instance.onContextSwitched.RemoveListener(OnContextSwitchedHandler);
        }
    }

    // Método dedicado para manejar el evento (evita el problema de las lambdas)
    private void OnContextSwitchedHandler()
    {
        UnregisterCallbacks();
        RegisterCallbacks();
    }

    private void RegisterCallbacks()
    {
        if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            uiDocument.rootVisualElement.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            uiDocument.rootVisualElement.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        }
    }

    private void UnregisterCallbacks()
    {
        if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            uiDocument.rootVisualElement.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            uiDocument.rootVisualElement.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
        }
    }

    // ... El resto de tus métodos (OnPointerMove, ConvertUIPositionToWorldPosition, OnPointerLeave) siguen igual ...
    // ... Copia aquí el resto de tu código original ...
    
    private void OnPointerMove(PointerMoveEvent evt)
    {
        if (uiDocument == null || uiDocument.rootVisualElement?.panel == null)
            return;

        Vector3 worldPosition = ConvertUIPositionToWorldPosition(evt.position);
        worldPosition += uiDocument.transform.forward * 0.01f;
        
        if (VisualPointer.Instance != null)
        {
            VisualPointer.Instance.SetMarkerPosition(worldPosition);
            isPointerOverUI = true;
        }
    }

    private Vector3 ConvertUIPositionToWorldPosition(Vector2 screenPosition)
    {
        var panelSettings = uiDocument.panelSettings;
        if (panelSettings == null) return Vector3.zero;

        Vector3 uiDocumentPos = uiDocument.transform.position;
        Vector3 uiDocumentScale = uiDocument.transform.localScale;
        Vector3 localUIPosition = new Vector3(screenPosition.x, screenPosition.y, 0);
        
        Vector3 scaledLocalPosition = new Vector3(
            localUIPosition.x * uiDocumentScale.x,
            localUIPosition.y * uiDocumentScale.y,
            localUIPosition.z * uiDocumentScale.z
        );

        return uiDocumentPos + scaledLocalPosition;
    }

    private void OnPointerLeave(PointerLeaveEvent evt)
    {
        if (isPointerOverUI && VisualPointer.Instance != null)
        {
            VisualPointer.Instance.HideMarker();
            isPointerOverUI = false;
        }
    }
}