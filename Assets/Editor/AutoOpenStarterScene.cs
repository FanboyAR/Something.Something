#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

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
    }
}
#endif
