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
            // Don't attempt to open scenes while entering or in Play Mode
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying)
            {
                hasOpened = true;
                return;
            }
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

            CreateStarterScene.PopulateStarterScene(scene);
        }
    }
}
#endif
