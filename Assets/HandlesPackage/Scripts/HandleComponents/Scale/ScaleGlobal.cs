using UnityEngine;

namespace TransformHandles
{
    /// <summary>
    /// Componente que maneja la funcionalidad de escalado global en un handle de transformación.
    /// Permite escalar objetos de manera uniforme en todas las direcciones.
    /// </summary>
    public class ScaleGlobal : HandleBase
    {
        [SerializeField] private Color defaultColor;
        [SerializeField] private MeshRenderer cubeMeshRenderer;
        
        private Vector3 _axis;
        private Vector3 _startScale;
        
        public void Initialize(Handle handle, Vector3 pAxis)
        {
            ParentHandle = handle;
            _axis = pAxis;
            DefaultColor = defaultColor;
        }

        public override void Interact(Vector3 pPreviousPosition, Vector3 pCurrentPosition)
        {
            var mouseVector = (pCurrentPosition - pPreviousPosition);
            var d = (mouseVector.x + mouseVector.y) * Time.deltaTime * 2;
            var testDelta = delta + d;
            var testScale = _startScale + Vector3.Scale(_startScale, _axis) * testDelta;
            
            // Clampear la escala propuesta
            var clampedScale = new Vector3(
                Mathf.Clamp(testScale.x, ParentHandle.minScaleLimit, ParentHandle.maxScaleLimit),
                Mathf.Clamp(testScale.y, ParentHandle.minScaleLimit, ParentHandle.maxScaleLimit),
                Mathf.Clamp(testScale.z, ParentHandle.minScaleLimit, ParentHandle.maxScaleLimit)
            );
            
            // Recalcular delta basado en la escala clampeada para mantener sincronización
            // delta debe representar el factor de escala desde _startScale
            if (_startScale.x > 0.001f)
                delta = (clampedScale.x / _startScale.x) - 1f;
            else if (_startScale.y > 0.001f)
                delta = (clampedScale.y / _startScale.y) - 1f;
            else if (_startScale.z > 0.001f)
                delta = (clampedScale.z / _startScale.z) - 1f;
            else
                delta = 0;
            
            ParentHandle.target.localScale = clampedScale;
            
            base.Interact(pPreviousPosition, pCurrentPosition);
        }

        public override void StartInteraction(Vector3 pHitPoint, Vector3 pointerPosition)
        {
            base.StartInteraction(pHitPoint, pointerPosition);
            _startScale = ParentHandle.target.localScale;
        }
        
        public override void SetColor(Color color)
        {
            cubeMeshRenderer.material.color = color;
        }
        
        public override void SetDefaultColor()
        {
            cubeMeshRenderer.material.color = DefaultColor;
        }
    }
}