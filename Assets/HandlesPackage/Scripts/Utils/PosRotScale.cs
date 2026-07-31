using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace TransformHandles
{
    /// <summary>
    /// Estructura que encapsula las propiedades de posición, rotación y escala de un objeto.
    /// Utilizada para gestionar transformaciones en handles de transformación.
    /// </summary>
    public struct PosRotScale
    {
        /// <summary>
        /// Posición del objeto.
        /// </summary>
        public Vector3 Position;
        /// <summary>
        /// Rotación del objeto.
        /// </summary>
        public Quaternion Rotation;
        /// <summary>
        /// Escala del objeto.
        /// </summary>
        public Vector3 Scale;
    }
}