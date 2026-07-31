using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraTouchController : ToolComponent
{
    private Vector3 startPos;
    public static Vector3 input = Vector3.zero; 
    private bool detectJoystickMovement = false; 
    
    private VisualElement joystickElement; 
    private VisualElement joystickKnob;
    
    [Header("Configuración Visual")]
    [SerializeField] private float size = 60; 
    [SerializeField] private VisualTreeAsset cameraTouchUXML; // Arrastra aquí CameraTouch.uxml
    [SerializeField] private StyleSheet cameraTouchUSS;     // Arrastra aquí CameraTouch.uss
    
    [Header("Configuración Input")]
    [SerializeField] private float sensitivity = 50; 
    [SerializeField] private float rotationMultiplier = 2.0f; // Multiplicador específico para rotación
    [SerializeField] private bool invertYAxis = true;
    
    VisualElement joystickContainer;
    VisualElement joystickTouchArea;
    private MobileFirstPersonController mobileFirstPersonController;
    
    private Vector2 currentInput = Vector2.zero;

    protected override void OnToolActivatedInternal()
    {
        base.OnToolActivatedInternal();
        Initialize();
    }

    protected override void OnToolDeactivatedInternal()
    {
        base.OnToolDeactivatedInternal();
        input = Vector3.zero;
        detectJoystickMovement = false;
        
        if (joystickElement != null && joystickElement.panel != null)
        {
            joystickElement.style.display = DisplayStyle.None;
            joystickKnob.style.translate = new StyleTranslate(new Translate(new Length(0, LengthUnit.Pixel), new Length(0, LengthUnit.Pixel)));
        }
        
        if (mobileFirstPersonController != null)
        {
            mobileFirstPersonController.isJoystickEnabled = false;
            
            if (!mobileFirstPersonController.isGyroEnabled)
                mobileFirstPersonController.EnableCameraMovement(false);
        }

        if (uIDocument != null && uIDocument.rootVisualElement != null)
        {
            VisualElement root = uIDocument.rootVisualElement;
            VisualElement auxWindow = root.Q<VisualElement>("tool-window");
            
            if (auxWindow != null)
            {
                if (joystickContainer != null && auxWindow.Contains(joystickContainer))
                    auxWindow.Remove(joystickContainer);
                
                if (cameraTouchUSS != null && auxWindow.styleSheets.Contains(cameraTouchUSS))
                    auxWindow.styleSheets.Remove(cameraTouchUSS);
            }
        }
    }

    private void Initialize()
    {
        if (uIDocument == null)
        {
            Debug.LogError("[CameraTouchController] ERROR CRÍTICO: 'uIDocument' es NULL. Asegúrate de que este script hereda correctamente y tiene acceso al UIDocument.");
            return;
        }

        VisualElement root = uIDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[CameraTouchController] ERROR: 'rootVisualElement' es NULL.");
            return;
        }

        string targetContainerName = "tool-window"; 
        VisualElement auxWindow = root.Q<VisualElement>(targetContainerName);
        if (auxWindow == null)
        {
            Debug.LogError($"[CameraTouchController] ERROR: No se encontró el VisualElement llamado '{targetContainerName}' en el UXML principal.");
            
            var oldWindow = root.Q<VisualElement>("tool-window");
            if (oldWindow != null)
            {
                Debug.LogWarning($"[CameraTouchController] AVISO: No encontré '{targetContainerName}' pero SÍ encontré 'tool-window'. ¿Quizás olvidaste cambiar el nombre en el UXML o en el código?");
            }
            else 
            {
                Debug.LogWarning("[CameraTouchController] PISTA: Verifica el atributo 'Name' en el UI Builder.");
            }
            return;
        }
        
        if (cameraTouchUXML == null)
        {
            Debug.LogError("[CameraTouchController] ERROR: La variable 'cameraTouchUXML' está vacía en el Inspector.");
            return;
        }
        auxWindow.style.display = DisplayStyle.Flex;
        joystickContainer = cameraTouchUXML.CloneTree();
        if (joystickContainer == null)
        {
            Debug.LogError("[CameraTouchController] ERROR: Falló al clonar el árbol UXML.");
            return;
        }

        joystickContainer.style.position = Position.Absolute;
        joystickContainer.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
        joystickContainer.style.height = new StyleLength(new Length(100, LengthUnit.Percent));

        joystickTouchArea = joystickContainer.Q<VisualElement>("CameraTouchArea");
        joystickElement = joystickContainer.Q("CameraOuterBorder"); 
        joystickKnob = joystickElement.Q("CameraKnob"); 

        if (joystickTouchArea == null || joystickElement == null || joystickKnob == null) return;

        joystickElement.style.width = size; 
        joystickElement.style.height = size; 
        joystickKnob.style.transformOrigin = new TransformOrigin(Length.Percent(100), 0, 0);

        if (cameraTouchUSS != null)
            auxWindow.styleSheets.Add(cameraTouchUSS); 

        auxWindow.Add(joystickContainer); 

        if (NetworkManager.Singleton != null)
        {
            if (LocalRegistry.Instance == null)
            {
                 Debug.LogError("[CameraTouchController] ERROR: LocalRegistry.Instance es NULL.");
                 return;
            }
            
            GameObject player = LocalRegistry.Instance.GetPlayerGameObject(NetworkManager.Singleton.LocalClientId);
            if (player != null)
            {
                mobileFirstPersonController = player.GetComponentInChildren<MobileFirstPersonController>();
                if (mobileFirstPersonController == null)
                {
                    Debug.LogError($"[CameraTouchController] ERROR: Se encontró el objeto jugador '{player.name}' pero NO tiene el componente 'MobileFirstPersonController' (o no está en los hijos).");
                }
                else
                {
                    mobileFirstPersonController.EnableCameraMovement(true);
                    mobileFirstPersonController.isJoystickEnabled = true;
                }
            }
            else
            {
                Debug.LogError("[CameraTouchController] ERROR: No se pudo encontrar el GameObject del jugador local.");
            }
        }
        else
        {
            Debug.LogError("[CameraTouchController] ERROR: NetworkManager.Singleton es null.");
            return;
        }

        joystickTouchArea.RegisterCallback<PointerDownEvent>((ev) => ShowJoystick(ev));
        joystickTouchArea.RegisterCallback<PointerMoveEvent>((ev) => UpdateJoystick(ev));
        joystickTouchArea.RegisterCallback<PointerUpEvent>((ev) => HideJoystick(ev));
        joystickTouchArea.RegisterCallback<PointerLeaveEvent>((ev) => HideJoystick(ev));
    }

    private void ShowJoystick(PointerDownEvent _ev)
    {
        detectJoystickMovement = true;
        Vector2 localPosition = _ev.localPosition;
        startPos = localPosition;
        
        joystickElement.style.left = localPosition.x - size / 2;
        joystickElement.style.top = localPosition.y - size / 2;
        joystickElement.style.display = DisplayStyle.Flex;
    }

    private void UpdateJoystick(PointerMoveEvent _ev)
    {
        if (detectJoystickMovement)
        {
            Vector2 localPosition = _ev.localPosition;
            float deltaX = localPosition.x - startPos.x;
            float deltaY = startPos.y - localPosition.y;
            input = new Vector3(deltaX, deltaY, 0);
            input = input.normalized;

            ApplySensitivity(ref input, deltaX, deltaY, sensitivity);
            
            currentInput = new Vector2(input.x, input.y);
            
            joystickKnob.style.translate = new StyleTranslate(new Translate(new Length(input.x * size / 2, LengthUnit.Pixel), new Length(-input.y * size / 2, LengthUnit.Pixel)));
        }
    }

    private void Update()
    {
        if (detectJoystickMovement && mobileFirstPersonController != null)
        {
            float finalY = invertYAxis ? -currentInput.y : currentInput.y;
            Vector2 finalInput = new Vector2(currentInput.x, finalY);
             mobileFirstPersonController.InputLook(finalInput * rotationMultiplier);
        }
    }

    private void HideJoystick(EventBase _ev) // Simplificado para aceptar Up y Leave
    {
        input = Vector3.zero;
        currentInput = Vector2.zero;
        detectJoystickMovement = false;
        joystickElement.style.display = DisplayStyle.None;
        joystickKnob.style.translate = new StyleTranslate(new Translate(new Length(0, LengthUnit.Pixel), new Length(0, LengthUnit.Pixel)));
        
        if (mobileFirstPersonController != null)
        {
             mobileFirstPersonController.InputLook(Vector2.zero);
        }
    }

    // Misma lógica de sensibilidad
    private static void ApplySensitivity(ref Vector3 input, float _deltaX, float _deltaY, float sensitivity)
    {
        if (Mathf.Abs(_deltaX) >= sensitivity || Mathf.Abs(_deltaY) >= sensitivity) { return; }

        if (_deltaX > 0) input.x = (_deltaX >= sensitivity) ? input.x : Mathf.Lerp(0f, 1f, _deltaX / sensitivity);
        else input.x = (_deltaX <= -sensitivity) ? input.x : Mathf.Lerp(0f, -1f, _deltaX / -sensitivity);

        if (_deltaY > 0) input.y = (_deltaY >= sensitivity) ? input.y : Mathf.Lerp(0f, 1f, _deltaY / sensitivity);
        else input.y = (_deltaY <= -sensitivity) ? input.y : Mathf.Lerp(0f, -1f, _deltaY / -sensitivity);
    }
    
    private void OnDestroy()
    {
        // Garantiza que al cerrar el menú y destruir el script, la cámara se bloquee
        OnToolDeactivatedInternal(); 
    }
}