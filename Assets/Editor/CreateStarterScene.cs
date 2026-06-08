#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheCube.Editor
{
    [InitializeOnLoad]
    public static class CreateStarterScene
    {
        private const string StarterScenePath = "Assets/Scenes/StarterScene.unity";

        static CreateStarterScene()
        {
            EditorApplication.delayCall += EnsureStarterSceneExists;
        }

        private static void EnsureStarterSceneExists()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(StarterScenePath) == null)
            {
                Create();
            }
        }

        [MenuItem("TheCube/Create Starter Scene")]
        public static void Create()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(StarterScenePath) != null)
            {
                Debug.Log("Starter scene already exists: " + StarterScenePath);
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            PopulateStarterScene(scene);

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, StarterScenePath);
            AssetDatabase.Refresh();

            Debug.Log("Starter scene created at " + StarterScenePath + ". Open it in Unity and press Play.");
        }

        public static void PopulateStarterScene(Scene scene)
        {
            bool hasStarterRoot = GameObject.Find("StarterRoomRoot") != null;
            bool hasPlayer = GameObject.Find("Player") != null;
            bool hasCamera = GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<Camera>() != null;
            bool hasFactory = Object.FindAnyObjectByType<RoomFactory>() != null;

            if (hasStarterRoot && hasPlayer && hasCamera && hasFactory)
                return;

            ClearScene(scene);
            CreateLighting();
            CreateGroundPlane();
            CreateStarterRooms();
            CreatePlayerStart(new Vector3(-24f, 1f, 0f));
            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static void ClearScene(Scene scene)
        {
            var rootObjects = scene.GetRootGameObjects();
            foreach (var root in rootObjects)
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void CreateLighting()
        {
            var lightGO = new GameObject("Directional Light");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            lightGO.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
        }

        private static void CreateGroundPlane()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = Vector3.one * 6f;
            var renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Standard"));
                material.color = new Color(0.4f, 0.4f, 0.45f);
                renderer.sharedMaterial = material;
            }
        }

        private static void CreateStarterRooms()
        {
            var root = new GameObject("StarterRoomRoot");
            var factoryGO = new GameObject("RoomFactory");
            factoryGO.transform.parent = root.transform;
            var factory = factoryGO.AddComponent<RoomFactory>();

            const float spacing = 16f;
            var corridorRoom = factory.CreateRoom(RoomType.Corridor, new Vector3(-spacing * 1.5f, 0f, 0f), Quaternion.identity);
            corridorRoom.transform.parent = root.transform;
            OpenRoomWall(corridorRoom, Vector3.right);

            var puzzleRoom = factory.CreateRoom(RoomType.Puzzle, new Vector3(-spacing * 0.5f, 0f, 0f), Quaternion.identity);
            puzzleRoom.transform.parent = root.transform;
            OpenRoomWall(puzzleRoom, Vector3.left);
            OpenRoomWall(puzzleRoom, Vector3.right);

            var challengeRoom = factory.CreateRoom(RoomType.Challenge, new Vector3(spacing * 0.5f, 0f, 0f), Quaternion.identity);
            challengeRoom.transform.parent = root.transform;
            OpenRoomWall(challengeRoom, Vector3.left);
            OpenRoomWall(challengeRoom, Vector3.right);

            var treasureRoom = factory.CreateRoom(RoomType.Treasure, new Vector3(spacing * 1.5f, 0f, 0f), Quaternion.identity);
            treasureRoom.transform.parent = root.transform;
            OpenRoomWall(treasureRoom, Vector3.left);
        }

        private static void OpenRoomWall(GameObject room, Vector3 direction)
        {
            foreach (Transform child in room.transform)
            {
                if (child.name != "Wall")
                    continue;

                if (Vector3.Dot(child.localPosition.normalized, direction) > 0.8f)
                {
                    Object.DestroyImmediate(child.gameObject);
                    break;
                }
            }
        }

        private static void CreatePlayerStart(Vector3 position)
        {
            var player = GameObject.Find("Player");
            if (player == null)
            {
                player = new GameObject("Player");
                player.transform.position = position;

                var cc = player.AddComponent<CharacterController>();
                cc.height = 1.8f;
                cc.center = new Vector3(0f, 0.9f, 0f);
                cc.radius = 0.3f;

                player.AddComponent<TheCube.PlayerController>();
            }
            else
            {
                player.transform.position = position;
                if (player.GetComponent<CharacterController>() == null)
                {
                    var cc = player.AddComponent<CharacterController>();
                    cc.height = 1.8f;
                    cc.center = new Vector3(0f, 0.9f, 0f);
                    cc.radius = 0.3f;
                }
                if (player.GetComponent<TheCube.PlayerController>() == null)
                {
                    player.AddComponent<TheCube.PlayerController>();
                }
            }

            CreatePlayerBody(player.transform);
            var cameraHolder = CreateCameraHolder(player);
            var cameraGO = CreateMainCamera(cameraHolder);

            var playerController = player.GetComponent<TheCube.PlayerController>();
            playerController.cameraHolder = cameraHolder.transform;
        }

        private static void CreatePlayerBody(Transform playerTransform)
        {
            var body = playerTransform.Find("PlayerBody");
            if (body == null)
            {
                var bodyGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bodyGO.name = "PlayerBody";
                bodyGO.transform.parent = playerTransform;
                bodyGO.transform.localPosition = new Vector3(0f, 0.8f, -0.5f);
                bodyGO.transform.localScale = new Vector3(0.8f, 1.6f, 0.8f);
                var renderer = bodyGO.GetComponent<Renderer>();
                if (renderer != null)
                {
                    var material = new Material(Shader.Find("Standard"));
                    material.color = new Color(0.2f, 0.6f, 0.9f);
                    renderer.sharedMaterial = material;
                }
            }
        }

        private static GameObject CreateCameraHolder(GameObject parent)
        {
            var holderTransform = parent.transform.Find("CameraHolder");
            if (holderTransform == null)
            {
                var holderGO = new GameObject("CameraHolder");
                holderGO.transform.parent = parent.transform;
                holderGO.transform.localPosition = new Vector3(0f, 1.7f, 0.4f);
                holderGO.transform.localRotation = Quaternion.identity;
                return holderGO;
            }
            return holderTransform.gameObject;
        }

        private static GameObject CreateMainCamera(GameObject holder)
        {
            var cameraTransform = holder.transform.Find("Main Camera");
            GameObject cameraGO;
            if (cameraTransform == null)
            {
                cameraGO = new GameObject("Main Camera");
                cameraGO.tag = "MainCamera";
                cameraGO.transform.parent = holder.transform;
                cameraGO.transform.localPosition = Vector3.zero;
                cameraGO.transform.localRotation = Quaternion.identity;
                cameraGO.AddComponent<Camera>();
                cameraGO.AddComponent<AudioListener>();
                cameraGO.AddComponent<TheCube.FPSCamera>();
            }
            else
            {
                cameraGO = cameraTransform.gameObject;
                cameraGO.tag = "MainCamera";
                if (cameraGO.GetComponent<Camera>() == null)
                    cameraGO.AddComponent<Camera>();
                if (cameraGO.GetComponent<AudioListener>() == null)
                    cameraGO.AddComponent<AudioListener>();
                if (cameraGO.GetComponent<TheCube.FPSCamera>() == null)
                    cameraGO.AddComponent<TheCube.FPSCamera>();
            }

            return cameraGO;
        }
    }
}
#endif
