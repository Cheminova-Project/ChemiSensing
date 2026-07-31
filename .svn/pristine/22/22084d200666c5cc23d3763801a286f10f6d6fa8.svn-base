using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation.Samples
{
    /// <summary>
    /// Controlador que gestiona eventos de raycast en AR y dispara eventos personalizados.
    /// Permite conectar raycasts de AR Foundation con eventos definidos en assets.
    /// </summary>
    public class RaycastEventController : MonoBehaviour
    {
        static List<ARRaycastHit> s_Hits = new();
        static Ray s_RaycastRay;

        [SerializeField]
        [Tooltip("The active XR Origin in the scene.")]
        XROrigin m_XROrigin;

        [SerializeField]
        [Tooltip("The active AR Raycast Manager in the scene.")]
        ARRaycastManager m_RaycastManager;

        [SerializeField]
        [Tooltip("The Input Action References to use. You can create this by right clicking in the Project Window " +
             "and going to <b>XR</b> > AR Foundation > Input Action References.")]
        PointerInputActionReferences inputActionReferences;

        [SerializeField]
        [Tooltip("Event to raise if anything was hit by the raycast.")]
        ARRaycastHitEventAsset m_ARRaycastHitEvent;

        [SerializeField]
        [Tooltip("The type of trackable the raycast will hit.")]
        TrackableType m_TrackableType = TrackableType.PlaneWithinPolygon;
        

        /// <summary>
        /// Asset de evento de raycast AR al que se notificará cuando ocurra un impacto.
        /// </summary>
        public ARRaycastHitEventAsset raycastHitEventAsset
        {
            get => m_ARRaycastHitEvent;
            set => m_ARRaycastHitEvent = value;
        }

        /// <summary>
        /// El tipo de seguimiento que el raycast impactará.
        /// </summary>
        public TrackableType trackableType
        {
            get => m_TrackableType;
            set => m_TrackableType = value;
        }
   

        Camera m_Camera;
        LayerMask m_UILayerMask;
        RaycastHit[] m_UIRaycastHits = new RaycastHit[1];

        void Awake()
        {
            m_Camera = Camera.main;
            var uiLayer = LayerMask.NameToLayer("UI");
            m_UILayerMask = 1 << uiLayer;
        }

        void OnEnable()
        {
            if (m_RaycastManager == null || m_XROrigin == null || inputActionReferences == null)
            {
                Debug.LogWarning($"{nameof(RaycastEventController)} component on {name} has null inputs and will have no effect in this scene.", this);
                return;
            }

            if (inputActionReferences.m_pointerPress.action != null)
            {
                inputActionReferences.m_pointerPress.action.performed += ScreenTapped;
                inputActionReferences.m_pointerPress.action.canceled += ScreenTapped;
                inputActionReferences.m_pointerPositon.action.performed += TapPositionChanged;
            }
                
            else
            {
                Debug.LogError("Input actions are incorrectly configured. Expected a Screen Tap binding.", this);
            }
        }

        private void TapPositionChanged(InputAction.CallbackContext obj)
        {
            Debug.Log("Tap Position Changed: " + obj.ReadValue<Vector2>());
        }


        void OnDisable()
        {
            if (inputActionReferences == null)
                return;

            if (inputActionReferences.m_pointerPress.action != null)
            {
                inputActionReferences.m_pointerPress.action.performed -= ScreenTapped;
                inputActionReferences.m_pointerPositon.action.performed -= TapPositionChanged;
            }
                
        }

        void ScreenTapped(InputAction.CallbackContext context)
        {
            if (context.control.device is not Pointer pointer)
            {
               Debug.LogError("Input actions are incorrectly configured. Expected a Pointer binding ScreenTapped.", this);
               return;
            }

            var tapPosition = pointer.position.ReadValue();
            if (m_ARRaycastHitEvent != null &&
                m_RaycastManager.Raycast(tapPosition, s_Hits, m_TrackableType))
            {
                m_ARRaycastHitEvent.Raise(s_Hits[0]);
            }
        }


        /// <summary>
        /// Automatically initialize serialized fields when component is first added.
        /// </summary>
        void Reset()
        {
            m_RaycastManager = GetComponent<ARRaycastManager>();
            m_XROrigin = GetComponent<XROrigin>();

            if (m_XROrigin == null)
            {
#if UNITY_2023_1_OR_NEWER
                m_XROrigin = FindAnyObjectByType<XROrigin>();
#else
                m_XROrigin = FindObjectOfType<XROrigin>();
#endif
            }

            if (m_RaycastManager == null)
            {
#if UNITY_2023_1_OR_NEWER
                m_RaycastManager = FindAnyObjectByType<ARRaycastManager>();
#else
                m_RaycastManager = FindObjectOfType<ARRaycastManager>();
#endif
            }
        }
    }
}
