using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// This scripts get attached UI Document object's UIDocument component and accesses its root then renders joystick in the root

public class JoystickController : ToolComponent
{
    private Vector3 startPos;
    public static Vector3 input = Vector3.zero; // it is base input to publish it anywhere to use for know which direction and how strenght I move my finger.
    private bool detectJoystickMovement = false; // you can assume it as a bug fixer flag. App triggers <PointerMoveEvent> for once at the ver first frame of app for a reason I dont know why. So this flag is to prevent it happen
    private VisualElement joystickElement; // joystick itself (parent joystick element) it will be used to show and hide joystick by changing its style (display: none | flex)
    private VisualElement joystickKnob; // inner circle of joystick, dynamic moving part
    [SerializeField] private float size = 60; // size(width and height) of joystick element, modify it if you want
    [SerializeField] private float sensitivity = 50; // the higher, the more sensitive. 0 means sudden switches between directions(no sensitivity)
    [SerializeField] private VisualTreeAsset joystickUXML; // Joystick.uxml file
    [SerializeField] private StyleSheet joystickUSS; // Joystick.uss file
    [SerializeField] private float magnitudeMultiplier = 0.5f;
    VisualElement joystickContainer;
    VisualElement joystickTouchArea;
    private MobileFirstPersonController mobileFirstPersonController;
    
    // Variables para mantener el estado del joystick
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
        currentInput = Vector2.zero;
        detectJoystickMovement = false;
        if (joystickElement != null && joystickElement.panel != null)
        {
            joystickElement.style.display = DisplayStyle.None;
            joystickKnob.style.translate = new StyleTranslate(new Translate(new Length(0, LengthUnit.Pixel), new Length(0, LengthUnit.Pixel)));
        }

        //Eliminar el joystick de aux-window
        if (uIDocument != null && uIDocument.rootVisualElement != null)
        {
            VisualElement auxWindow = uIDocument.rootVisualElement.Q<VisualElement>("aux-window");
            if (auxWindow != null && joystickContainer != null && auxWindow.Contains(joystickContainer))
            {
                auxWindow.Remove(joystickContainer);
                
                if (auxWindow.childCount == 0)
                    auxWindow.style.display = DisplayStyle.None;
            }
        }
    }
    private void Initialize()
    {
        VisualElement root = uIDocument.rootVisualElement;
        VisualElement auxWindow = root.Q<VisualElement>("aux-window");
        if (auxWindow != null)
            auxWindow.style.display = DisplayStyle.Flex;
        joystickContainer = joystickUXML.CloneTree();
        joystickContainer.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
        joystickContainer.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
        joystickContainer.styleSheets.Add(joystickUSS);
        joystickTouchArea = joystickContainer.Q<VisualElement>("JoystickTouchArea");
        joystickElement = joystickContainer.Q("JoystickOuterBorder"); // There is a parent node named "JoystickOuterBorder" in Joystick.uxml file, just leave it as it is, you will need this variable to show/hide joystick later
        joystickKnob = joystickElement.Q("JoystickKnob"); // There is a child node named "JoystickKnob" in Joystick.uxml file, just leave it as it is, you will need this variable to move the little circle on the middle of the joystick later

        joystickElement.style.width = size; // applying width of joystick
        joystickElement.style.height = size; // applying height of joystick

        joystickElement.style.display = DisplayStyle.None;
        joystickElement.style.position = Position.Absolute;

        auxWindow.styleSheets.Add(joystickUSS); // add joystick uss file to aux-window, it is needed to apply joystick styles
        auxWindow.Add(joystickContainer); // add complete joystick UI as child of aux-window

        if (NetworkManager.Singleton != null)
        {
            GameObject player = LocalRegistry.Instance.GetPlayerGameObject(NetworkManager.Singleton.LocalClientId);
            mobileFirstPersonController = player.GetComponentInChildren<MobileFirstPersonController>();
            if (mobileFirstPersonController == null)
            {
                Debug.LogError("MobileFirstPersonController not found on the player GameObject.");
            }
        }
        else
        {
            Debug.LogError("NetworkManager.Singleton is null. Ensure that the NetworkManager is properly set up in the scene.");
            return;
        }

        joystickTouchArea.RegisterCallback<PointerDownEvent>((ev) =>
        {
            ShowJoystick(ev);
        });

        joystickTouchArea.RegisterCallback<PointerMoveEvent>((ev) =>
        {
            UpdateJoystick(ev);
        });

        joystickTouchArea.RegisterCallback<PointerUpEvent>((ev) =>
        {
            HideJoystick(ev);
        });

        joystickTouchArea.RegisterCallback<PointerLeaveEvent>((ev) =>
        {
            HideJoystick(ev);
        });

    }

    private void ShowJoystick(PointerDownEvent _ev)
    {
        detectJoystickMovement = true;
        joystickTouchArea.CapturePointer(_ev.pointerId);
        
        Vector2 localPosition = _ev.localPosition;
        startPos = localPosition;
        
        joystickElement.style.position = Position.Absolute;
        joystickElement.style.left = localPosition.x - (size / 2f);
        joystickElement.style.top = localPosition.y - (size / 2f);
        joystickElement.style.display = DisplayStyle.Flex;
    }

    private void UpdateJoystick(PointerMoveEvent _ev)
    {
        if (detectJoystickMovement)
        {
            Vector2 localPosition = _ev.localPosition;
            
            float deltaX = localPosition.x - startPos.x;
            float deltaY = localPosition.y - startPos.y;
            
            Vector2 delta = new Vector2(deltaX, deltaY);
            
            float maxRadius = size / 2f;
            
            Vector2 clampedDelta = Vector2.ClampMagnitude(delta, maxRadius);
            
            currentInput = new Vector2(clampedDelta.x / maxRadius, -clampedDelta.y / maxRadius);
            input = new Vector3(currentInput.x, currentInput.y, 0);
            
            joystickKnob.style.translate = new StyleTranslate(new Translate(new Length(clampedDelta.x, LengthUnit.Pixel), new Length(clampedDelta.y, LengthUnit.Pixel)));
        }
    }

    private void Update()
    {
        // Enviar input continuamente mientras el joystick esté activo
        if (detectJoystickMovement && mobileFirstPersonController != null)
        {
            mobileFirstPersonController.InputMove(currentInput * magnitudeMultiplier);
        }
    }

    private void HideJoystick(PointerUpEvent _ev)
    {
        input = Vector3.zero;
        currentInput = Vector2.zero; 
        detectJoystickMovement = false;
        
        joystickTouchArea.ReleasePointer(_ev.pointerId);
        
        joystickElement.style.display = DisplayStyle.None;
        joystickKnob.style.translate = new StyleTranslate(new Translate(new Length(0, LengthUnit.Pixel), new Length(0, LengthUnit.Pixel)));
        
        if (mobileFirstPersonController != null)
            mobileFirstPersonController.InputMove(Vector2.zero);
    }

    private void HideJoystick(PointerLeaveEvent _ev)
    {
        input = Vector3.zero;
        currentInput = Vector2.zero; 
        detectJoystickMovement = false;

        joystickTouchArea.ReleasePointer(_ev.pointerId); 

        joystickElement.style.display = DisplayStyle.None;
        joystickKnob.style.translate = new StyleTranslate(new Translate(new Length(0, LengthUnit.Pixel), new Length(0, LengthUnit.Pixel)));
        
        if (mobileFirstPersonController != null)
            mobileFirstPersonController.InputMove(Vector2.zero);
    }
}