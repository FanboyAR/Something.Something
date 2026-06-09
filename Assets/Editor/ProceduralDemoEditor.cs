#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TheCube.EditorUtilities
{
    public static class ProceduralDemoEditor
    {
        private const string scenePath = "Assets/Scenes/ProceduralGenerationDemo.unity";

        [MenuItem("Tools/Procedural Demo/Open Demo Scene")]
        public static void OpenDemoScene()
        {
            if (!System.IO.File.Exists(scenePath))
            {
                EditorUtility.DisplayDialog("Scene Missing", $"Scene not found at {scenePath}.\nTry: Assets > Refresh or restart Unity.", "OK");
                AssetDatabase.Refresh();
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EditorSceneManager.OpenScene(scenePath);
            EditorUtility.DisplayDialog("Scene Opened", "Procedural Generation Demo scene opened.", "OK");
        }

        [MenuItem("Tools/Procedural Demo/Add Scene To Build Settings")]
        public static void AddSceneToBuildSettings()
        {
            if (!System.IO.File.Exists(scenePath))
            {
                EditorUtility.DisplayDialog("Scene Missing", $"Scene not found at {scenePath}.\nTry: Assets > Refresh or restart Unity.", "OK");
                AssetDatabase.Refresh();
                return;
            }

            var scenes = UnityEditor.EditorBuildSettings.scenes;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].path == scenePath)
                {
                    EditorUtility.DisplayDialog("Already Added", "Demo scene is already in Build Settings.", "OK");
                    return;
                }
            }

            var list = new System.Collections.Generic.List<UnityEditor.EditorBuildSettingsScene>(scenes);
            list.Add(new UnityEditor.EditorBuildSettingsScene(scenePath, true));
            UnityEditor.EditorBuildSettings.scenes = list.ToArray();

            EditorUtility.DisplayDialog("Added", "Procedural Demo scene added to Build Settings.", "OK");
        }

        [MenuItem("Tools/Procedural Demo/Refresh Assets and Import Scene")]
        public static void RefreshAndImport()
        {
            AssetDatabase.Refresh();
            if (System.IO.File.Exists(scenePath))
                AssetDatabase.ImportAsset(scenePath, ImportAssetOptions.ForceUpdate);

            EditorUtility.DisplayDialog("Refreshed", "Assets refreshed and scene reimported.", "OK");
        }

        [MenuItem("Tools/Procedural Demo/Reveal Scene In Project")]
        public static void RevealSceneInProject()
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(scenePath);
            if (asset == null)
            {
                EditorUtility.DisplayDialog("Scene Missing", $"Scene asset not found at {scenePath}.", "OK");
                AssetDatabase.Refresh();
                return;
            }

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        [MenuItem("Tools/Procedural Demo/Add And Open Demo Scene")]
        public static void AddAndOpenDemoScene()
        {
            // Ensure asset exists
            if (!System.IO.File.Exists(scenePath))
            {
                EditorUtility.DisplayDialog("Scene Missing", $"Scene not found at {scenePath}.\nTry: Assets > Refresh or restart Unity.", "OK");
                AssetDatabase.Refresh();
                return;
            }

            // Add to build settings if not present
            var scenes = UnityEditor.EditorBuildSettings.scenes;
            bool found = false;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].path == scenePath)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                var list = new System.Collections.Generic.List<UnityEditor.EditorBuildSettingsScene>(scenes);
                list.Add(new UnityEditor.EditorBuildSettingsScene(scenePath, true));
                UnityEditor.EditorBuildSettings.scenes = list.ToArray();
            }

            // Save current scenes and open the demo scene
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EditorSceneManager.OpenScene(scenePath);
            // Try to automatically run the setup script in the opened scene so player/camera are configured
            UnityEditor.EditorApplication.delayCall += () =>
            {
                // Find any existing setup component
                var setupComp = UnityEngine.Object.FindAnyObjectByType<TheCube.ProceduralGenerationDemoSetup>();
                if (setupComp == null)
                {
                    var go = new GameObject("ProceduralGenerationDemoSetup");
                    setupComp = go.AddComponent<TheCube.ProceduralGenerationDemoSetup>();
                }

                try
                {
                    setupComp.SetupDemoScene();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"ProceduralDemoEditor: SetupDemoScene failed: {ex}");
                }
            };

            EditorUtility.DisplayDialog("Done", "Demo scene added to Build Settings and opened. Setup will run shortly.", "OK");
        }
    }
}
#endif
