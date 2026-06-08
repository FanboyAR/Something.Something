using UnityEngine;

namespace TheCube
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 5f;
        public float runMultiplier = 1.8f;
        public float jumpHeight = 1.4f;
        public float gravity = -9.81f;

        [Header("View")]
        public Transform cameraHolder;
        public float mouseSensitivity = 2.0f;
        public float maxLookAngle = 85f;
        public bool lockCursor = true;

        private CharacterController cc;
        private Vector3 velocity;
        private float verticalLookRotation;
        private bool cursorLocked;

        private void Awake()
        {
            cc = GetComponent<CharacterController>();
            if (cameraHolder == null && Camera.main != null)
                cameraHolder = Camera.main.transform;
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
        }

        void HandleLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            verticalLookRotation -= mouseY;
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, -maxLookAngle, maxLookAngle);
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

            if (cc.isGrounded && velocity.y < 0)
                velocity.y = -2f; // small downward force to keep grounded

            if (Input.GetButtonDown("Jump") && cc.isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            Vector3 horizontalVelocity = move * speed;
            cc.Move((horizontalVelocity + new Vector3(0, velocity.y, 0)) * Time.deltaTime);

            velocity.y += gravity * Time.deltaTime;
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
    }
}
