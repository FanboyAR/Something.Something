#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheCube.Editor
{
    [InitializeOnLoad]
    public static class AutoOpenStarterScene
    {
        private const string StarterScenePath = "Assets/Scenes/StarterScene.unity";
        private static bool hasOpened;

        static AutoOpenStarterScene()
        {
            EditorApplication.delayCall += TryOpenStarterScene;
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        private static void TryOpenStarterScene()
        {
            if (hasOpened)
                return;

            hasOpened = true;

            var activeScenePath = EditorSceneManager.GetActiveScene().path;
            if (activeScenePath == StarterScenePath)
                return;

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(StarterScenePath) != null)
            {
                EditorSceneManager.OpenScene(StarterScenePath);
                Debug.Log("Auto-opened StarterScene.unity.");
            }
            else
            {
                Debug.LogWarning("Starter scene asset not found at " + StarterScenePath + ". Use TheCube/Create Starter Scene.");
            }
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (scene.path != StarterScenePath)
                return;

            EnsureStarterSceneContent(scene);
        }

        private static void EnsureStarterSceneContent(Scene scene)
        {
            var rootObjects = scene.GetRootGameObjects();
            if (rootObjects.Length > 3)
                return;

            if (GameObject.Find("StarterRoomRoot") != null)
                return;

            var root = new GameObject("StarterRoomRoot");
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.parent = root.transform;
            floor.transform.localPosition = new Vector3(0f, -0.55f, 0f);
            floor.transform.localScale = new Vector3(20f, 1f, 20f);
            SetMaterialColor(floor, new Color(0.75f, 0.75f, 0.75f));

            for (int i = 0; i < 4; i++)
            {
                var room = GameObject.CreatePrimitive(PrimitiveType.Cube);
                room.name = i == 0 ? "Corridor Room" : i == 1 ? "Puzzle Room" : i == 2 ? "Challenge Room" : "Treasure Room";
                room.transform.parent = root.transform;
                room.transform.localScale = new Vector3(6f, 2f, 6f);
                room.transform.localPosition = new Vector3((i - 1.5f) * 8f, 1f, 0f);
                SetMaterialColor(room, i == 0 ? new Color(0.7f, 0.8f, 0.9f) : i == 1 ? new Color(0.6f, 0.9f, 0.6f) : i == 2 ? new Color(0.9f, 0.5f, 0.5f) : new Color(0.95f, 0.9f, 0.3f));
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("Starter scene populated with visible room prototypes.");
        }

        private static void SetMaterialColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer == null)
                return;

            var material = new Material(Shader.Find("Standard"));
            material.color = color;
            renderer.sharedMaterial = material;
        }
    }
}
#endif
