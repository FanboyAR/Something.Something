using UnityEngine;

namespace TheCube
{
    public class MazeKeyPickup : MonoBehaviour
    {
        [SerializeField] private float hoverHeight = 0.25f;
        [SerializeField] private float hoverSpeed = 2f;
        [SerializeField] private float rotateSpeed = 90f;
        [SerializeField] private float holdDistance = 0.95f;
        [SerializeField] private float holdSideOffset = 0.35f;
        [SerializeField] private float holdDropOffset = -0.25f;
        [SerializeField] private float holdStrength = 35f;
        [SerializeField] private float maxHoldSpeed = 4f;

        private Vector3 startLocalPosition;
        private bool isCarried;
        private PlayerController carrier;
        private Rigidbody body;

        private void Awake()
        {
            startLocalPosition = transform.localPosition;
            body = GetComponent<Rigidbody>();
            if (body == null)
                body = gameObject.AddComponent<Rigidbody>();

            body.mass = 1.5f;
            body.useGravity = false;
            body.isKinematic = true;
            body.linearDamping = 8f;
            body.angularDamping = 8f;
        }

        private void Update()
        {
            if (isCarried)
            {
                transform.Rotate(Vector3.up, rotateSpeed * 0.4f * Time.deltaTime, Space.Self);
                return;
            }

            float hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
            transform.localPosition = startLocalPosition + Vector3.up * hoverOffset;
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
        }

        private void FixedUpdate()
        {
            if (!isCarried || carrier == null || body == null)
                return;

            Transform anchor = carrier.cameraHolder != null ? carrier.cameraHolder : carrier.transform;
            Vector3 holdPoint = anchor.position
                + anchor.forward * holdDistance
                + anchor.right * holdSideOffset
                + Vector3.up * holdDropOffset;

            Vector3 toHoldPoint = holdPoint - body.position;
            body.linearVelocity = Vector3.ClampMagnitude(toHoldPoint * holdStrength, maxHoldSpeed);
            body.angularVelocity *= 0.8f;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isCarried)
                return;

            var player = other.GetComponentInParent<PlayerController>();
            if (player == null)
                return;

            PickUp(player);
        }

        private void PickUp(PlayerController player)
        {
            isCarried = true;
            carrier = player;
            MazeRunState.CollectExitKey(this);

            var collider = GetComponent<Collider>();
            if (collider != null)
                collider.isTrigger = false;

            transform.SetParent(null, true);
            transform.localScale = Vector3.one * 0.35f;

            if (body != null)
            {
                body.isKinematic = false;
                body.useGravity = false;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                body.interpolation = RigidbodyInterpolation.Interpolate;
            }
        }
    }
}
