using System;
using UnityEngine;

namespace TransformHandles
{
    /// <summary>
    /// Clase que representa un "fantasma" utilizado durante la manipulación de objetos con handles
    /// de transformación. El fantasma refleja las transformaciones aplicadas al objeto objetivo.
    /// </summary>
    public class Ghost : MonoBehaviour
    {
        private Transform GhostTransform => transform;
        private TransformHandleManager _handleManager;
        private PosRotScale _initialProperties;

        /// <summary>
        /// Obtiene las propiedades iniciales del fantasma al comenzar la interacción.
        /// </summary>
        public virtual void Initialize(TransformHandleManager handleManager)
        {
            _handleManager = handleManager;
        }
        /// <summary>
        /// Elimina el objeto fantasma de la escena.
        /// </summary>
        public virtual void Terminate()
        {
            DestroyImmediate(gameObject);
        }

        /// <summary>
        /// Actualiza la transformación del fantasma según las propiedades promedio proporcionadas.
        /// </summary>
        public void UpdateGhostTransform(PosRotScale average)
        {
            GhostTransform.position = average.Position;
            GhostTransform.rotation = average.Rotation;
            GhostTransform.localScale = average.Scale;
        }
        /// <summary>
        /// Restablece la transformación del fantasma a su estado predeterminado.
        /// </summary>
        public void ResetGhostTransform()
        {
            GhostTransform.position = Vector3.zero;
            GhostTransform.rotation = Quaternion.identity;
            GhostTransform.localScale = Vector3.one;
        }

        /// <summary>
        /// Llama al método correspondiente en el manager de handles al iniciar la interacción.
        /// </summary>
        public virtual void OnInteractionStart()
        {
            _initialProperties = new PosRotScale()
            {
                Position = GhostTransform.position,
                Rotation = GhostTransform.rotation,
                Scale = GhostTransform.lossyScale
            };
        }
        /// <summary>
        /// Llama al método correspondiente en el manager de handles al finalizar la interacción.
        /// </summary>
        public virtual void OnInteraction(HandleType handleType)
        {
            switch (handleType)
            {
                case HandleType.Position:
                    GhostTransform.position = _handleManager.ApplyGroupPosition(this, GhostTransform.position);
                    break;
                case HandleType.Rotation:
                    _handleManager.ApplyGroupRotation(this, GhostTransform.rotation);
                    break;
                case HandleType.Scale:
                    _handleManager.ApplyGroupScale(this, GhostTransform.localScale);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        /// <summary>
        /// Restablece las propiedades iniciales del fantasma.
        /// </summary>
        private void ResetInitialGhostTransformProperties()
        {
            _initialProperties.Position = GhostTransform.position;
            _initialProperties.Rotation = GhostTransform.rotation;
            _initialProperties.Scale = GhostTransform.lossyScale;
        }
    }
}