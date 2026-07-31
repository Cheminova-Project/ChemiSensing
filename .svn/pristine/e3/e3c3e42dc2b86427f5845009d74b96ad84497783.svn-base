using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace TransformHandles
{
    /// <summary>
    /// Componente que maneja la funcionalidad de rotación mediante un gimbal en un handle de transformación.
    /// Permite rotar objetos alrededor de múltiples ejes utilizando un gimbal.
    /// </summary>
    public class RotationHandleNew : HandleBase
    {
        private Camera _handleCamera;
        private Handle _parentHandle;

        private bool _handleInitialized;

        private Vector3 grabbedPoint;
        private Vector3 lastGimbalHitPoint;
        private bool grabbedPointSet;
        private Quaternion initialRotation;
        private Transform gimbalTransform;
        public Material rayMaterial;
        private ThreePointsBezierLine _bezierLine;
        [SerializeField]private LayerMask layerMask;
        public void Initialize(Handle handle)
        {
            if (_handleInitialized) return;

            ParentHandle = handle;
            _handleCamera = ParentHandle.handleCamera;

            _parentHandle = handle;
            transform.SetParent(_parentHandle.transform, false);
            grabbedPointSet = false;

            _bezierLine = gameObject.AddComponent<ThreePointsBezierLine>();
            _bezierLine.CreateLineRenderer(0.01f, rayMaterial);
            _bezierLine.DisableLine();
            
            _handleInitialized = true;
            gimbalTransform = null;
        }
        
        public override void SetDefaultColor()
        {
            
        }
        
        public override void SetColor(Color color)
        {
           
        }

        public override void Interact(Vector3 pPreviousPosition, Vector3 pCurrentPosition)
        {
            RotationRaycast(pCurrentPosition);
            base.Interact(pPreviousPosition, pCurrentPosition);
        }

        public override void StartInteraction(Vector3 pHitPoint, Vector3 pointerPosition)
        {
            base.StartInteraction(pHitPoint, pointerPosition);
        }
        
        public override void EndInteraction()
        {
            grabbedPointSet = false;
            _bezierLine.DisableLine();
            base.EndInteraction();
            delta = 0;
        }

        protected void RotationRaycast(Vector3 pointerPosition)
        {
            var ray = _handleCamera.ScreenPointToRay(pointerPosition);
            bool isInCollider = true;
            
            if (Physics.Raycast(ray, out var hit, float.MaxValue, layerMask))
            {
                if (!grabbedPointSet)
                {
                    grabbedPoint = hit.point;
                    initialRotation = ParentHandle.target.rotation;
                    grabbedPointSet = true;
                    gimbalTransform = hit.transform;
                    return;
                }
                lastGimbalHitPoint = hit.point;
            }
            else
                isInCollider = false;// Si salimos del collider, dejamos de actualizar el último punto de impacto

            if (isInCollider)
            {
                // Si estamos dentro del collider, alineamos con el cursor
                var dir1 = grabbedPoint - ParentHandle.target.position;
                var dir2 = lastGimbalHitPoint - ParentHandle.target.position;
     
                dir1.Normalize();
                dir2.Normalize();

                // Calculamos la rotación necesaria para alinear los puntos
                var rotation = Quaternion.FromToRotation(dir1, dir2);
                ParentHandle.target.rotation = rotation * initialRotation;
                
                _bezierLine.DisableLine(); 
            }
            else
            {
                var depth = Vector3.Distance(_handleCamera.transform.position, lastGimbalHitPoint) / 2f;
                var lastPointerPositionWorld = _handleCamera.ScreenToWorldPoint(new Vector3(
                    pointerPosition.x, 
                    pointerPosition.y, 
                    depth));
                _bezierLine.SetPoints(_handleCamera.transform.position, lastPointerPositionWorld, lastGimbalHitPoint);
            }
        }
    }
}