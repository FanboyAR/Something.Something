using UnityEngine;

namespace TheCube
{
    /// <summary>
    /// Diagnostic script to help debug character controller falling through floor issues.
    /// </summary>
    public class CharacterControllerDiagnostics : MonoBehaviour
    {
        [SerializeField] private CharacterController characterController;
        [SerializeField] private GameObject groundObject;

        private void Start()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            if (groundObject == null)
                groundObject = GameObject.Find("Ground");
        }

        private void Update()
        {
            if (characterController == null)
            {
                Debug.LogError("CharacterControllerDiagnostics: CharacterController not found!");
                return;
            }

            // Log status
            if (Input.GetKeyDown(KeyCode.D))
            {
                DiagnosticReport();
            }
        }

        public void DiagnosticReport()
        {
            Debug.Log("=== Character Controller Diagnostics ===");

            if (characterController != null)
            {
                Debug.Log($"Character Position: {transform.position}");
                Debug.Log($"Character CC Height: {characterController.height}");
                Debug.Log($"Character CC Radius: {characterController.radius}");
                Debug.Log($"Character CC Center: {characterController.center}");
                Debug.Log($"Character CC IsGrounded: {characterController.isGrounded}");
                Debug.Log($"Character CC Enabled: {characterController.enabled}");
            }

            if (groundObject != null)
            {
                Debug.Log($"Ground Position: {groundObject.transform.position}");
                Debug.Log($"Ground Active: {groundObject.activeInHierarchy}");
                
                Collider groundCollider = groundObject.GetComponent<Collider>();
                if (groundCollider != null)
                {
                    Debug.Log($"Ground Collider Type: {groundCollider.GetType().Name}");
                    Debug.Log($"Ground Collider IsTrigger: {groundCollider.isTrigger}");
                    Debug.Log($"Ground Collider Enabled: {groundCollider.enabled}");
                    
                    if (groundCollider is BoxCollider bc)
                    {
                        Debug.Log($"Ground BoxCollider Size: {bc.size}");
                        Debug.Log($"Ground BoxCollider Center: {bc.center}");
                    }
                }
                else
                {
                    Debug.LogError("Ground object has NO collider!");
                }
            }
            else
            {
                Debug.LogError("Ground object not found!");
            }

            Debug.Log("=== Press D again or call DiagnosticReport() to repeat ===");
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
                return;

            if (characterController == null)
                return;

            // Draw character controller bounds
            Gizmos.color = Color.yellow;
            Vector3 ccCenter = transform.position + characterController.center;
            Gizmos.DrawWireCube(ccCenter, new Vector3(characterController.radius * 2, characterController.height, characterController.radius * 2));

            // Draw ground bounds if available
            if (groundObject != null)
            {
                BoxCollider bc = groundObject.GetComponent<BoxCollider>();
                if (bc != null)
                {
                    Gizmos.color = Color.green;
                    Vector3 groundCenter = groundObject.transform.position + bc.center;
                    Gizmos.DrawWireCube(groundCenter, bc.size);
                }
            }
        }
    }
}
