using UnityEngine;

namespace TransformHandles.Utils
{
    /// <summary>
    /// Clase estática que proporciona métodos de utilidad para transformaciones.
    /// </summary>
    public static class TransformUtils
    {
        /// <summary>
        /// Verifica si un Transform es un padre profundo de otro Transform.
        /// </summary>
        public static bool IsDeepParentOf(this Transform self, Transform other)
        {
            if (self == null || self == other)
            {
                return false;
            }
        
            return other.IsChildOf(self);
        }

        /// <summary>
        /// Calcula y devuelve los bounds promedio de todos los renderers hijos de un Transform.
        /// </summary>

        public static Bounds GetBounds(this Transform transform)
        {
            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            var renderers = transform.GetComponentsInChildren<Renderer>();
            var renderersCount = renderers.Length;
            
            var averageCenter = Vector3.zero;
            var averageSize = Vector3.zero;
            foreach (var renderer in renderers)
            {
                var bound = renderer.bounds;
                averageCenter += bound.center;
                averageSize += bound.size;
            }
            bounds.center = averageCenter/renderersCount;
            bounds.size = averageSize/renderersCount;
            
            return bounds;
        }
    }
}