using System;
using UnityEngine;

namespace TransformHandles
{
    public class 
        Handle : MonoBehaviour
    {
        [SerializeField] private float autoScaleSizeInPixels = 192;
        [SerializeField] public bool autoScale;
        
        public virtual event Action<Handle> OnInteractionStartEvent;
        public virtual event Action<Handle> OnInteractionEvent;
        public virtual event Action<Handle> OnInteractionEndEvent;
        public virtual event Action<Handle> OnHandleDestroyedEvent; 

        public Transform target;
        public HandleAxes axes = HandleAxes.XYZ;
        public Space space = Space.Self;
        public HandleType type = HandleType.Position;
        public SnappingType snappingType = SnappingType.Relative;

        public Vector3 positionSnap = Vector3.zero;
        public float rotationSnap;
        public Vector3 scaleSnap = Vector3.zero;

        public Camera handleCamera;
        public float minScaleLimit = 0.25f;
        public float maxScaleLimit = 4f;

        private PositionHandle PositionHandle { get; set; }
        private RotationHandleOld RotationHandle { get; set; }
        private ScaleHandle ScaleHandle { get; set; }

        private TransformHandleManager _manager;
        
        protected virtual void Awake()
        {
            autoScaleSizeInPixels = Mathf.Min(Screen.width, Screen.height) / 4f;
            PositionHandle = GetComponentInChildren<PositionHandle>();
            RotationHandle = GetComponentInChildren<RotationHandleOld>();
            ScaleHandle = GetComponentInChildren<ScaleHandle>();
            
            Clear();
        }
        
        protected virtual void OnEnable()
        {
            if(_manager)
                handleCamera = _manager.mainCamera;
        }

        protected virtual void OnDisable()
        {
            Disable();
        }

        protected void OnDestroy()
        {
            if (_manager == null) return;
            
            _manager.RemoveHandle(this);
            OnHandleDestroyedEvent?.Invoke(this);
        }

        protected virtual void LateUpdate()
        {
            UpdateHandleTransformation();
            
            if (!autoScale || handleCamera == null) return;
            transform.PreserveScaleOnScreen(handleCamera.fieldOfView, autoScaleSizeInPixels, handleCamera);
        }

        public void Initialize(TransformHandleManager transformHandleManager)
        {
            _manager = transformHandleManager;
            handleCamera = _manager.mainCamera;
        }
        
        public virtual void Enable(Transform targetTransform)
        {
            target = targetTransform;
            transform.position = targetTransform.position;
            
            CreateHandles();
        }

        public virtual void Disable()
        {
            target = null;

            Clear();
        }

        public virtual void InteractionStart()
        {
            OnInteractionStartEvent?.Invoke(this);
        }

        public virtual void InteractionStay()
        {
            OnInteractionEvent?.Invoke(this);
        }
        
        public virtual void InteractionEnd()
        {
            OnInteractionEndEvent?.Invoke(this);
        }

        public virtual void ChangeHandleType(HandleType handleType)
        {
            type = handleType;
            
            Clear();
            CreateHandles();
        }

        public virtual void ChangeHandleSpace(Space newSpace)
        {
            space = newSpace;
        }

        public virtual void ChangeAxes(HandleAxes handleAxes)
        {
            axes = handleAxes;
            
            Clear();
            CreateHandles();
        }

        protected virtual void UpdateHandleTransformation()
        {
            if(!target) return;
            
            transform.position = target.transform.position;
            if (space == Space.Self || type == HandleType.Scale)
            {
                transform.rotation = target.transform.rotation;
            }
            else
            {
                transform.rotation = Quaternion.identity;
            }
        }

        protected virtual void CreateHandles()
        {
            switch (type)
            {
                case HandleType.Position:
                    ActivatePositionHandle();
                    break;
                case HandleType.Rotation:
                    ActivateRotationHandle();
                    break;
                case HandleType.Scale:
                    ActivateScaleHandle();
                    break;
                case HandleType.PositionRotation:
                    ActivatePositionHandle();
                    ActivateRotationHandle();
                    break;
                case HandleType.PositionScale:
                    ActivatePositionHandle();
                    ActivateScaleHandle();
                    break;
                case HandleType.RotationScale:
                    ActivateRotationHandle();
                    ActivateScaleHandle();
                    break;
                case HandleType.All:
                    ActivatePositionHandle();
                    ActivateRotationHandle();
                    ActivateScaleHandle();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ActivatePositionHandle()
        {
            PositionHandle.Initialize(this);
            PositionHandle.gameObject.SetActive(true);
        }

        public void HidePositionHandle()
        {
            PositionHandle.gameObject.SetActive(false);
        }

        public void HideRotationHandle()
        {
            RotationHandle.gameObject.SetActive(false);
        }

        public void HideScaleHandle()
        {
            ScaleHandle.gameObject.SetActive(false);
        }

        private void ActivateRotationHandle()
        {
            RotationHandle.Initialize(this);
            RotationHandle.gameObject.SetActive(true);
        }
        
        private void ActivateScaleHandle()
        {
            ScaleHandle.Initialize(this);
            ScaleHandle.gameObject.SetActive(true);
        }
        
        protected virtual void Clear()
        {
            if (PositionHandle.gameObject.activeSelf) PositionHandle.gameObject.SetActive(false);
            if (RotationHandle.gameObject.activeSelf) RotationHandle.gameObject.SetActive(false);
            if (ScaleHandle.gameObject.activeSelf) ScaleHandle.gameObject.SetActive(false);
        }
    }
}