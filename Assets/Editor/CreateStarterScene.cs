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
            if (scene.GetRootGameObjects().Length > 0 && GameObject.Find("StarterRoomRoot") != null)
                return;

            CreateLighting();
            CreateStarterRooms();
            CreatePlayerStart(new Vector3(-14f, 1f, 0f));

            EditorSceneManager.MarkSceneDirty(scene);
        }

        private static void CreateLighting()
        {
            if (GameObject.Find("Directional Light") != null)
                return;

            var lightGO = new GameObject("Directional Light");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.Soft;
            lightGO.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
        }

        private static void CreateStarterRooms()
        {
            var root = new GameObject("StarterRoomRoot");
            var factoryGO = new GameObject("RoomFactory");
            factoryGO.transform.parent = root.transform;
            var factory = factoryGO.AddComponent<RoomFactory>();

            const float spacing = 14f;
            var corridorRoom = factory.CreateRoom(RoomType.Corridor, new Vector3(-spacing, 0f, 0f), Quaternion.identity);
            corridorRoom.transform.parent = root.transform;
            OpenRoomWall(corridorRoom, Vector3.right);

            var puzzleRoom = factory.CreateRoom(RoomType.Puzzle, new Vector3(0f, 0f, 0f), Quaternion.identity);
            puzzleRoom.transform.parent = root.transform;
            OpenRoomWall(puzzleRoom, Vector3.left);
            OpenRoomWall(puzzleRoom, Vector3.right);

            var challengeRoom = factory.CreateRoom(RoomType.Challenge, new Vector3(spacing, 0f, 0f), Quaternion.identity);
            challengeRoom.transform.parent = root.transform;
            OpenRoomWall(challengeRoom, Vector3.left);
            OpenRoomWall(challengeRoom, Vector3.right);

            var treasureRoom = factory.CreateRoom(RoomType.Treasure, new Vector3(spacing * 2f, 0f, 0f), Quaternion.identity);
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
            if (GameObject.Find("Player") != null)
                return;

            var player = new GameObject("Player");
            player.transform.position = position;

            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            cc.radius = 0.3f;

            var playerController = player.AddComponent<TheCube.PlayerController>();

            var cameraHolder = new GameObject("CameraHolder");
            cameraHolder.transform.parent = player.transform;
            cameraHolder.transform.localPosition = new Vector3(0f, 0.9f, 0f);

            var cameraGO = new GameObject("Main Camera");
            var camera = cameraGO.AddComponent<Camera>();
            cameraGO.tag = "MainCamera";
            cameraGO.transform.parent = cameraHolder.transform;
            cameraGO.transform.localPosition = Vector3.zero;
            cameraGO.transform.localRotation = Quaternion.identity;
            cameraGO.AddComponent<AudioListener>();
            cameraGO.AddComponent<TheCube.FPSCamera>();

            playerController.cameraHolder = cameraHolder.transform;
        }
    }
}
#endif
