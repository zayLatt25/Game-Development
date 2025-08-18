using UnityEngine;

namespace SmallScaleInc.TopDownPixelCharactersPack1
{
    public class SmoothCameraFollow : MonoBehaviour
    {
        [Header("Target & Offset")]
        public Transform target;          // The target the camera should follow
        public Vector3 offset;            // The offset from the target's position

        [Header("Smooth Movement")]
        [Tooltip("Time it takes for the camera to reach the target position")]
        public float smoothTime = 0.3f;     // Smoothing time for the camera movement
        private Vector3 velocity = Vector3.zero;  // Used by SmoothDamp for velocity tracking

        [Header("Look-Ahead Settings")]
        [Tooltip("Enable to allow the camera to look ahead in the player's movement direction")]
        public bool enableLookAhead = true;
        [Tooltip("How far ahead of the target to look based on movement direction")]
        public float lookAheadDistance = 2f;
        [Tooltip("Speed at which the camera's look-ahead offset adjusts")]
        public float lookAheadSpeed = 5f;
        private Vector3 currentLookAhead = Vector3.zero;  // Current look-ahead offset
        private Vector3 lastTargetPosition;               // Stores the target's last frame position

        void Start()
        {
            if (target != null)
            {
                lastTargetPosition = target.position;
            }
        }

        void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            // Determine how much the target has moved since the last frame
            Vector3 targetDelta = target.position - lastTargetPosition;
            lastTargetPosition = target.position;

            // Calculate look-ahead offset if enabled
            if (enableLookAhead)
            {
                // Desired look-ahead is in the direction of target movement, scaled by lookAheadDistance.
                Vector3 desiredLookAhead = targetDelta.normalized * lookAheadDistance;
                // Smoothly interpolate from the current look-ahead to the desired value.
                currentLookAhead = Vector3.Lerp(currentLookAhead, desiredLookAhead, Time.deltaTime * lookAheadSpeed);
            }
            else
            {
                currentLookAhead = Vector3.zero;
            }

            // Compute the desired camera position with offset and look-ahead
            Vector3 desiredPosition = target.position + offset + currentLookAhead;
            // Keep the camera's z-coordinate (useful for 2D games)
            desiredPosition.z = transform.position.z;

            // Smoothly move the camera toward the desired position
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        }
    }
}
