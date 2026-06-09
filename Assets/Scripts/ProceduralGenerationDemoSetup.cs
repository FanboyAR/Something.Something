using UnityEngine;

namespace TheCube
{
    /// <summary>
    /// Editor helper script for setting up the Procedural Generation Demo scene.
    /// Usage: Add this script to a GameObject, and call SetupDemoScene() from the editor.
    /// </summary>
    public class ProceduralGenerationDemoSetup : MonoBehaviour
    {
#if UNITY_EDITOR
        [ContextMenu("Setup Demo Scene")]
        public void SetupDemoScene()
        {
            Debug.Log("Setting up Procedural Generation Demo Scene...");

            // Create or get GameManager
            GameObject gameManagerGO = GameObject.Find("GameManager");
            if (gameManagerGO == null)
            {
                gameManagerGO = new GameObject("GameManager");
            }
            GameManager gameManager = gameManagerGO.GetComponent<GameManager>();
            if (gameManager == null)
            {
                gameManager = gameManagerGO.AddComponent<GameManager>();
            }

            // Create or get MazeGenerator
            GameObject mazeGeneratorGO = GameObject.Find("MazeGenerator");
            if (mazeGeneratorGO == null)
            {
                mazeGeneratorGO = new GameObject("MazeGenerator");
            }
            MazeGenerator mazeGenerator = mazeGeneratorGO.GetComponent<MazeGenerator>();
            if (mazeGenerator == null)
            {
                mazeGenerator = mazeGeneratorGO.AddComponent<MazeGenerator>();
            }

            // Ensure LevelManager exists on GameManager and link it to MazeGenerator
            LevelManager levelManager = gameManagerGO.GetComponent<LevelManager>();
            if (levelManager == null)
            {
                levelManager = gameManagerGO.AddComponent<LevelManager>();
            }
            UnityEditor.SerializedObject levelSO = new UnityEditor.SerializedObject(levelManager);
            var prop = levelSO.FindProperty("mazeGenerator");
            if (prop != null)
            {
                prop.objectReferenceValue = mazeGenerator;
                levelSO.ApplyModifiedProperties();
            }

            // Also ensure the GameManager has its LevelManager reference assigned
            UnityEditor.SerializedObject gmSO = new UnityEditor.SerializedObject(gameManager);
            var gmProp = gmSO.FindProperty("levelManager");
            if (gmProp != null)
            {
                gmProp.objectReferenceValue = levelManager;
                gmSO.ApplyModifiedProperties();
            }

            // Create or get Player
            GameObject playerGO = GameObject.Find("Player");
            if (playerGO == null)
            {
                playerGO = new GameObject("Player");
                playerGO.transform.position = new Vector3(0, 0, 0);
            }

            // Add CharacterController to Player
            CharacterController cc = playerGO.GetComponent<CharacterController>();
            if (cc == null)
            {
                cc = playerGO.AddComponent<CharacterController>();
                cc.height = 1.8f;
                cc.radius = 0.4f;
                cc.center = new Vector3(0, 0.9f, 0);
            }

            // Add PlayerController to Player
            PlayerController playerController = playerGO.GetComponent<PlayerController>();
            if (playerController == null)
            {
                playerController = playerGO.AddComponent<PlayerController>();
            }

            // Remove any Rigidbody if it exists (CharacterController doesn't work well with Rigidbody)
            Rigidbody rb = playerGO.GetComponent<Rigidbody>();
            if (rb != null)
            {
                UnityEditor.EditorGUIUtility.PingObject(rb);
            }

            // Create or get Camera
            GameObject cameraGO = playerGO.transform.Find("CameraHolder")?.gameObject;
            if (cameraGO == null)
            {
                cameraGO = new GameObject("CameraHolder");
                cameraGO.transform.SetParent(playerGO.transform);
                cameraGO.transform.localPosition = new Vector3(0, 0.6f, 0); // Eye height
            }

            // Add FPSCamera to CameraHolder
            if (cameraGO.GetComponent<FPSCamera>() == null)
            {
                cameraGO.AddComponent<FPSCamera>();
            }

            // Add Camera component if missing
            Camera camera = cameraGO.GetComponent<Camera>();
            if (camera == null)
            {
                camera = cameraGO.AddComponent<Camera>();
            }

            // Configure camera for demo visibility
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.backgroundColor = new Color(0.19f, 0.3f, 0.47f);
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;
            cameraGO.tag = "MainCamera";
            cameraGO.transform.localPosition = new Vector3(0, 0.6f, 0);
            cameraGO.transform.localRotation = Quaternion.identity;

            // Update PlayerController to reference the camera
            UnityEditor.SerializedObject playerSO = new UnityEditor.SerializedObject(playerController);
            playerSO.FindProperty("cameraHolder").objectReferenceValue = cameraGO.transform;
            playerSO.ApplyModifiedProperties();

            // Create Ground for reference (MUST be before diagnostics linking)
            GameObject groundGO = GameObject.Find("Ground");
            if (groundGO == null)
            {
                groundGO = new GameObject("Ground");
                groundGO.transform.position = Vector3.zero;
                
                // Add mesh for visibility
                MeshFilter meshFilter = groundGO.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = groundGO.AddComponent<MeshRenderer>();
                
                // Create a simple plane mesh
                Mesh planeMesh = new Mesh();
                planeMesh.vertices = new Vector3[]
                {
                    new Vector3(-50, 0, -50),
                    new Vector3(50, 0, -50),
                    new Vector3(-50, 0, 50),
                    new Vector3(50, 0, 50)
                };
                planeMesh.triangles = new int[] { 0, 2, 1, 1, 2, 3 };
                planeMesh.RecalculateNormals();
                meshFilter.mesh = planeMesh;
                
                // Add collider - use BoxCollider for ground
                BoxCollider groundCollider = groundGO.AddComponent<BoxCollider>();
                groundCollider.size = new Vector3(100, 1, 100);
                groundCollider.center = new Vector3(0, -0.5f, 0);
                groundCollider.isTrigger = false;
            }
            else
            {
                // Ensure existing ground doesn't have wrong collider type
                Collider[] colliders = groundGO.GetComponents<Collider>();
                foreach (Collider col in colliders)
                {
                    if (!(col is BoxCollider))
                        UnityEditor.EditorGUIUtility.PingObject(col.gameObject);
                }
            }

            // Create or get Demo script holder
            GameObject demoGO = GameObject.Find("ProceduralGenerationDemo");
            if (demoGO == null)
            {
                demoGO = new GameObject("ProceduralGenerationDemo");
            }

            ProceduralGenerationDemo demo = demoGO.GetComponent<ProceduralGenerationDemo>();
            if (demo == null)
            {
                demo = demoGO.AddComponent<ProceduralGenerationDemo>();
            }

            // Add diagnostics to player for easy debugging
            CharacterControllerDiagnostics diagnostics = playerGO.GetComponent<CharacterControllerDiagnostics>();
            if (diagnostics == null)
            {
                diagnostics = playerGO.AddComponent<CharacterControllerDiagnostics>();
            }
            
            // Link diagnostics to ground and character controller
            UnityEditor.SerializedObject diagnosticsSO = new UnityEditor.SerializedObject(diagnostics);
            diagnosticsSO.FindProperty("characterController").objectReferenceValue = cc;
            diagnosticsSO.FindProperty("groundObject").objectReferenceValue = groundGO;
            diagnosticsSO.ApplyModifiedProperties();

            // Link Demo to MazeGenerator and GameManager and PlayerController
            UnityEditor.SerializedObject demoSO = new UnityEditor.SerializedObject(demo);
            demoSO.FindProperty("mazeGenerator").objectReferenceValue = mazeGenerator;
            demoSO.FindProperty("gameManager").objectReferenceValue = gameManager;
            demoSO.FindProperty("playerController").objectReferenceValue = playerController;
            demoSO.ApplyModifiedProperties();

            // Create Lighting
            GameObject lightGO = GameObject.Find("Directional Light");
            if (lightGO == null)
            {
                lightGO = new GameObject("Directional Light");
                Light light = lightGO.AddComponent<Light>();
                light.type = LightType.Directional;
                lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);
            }

            Debug.Log("Procedural Generation Demo scene setup complete!");
            Debug.Log("You can now press Play to see the procedural generation in action.");
            Debug.Log("Controls: WASD to move, Mouse to look, Space to jump, R to regenerate");
        }
#endif
    }
}
