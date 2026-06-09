using UnityEngine;

namespace TheCube
{
    public class MazeKeyPickup : MonoBehaviour
    {
        [SerializeField] private float hoverHeight = 0.25f;
        [SerializeField] private float hoverSpeed = 2f;
        [SerializeField] private float rotateSpeed = 90f;

        private Vector3 startLocalPosition;
        private bool isCarried;

        private void Awake()
        {
            startLocalPosition = transform.localPosition;
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
            MazeRunState.CollectExitKey(this);

            var collider = GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;

            var body = GetComponent<Rigidbody>();
            if (body != null)
                body.isKinematic = true;

            Transform carryAnchor = player.cameraHolder != null ? player.cameraHolder : player.transform;
            transform.SetParent(carryAnchor, false);
            transform.localPosition = new Vector3(0.45f, -0.25f, 0.9f);
            transform.localRotation = Quaternion.Euler(15f, 35f, 0f);
            transform.localScale = Vector3.one * 0.35f;
        }
    }
}
