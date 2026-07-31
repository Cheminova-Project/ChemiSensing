using TransformHandles.Utils;
using UnityEngine;

namespace TransformHandles
{
    /// <summary>
    /// Componente que maneja la funcionalidad de rotación a lo largo de un eje específico en un handle de transformación.
    /// Permite rotar objetos alrededor de un eje definido.
    /// </summary>
    public class RotationAxis : HandleBase
    {
        [SerializeField] private Color defaultColor;
        [SerializeField] private Material arcMaterial;
        [SerializeField] private MeshRenderer torusMeshRenderer;

        private Camera _handleCamera;
        
        private Mesh _arcMesh;
        private Vector3 _axis;
        private Vector3 _rotatedAxis;
        private Plane _axisPlane;
        private Vector3 _tangent;
        private Vector3 _biTangent;
        
        private Quaternion _startRotation;

        private Transform _rotationHandleTransform;

        public void Initialize(Handle handle, Vector3 pAxis)
        {
            ParentHandle = handle;
            _axis = pAxis;
            DefaultColor = defaultColor;
            
            _handleCamera = ParentHandle.handleCamera;

            _rotationHandleTransform = transform.GetComponentInParent<Handle>().transform;
        }

        public override void Interact(Vector3 pPreviousPosition, Vector3 pCurrentPosition)
        {
            var cameraRay = _handleCamera.ScreenPointToRay(pCurrentPosition);
            
            if (!_axisPlane.Raycast(cameraRay, out var hitT))
            {
                base.Interact(pPreviousPosition, pCurrentPosition);
                return;
            }
            
            var hitPoint     = cameraRay.GetPoint(hitT);
            var hitDirection = (hitPoint - ParentHandle.target.position).normalized;
            
            Debug.DrawLine(ParentHandle.target.position, hitPoint, Color.yellow, Time.deltaTime, false);
            
            var   x            = Vector3.Dot(hitDirection, _tangent);
            var   y            = Vector3.Dot(hitDirection, _biTangent);
            var   angleRadians = Mathf.Atan2(y, x);
            var   angleDegrees = angleRadians * Mathf.Rad2Deg;

            if (angleRadians < 0)
                angleRadians += Mathf.PI * 2;
            
            if (ParentHandle.rotationSnap != 0)
            {
                angleDegrees = Mathf.Round(angleDegrees / ParentHandle.rotationSnap) * ParentHandle.rotationSnap;
                angleRadians = angleDegrees * Mathf.Deg2Rad;
            }

            if (ParentHandle.space == Space.Self)
            {
                ParentHandle.target.localRotation = _startRotation * Quaternion.AngleAxis(angleDegrees, _axis);
            }
            else
            {
                var invertedRotatedAxis = Quaternion.Inverse(_startRotation) * _axis;
                ParentHandle.target.rotation = _startRotation * Quaternion.AngleAxis(angleDegrees, invertedRotatedAxis);
            }
            _arcMesh = MeshUtils.CreateArc(transform.position, 
                HitPoint, 
                _rotatedAxis, 
                _rotationHandleTransform.localScale.x, 
                angleRadians, 
                Mathf.Max(Mathf.Abs(Mathf.CeilToInt(angleDegrees)) + 1, 15));
            DrawArc();

            base.Interact(pPreviousPosition, pCurrentPosition);
        }

        public override void StartInteraction(Vector3 pHitPoint, Vector3 pointerPosition)
        {
            base.StartInteraction(pHitPoint, pointerPosition);
            
            _startRotation = ParentHandle.space == Space.Self ? ParentHandle.target.localRotation : ParentHandle.target.rotation;
            

            if (ParentHandle.space == Space.Self)
            {
                _rotatedAxis = _startRotation * _axis;
            }
            else
            {
                _rotatedAxis = _axis;
            }
            _rotatedAxis.Normalize();
            
            _axisPlane = new Plane(_rotatedAxis, ParentHandle.target.position);

            var     cameraRay = _handleCamera.ScreenPointToRay(pointerPosition);
            var startHitPoint = _axisPlane.Raycast(cameraRay, out var hitT) ?
                cameraRay.GetPoint(hitT) : _axisPlane.ClosestPointOnPlane(pHitPoint);
            
            Debug.DrawLine(startHitPoint, ParentHandle.target.position, Color.blue, 5f);
            
            _tangent   = (startHitPoint - ParentHandle.target.position).normalized;
            _biTangent = Vector3.Cross(_rotatedAxis, _tangent);
        }
        
        public override void EndInteraction()
        {
            base.EndInteraction();
            delta = 0;
        }

        private void DrawArc()
        {
            Graphics.DrawMesh(_arcMesh, Matrix4x4.identity, arcMaterial, gameObject.layer);
        }
        
        public override void SetColor(Color color)
        {
            torusMeshRenderer.material.color = color;
        }
        
        public override void SetDefaultColor()
        {
            torusMeshRenderer.material.color = DefaultColor;
        }
    }
}