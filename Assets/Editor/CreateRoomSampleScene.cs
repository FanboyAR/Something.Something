#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TheCube.Editor
{
    public static class CreateRoomSampleScene
    {
        [MenuItem("TheCube/Create Room Sample Scene")]
        public static void Create()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var lightGO = new GameObject("Directional Light");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = Vector3.one * 10f;
            var groundRenderer = ground.GetComponent<Renderer>();
            if (groundRenderer != null)
            {
                groundRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
                groundRenderer.sharedMaterial.color = new Color(0.7f, 0.7f, 0.7f);
            }

            var factoryGO = new GameObject("RoomFactory");
            var factory = factoryGO.AddComponent<TheCube.RoomFactory>();

            factory.CreateRoom(RoomType.Corridor, new Vector3(-20f, 0f, 0f), Quaternion.identity);
            factory.CreateRoom(RoomType.Puzzle, new Vector3(0f, 0f, 0f), Quaternion.identity);
            factory.CreateRoom(RoomType.Challenge, new Vector3(20f, 0f, 0f), Quaternion.identity);
            factory.CreateRoom(RoomType.Treasure, new Vector3(40f, 0f, 0f), Quaternion.identity);

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            var scenePath = "Assets/Scenes/RoomSampleScene.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.Refresh();
            Debug.Log("Room sample scene created: " + scenePath);
        }
    }
}
#endif
