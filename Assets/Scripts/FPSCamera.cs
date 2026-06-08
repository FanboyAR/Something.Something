using UnityEngine;

namespace TheCube
{
    public class FPSCamera : MonoBehaviour
    {
        [Tooltip("Smooth camera follow speed. 0 = instant")]
        public float smoothSpeed = 10f;

        private Transform target;
        private Vector3 localPos;

        private void Awake()
        {
            target = transform.parent; // expects camera to be child of player
            localPos = transform.localPosition;
        }

        private void LateUpdate()
        {
            if (target == null) return;
            transform.position = Vector3.Lerp(transform.position, target.TransformPoint(localPos), Time.deltaTime * smoothSpeed);
        }
    }
}
