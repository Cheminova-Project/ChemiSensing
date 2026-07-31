using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit
{
    public class ExtendedLazyFollow : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("The target transform to follow.")]
        [SerializeField]
        private Transform followTarget;

        [Tooltip("Offset from the target, in local space of the target.")]
        [SerializeField]
        public Vector3 localOffset = Vector3.zero;
    
        [Header("Smoothing")]
        [Range(0f, 1f)]
        [SerializeField]
        private float smoothPositionAmount = 4f;

        [Range(0f, 1f)]
        [SerializeField]
        private float smoothRotationAmount = 4f;

        [Header("Lock Position Axes")]
        [SerializeField] public bool lockPositionX = false;
        [SerializeField] public bool lockPositionY = false;
        [SerializeField] public bool lockPositionZ = false;

        [Header("Lock Rotation Axes (Euler angles)")]
        [SerializeField] public bool lockRotationX = false;
        [SerializeField] public bool lockRotationY = false;
        [SerializeField] public bool lockRotationZ = false;

        private void LateUpdate()
        {
            if (followTarget == null)
                return;

            UpdateFollow();
        }

        private void UpdateFollow()
        {
            // Calculate world-space target position from local offset
            Vector3 targetPosition = followTarget.TransformPoint(localOffset);
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, 1f - smoothPositionAmount);

            // Lock position axes
            Vector3 currentPos = transform.position;
            if (lockPositionX) smoothedPosition.x = currentPos.x;
            if (lockPositionY) smoothedPosition.y = currentPos.y;
            if (lockPositionZ) smoothedPosition.z = currentPos.z;

            transform.position = smoothedPosition;

            // Calculate target rotation (same as target's rotation)
            Quaternion targetRotation = followTarget.rotation;
            Quaternion smoothedRotation = Quaternion.Slerp(transform.rotation, targetRotation, 1f - smoothRotationAmount);

            // Lock rotation axes
            Vector3 targetEuler = smoothedRotation.eulerAngles;
            Vector3 currentEuler = transform.rotation.eulerAngles;

            if (lockRotationX) targetEuler.x = currentEuler.x;
            if (lockRotationY) targetEuler.y = currentEuler.y;
            if (lockRotationZ) targetEuler.z = currentEuler.z;

            transform.rotation = Quaternion.Euler(targetEuler);
        }

        // Public setter (opcional)
        public void SetFollowTarget(Transform target) => followTarget = target;
    }
}
