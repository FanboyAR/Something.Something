using UnityEngine;

namespace TheCube
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 3.2f;
        public float runMultiplier = 1.45f;
        public float acceleration = 12f;
        public float deceleration = 16f;
        public float jumpHeight = 1.1f;
        public float gravity = -16f;

        [Header("View")]
        public Transform cameraHolder;
        public float mouseSensitivity = 0.9f;
        public float lookSmoothing = 18f;
        public float maxLookAngle = 80f;
        public bool lockCursor = true;

        [Header("Interaction")]
        public float grabDistance = 3f;
        public float holdDistance = 1.7f;
        public float holdStrength = 45f;
        public float maxHeldObjectSpeed = 5f;
        public float fallRespawnY = -2.5f;

        private CharacterController cc;
        private Vector3 velocity;
        private Vector3 horizontalVelocity;
        private float verticalLookRotation;
        private float targetVerticalLookRotation;
        private bool cursorLocked;
        private Rigidbody heldBody;

        private void Awake()
        {
            cc = GetComponent<CharacterController>();
            if (cameraHolder == null && Camera.main != null)
                cameraHolder = Camera.main.transform;

            EnsureBodyHitbox();
        }

        private void Start()
        {
            SetCursorState(lockCursor);
        }

        void Update()
        {
            if (lockCursor)
                HandleCursorLock();

            HandleLook();
            HandleMovement();
            HandleGrab();
            HandleFallRespawn();
        }

        void HandleLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            targetVerticalLookRotation -= mouseY;
            targetVerticalLookRotation = Mathf.Clamp(targetVerticalLookRotation, -maxLookAngle, maxLookAngle);
            verticalLookRotation = Mathf.Lerp(verticalLookRotation, targetVerticalLookRotation, lookSmoothing * Time.deltaTime);

            if (cameraHolder != null)
                cameraHolder.localEulerAngles = new Vector3(verticalLookRotation, 0f, 0f);
        }

        void HandleMovement()
        {
            float inputX = Input.GetAxis("Horizontal");
            float inputZ = Input.GetAxis("Vertical");
            Vector3 move = transform.right * inputX + transform.forward * inputZ;
            move.Normalize();

            bool running = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float speed = walkSpeed * (running ? runMultiplier : 1f);
            Vector3 targetHorizontalVelocity = move * speed;
            float responsiveness = move.sqrMagnitude > 0f ? acceleration : deceleration;
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetHorizontalVelocity, responsiveness * Time.deltaTime);

            // Apply gravity
            velocity.y += gravity * Time.deltaTime;

            // Ground detection and handling
            if (cc.isGrounded)
            {
                // Keep a small downward velocity to ensure grounded state
                if (velocity.y < 0)
                    velocity.y = -2f;

                // Jump input
                if (Input.GetButtonDown("Jump"))
                {
                    velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }
            }

            cc.Move((horizontalVelocity + new Vector3(0, velocity.y, 0)) * Time.deltaTime);
        }

        private void HandleCursorLock()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetCursorState(false);
            }
            else if (Input.GetMouseButtonDown(0) && !cursorLocked)
            {
                SetCursorState(true);
            }
        }

        private void SetCursorState(bool locked)
        {
            cursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        public void ResetMotion()
        {
            velocity = Vector3.zero;
            horizontalVelocity = Vector3.zero;
            DropHeldBody();
        }

        public void ApplyExternalImpulse(Vector3 impulse)
        {
            horizontalVelocity = new Vector3(impulse.x, 0f, impulse.z);
            velocity.y = Mathf.Max(velocity.y, impulse.y);
        }

        public void RespawnAtCheckpoint(string reason)
        {
            transform.position = MazeRunState.CheckpointPosition;
            ResetMotion();
            Debug.Log(reason);
        }

        private void EnsureBodyHitbox()
        {
            Transform body = transform.Find("BodyHitbox");
            if (body == null)
            {
                var bodyGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                bodyGO.name = "BodyHitbox";
                bodyGO.transform.SetParent(transform, false);
                body = bodyGO.transform;
            }

            body.localPosition = new Vector3(0f, 0.9f, 0f);
            body.localRotation = Quaternion.identity;
            body.localScale = new Vector3(0.8f, 0.9f, 0.8f);

            var bodyCollider = body.GetComponent<CapsuleCollider>();
            if (bodyCollider != null)
            {
                bodyCollider.isTrigger = false;
                bodyCollider.height = 2f;
                bodyCollider.radius = 0.5f;
                bodyCollider.center = Vector3.zero;
            }

            var bodyRigidbody = body.GetComponent<Rigidbody>();
            if (bodyRigidbody == null)
                bodyRigidbody = body.gameObject.AddComponent<Rigidbody>();

            bodyRigidbody.isKinematic = true;
            bodyRigidbody.useGravity = false;

            if (body.GetComponent<PlayerHitbox>() == null)
                body.gameObject.AddComponent<PlayerHitbox>();
        }

        private void HandleGrab()
        {
            if (Input.GetMouseButtonDown(0))
                TryGrabBody();

            if (Input.GetMouseButtonUp(0))
                DropHeldBody();

            if (heldBody == null)
                return;

            Transform origin = cameraHolder != null ? cameraHolder : transform;
            Vector3 holdPoint = origin.position + origin.forward * holdDistance;
            Vector3 toHoldPoint = holdPoint - heldBody.position;
            Vector3 desiredVelocity = Vector3.ClampMagnitude(toHoldPoint * holdStrength, maxHeldObjectSpeed);
            heldBody.linearVelocity = desiredVelocity;
            heldBody.angularVelocity *= 0.85f;
        }

        private void TryGrabBody()
        {
            Transform origin = cameraHolder != null ? cameraHolder : transform;
            Ray ray = new Ray(origin.position, origin.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, grabDistance))
                return;

            var puzzleBox = hit.collider.GetComponentInParent<PuzzleBox>();
            if (puzzleBox == null)
                return;

            heldBody = puzzleBox.GetComponent<Rigidbody>();
            if (heldBody == null)
                return;

            heldBody.useGravity = true;
            heldBody.linearDamping = 8f;
            heldBody.angularDamping = 8f;
        }

        private void DropHeldBody()
        {
            if (heldBody == null)
                return;

            heldBody.linearVelocity = Vector3.ClampMagnitude(heldBody.linearVelocity, maxHeldObjectSpeed);
            heldBody = null;
        }

        private void HandleFallRespawn()
        {
            if (transform.position.y > fallRespawnY)
                return;

            RespawnAtCheckpoint("Fell into the pit. Returned to the nearest checkpoint.");
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            var body = hit.collider.attachedRigidbody;
            if (body == null || body.isKinematic)
                return;

            Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z);
            body.linearVelocity = pushDirection * walkSpeed;
        }
    }
}
