#if UNITY_EDITOR
using System.Collections.Generic;
using TheCube;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TheCube.Editor
{
    public static class RepairProceduralDemoScene
    {
        private const string PrefabFolder = "Assets/Prefabs/Maze";

        [MenuItem("TheCube/Repair Current Procedural Demo Scene")]
        public static void RepairCurrentScene()
        {
            RemoveMissingScriptsInScene();

            var mazeGenerator = GetOrCreateComponent<MazeGenerator>("MazeGenerator");
            var gameManager = GetOrCreateComponent<GameManager>("GameManager");
            var levelManager = gameManager.GetComponent<LevelManager>();
            if (levelManager == null)
                levelManager = gameManager.gameObject.AddComponent<LevelManager>();

            var playerController = CreateOrRepairPlayer();
            var demo = GetOrCreateComponent<ProceduralGenerationDemo>("ProceduralGenerationDemo");

            AssignObjectReference(gameManager, "levelManager", levelManager);
            AssignObjectReference(levelManager, "mazeGenerator", mazeGenerator);
            AssignObjectReference(levelManager, "player", playerController.transform);
            AssignObjectReference(demo, "mazeGenerator", mazeGenerator);
            AssignObjectReference(demo, "gameManager", gameManager);
            AssignObjectReference(demo, "playerController", playerController);
            AssignMazeModules(mazeGenerator);

            EnsureDirectionalLight();

            EditorUtility.SetDirty(mazeGenerator);
            EditorUtility.SetDirty(gameManager);
            EditorUtility.SetDirty(levelManager);
            EditorUtility.SetDirty(playerController);
            EditorUtility.SetDirty(demo);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            Selection.activeGameObject = mazeGenerator.gameObject;
            EditorGUIUtility.PingObject(mazeGenerator.gameObject);
            Debug.Log("Repaired procedural demo scene. MazeGenerator, GameManager, LevelManager, Player, and demo references are now saved.");
        }

        private static T GetOrCreateComponent<T>(string objectName) where T : Component
        {
            var go = GameObject.Find(objectName);
            if (go == null)
                go = new GameObject(objectName);

            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);

            var component = go.GetComponent<T>();
            if (component == null)
                component = go.AddComponent<T>();

            return component;
        }

        private static PlayerController CreateOrRepairPlayer()
        {
            var player = GameObject.Find("Player");
            if (player == null)
            {
                player = new GameObject("Player");
                player.transform.position = new Vector3(0f, 1f, 0f);
            }

            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(player);

            var characterController = player.GetComponent<CharacterController>();
            if (characterController == null)
                characterController = player.AddComponent<CharacterController>();

            characterController.height = 1.8f;
            characterController.radius = 0.4f;
            characterController.center = new Vector3(0f, 0.9f, 0f);

            var playerController = player.GetComponent<PlayerController>();
            if (playerController == null)
                playerController = player.AddComponent<PlayerController>();

            var cameraHolder = player.transform.Find("CameraHolder");
            if (cameraHolder == null)
            {
                var cameraHolderGO = new GameObject("CameraHolder");
                cameraHolderGO.transform.SetParent(player.transform, false);
                cameraHolder = cameraHolderGO.transform;
            }

            cameraHolder.localPosition = new Vector3(0f, 0.6f, 0f);
            cameraHolder.localRotation = Quaternion.identity;
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(cameraHolder.gameObject);

            var camera = cameraHolder.GetComponent<Camera>();
            if (camera == null)
                camera = cameraHolder.gameObject.AddComponent<Camera>();

            var listener = cameraHolder.GetComponent<AudioListener>();
            if (listener == null)
                cameraHolder.gameObject.AddComponent<AudioListener>();

            if (cameraHolder.GetComponent<FPSCamera>() == null)
                cameraHolder.gameObject.AddComponent<FPSCamera>();

            cameraHolder.gameObject.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.backgroundColor = new Color(0.19f, 0.3f, 0.47f);
            camera.fieldOfView = 60f;
            DisableLooseMainCameras(camera);

            AssignObjectReference(playerController, "cameraHolder", cameraHolder);
            return playerController;
        }

        private static void DisableLooseMainCameras(Camera activeCamera)
        {
            var cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include);
            foreach (var camera in cameras)
            {
                if (camera == activeCamera)
                    continue;

                if (camera.CompareTag("MainCamera"))
                    camera.gameObject.tag = "Untagged";

                if (camera.GetComponentInParent<PlayerController>() == null)
                {
                    camera.enabled = false;
                    var listener = camera.GetComponent<AudioListener>();
                    if (listener != null)
                        listener.enabled = false;
                }
            }
        }

        private static void EnsureDirectionalLight()
        {
            var lightGO = GameObject.Find("Directional Light");
            if (lightGO == null)
                lightGO = new GameObject("Directional Light");

            var light = lightGO.GetComponent<Light>();
            if (light == null)
                light = lightGO.AddComponent<Light>();

            light.type = LightType.Directional;
            light.intensity = 1f;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void AssignMazeModules(MazeGenerator mazeGenerator)
        {
            var modules = new List<MazeModule>();
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabFolder });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var module = prefab != null ? prefab.GetComponent<MazeModule>() : null;
                if (module != null)
                    modules.Add(module);
            }

            var serializedObject = new SerializedObject(mazeGenerator);
            var modulesProperty = serializedObject.FindProperty("modulePrefabs");
            modulesProperty.arraySize = modules.Count;
            for (int i = 0; i < modules.Count; i++)
                modulesProperty.GetArrayElementAtIndex(i).objectReferenceValue = modules[i];

            serializedObject.ApplyModifiedProperties();
        }

        private static void AssignObjectReference(Object target, string propertyName, Object value)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.objectReferenceValue = value;
                serializedObject.ApplyModifiedProperties();
            }
        }

        private static void RemoveMissingScriptsInScene()
        {
            var transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
            foreach (var transform in transforms)
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject);
        }
    }
}
#endif
