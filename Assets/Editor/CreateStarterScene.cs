#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheCube.Editor
{
    public static class CreateStarterScene
    {
        [MenuItem("TheCube/Create Starter Scene")]
        public static void Create()
        {
            // Create new empty scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Lighting
            var lightGO = new GameObject("Directional Light");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Ground
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = Vector3.one * 5f;

            // Player
            var player = new GameObject("Player");
            player.transform.position = new Vector3(0f, 1f, 0f);
            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            var playerController = player.AddComponent<TheCube.PlayerController>();

            // Camera
            var camHolder = new GameObject("CameraHolder");
            camHolder.transform.parent = player.transform;
            camHolder.transform.localPosition = new Vector3(0f, 0.9f, 0f);

            var camGO = new GameObject("Main Camera");
            var cam = camGO.AddComponent<Camera>();
            camGO.tag = "MainCamera";
            camGO.transform.parent = camHolder.transform;
            camGO.transform.localPosition = Vector3.zero;
            camGO.transform.localRotation = Quaternion.identity;

            camGO.AddComponent<TheCube.FPSCamera>();

            // Hook camera onto PlayerController
            playerController.cameraHolder = camHolder.transform;

            // Save scene
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            var scenePath = "Assets/Scenes/StarterScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.Refresh();

            Debug.Log("Starter scene created at " + scenePath + ". Open it in Unity and press Play.");
        }
    }
}
#endif
