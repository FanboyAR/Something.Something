#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TheCube.Editor
{
    public static class MissingScriptUtility
    {
        [MenuItem("TheCube/Missing Scripts/Select First Missing Script In Scene")]
        public static void SelectFirstMissingScriptInScene()
        {
            var transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
            foreach (var transform in transforms)
            {
                int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
                if (missingCount <= 0)
                    continue;

                Selection.activeGameObject = transform.gameObject;
                EditorGUIUtility.PingObject(transform.gameObject);
                Debug.LogWarning($"Missing script found on scene object: {GetPath(transform)}", transform.gameObject);
                return;
            }

            Debug.Log("No missing scripts found in the open scene.");
        }

        [MenuItem("TheCube/Missing Scripts/Remove Missing Scripts From Scene")]
        public static void RemoveMissingScriptsFromScene()
        {
            int cleanedObjects = 0;
            int removedComponents = 0;
            var transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);

            foreach (var transform in transforms)
            {
                int missingCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
                if (missingCount <= 0)
                    continue;

                Undo.RegisterCompleteObjectUndo(transform.gameObject, "Remove Missing Scripts");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(transform.gameObject);
                cleanedObjects++;
                removedComponents += missingCount;
            }

            Debug.Log($"Removed {removedComponents} missing script component(s) from {cleanedObjects} scene object(s).");
        }

        private static string GetPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }

            return path;
        }
    }
}
#endif
