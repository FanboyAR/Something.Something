#if UNITY_EDITOR
using System.Collections.Generic;
using TheCube;
using UnityEditor;
using UnityEngine;

namespace TheCube.Editor
{
    public static class CreateMazePrefabLibrary
    {
        private const string PrefabFolder = "Assets/Prefabs/Maze";
        private const string MaterialFolder = "Assets/Materials/Maze";
        private const float RoomSize = 10f;
        private const float WallHeight = 3f;
        private const float WallThickness = 0.5f;

        [MenuItem("TheCube/Create Maze Prefab Library")]
        public static void Create()
        {
            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets/Prefabs", "Maze");
            EnsureFolder("Assets", "Materials");
            EnsureFolder("Assets/Materials", "Maze");

            var floorMaterial = GetMaterial("Maze_Floor", new Color(0.44f, 0.46f, 0.42f));
            var wallMaterial = GetMaterial("Maze_Wall", new Color(0.72f, 0.72f, 0.68f));
            var trimMaterial = GetMaterial("Maze_Trim", new Color(0.19f, 0.22f, 0.24f));
            var puzzleMaterial = GetMaterial("Maze_Puzzle", new Color(0.20f, 0.58f, 0.86f));
            var hazardMaterial = GetMaterial("Maze_Hazard", new Color(0.86f, 0.19f, 0.16f));
            var treasureMaterial = GetMaterial("Maze_Treasure", new Color(0.95f, 0.76f, 0.20f));

            CreateModule("Maze_Room_Cross", RoomType.Corridor, true, true, true, true, floorMaterial, wallMaterial, trimMaterial);
            CreateModule("Maze_Corridor_NS", RoomType.Corridor, true, false, true, false, floorMaterial, wallMaterial, trimMaterial);
            CreateModule("Maze_Corridor_EW", RoomType.Corridor, false, true, false, true, floorMaterial, wallMaterial, trimMaterial);
            CreateModule("Maze_Corner_NE", RoomType.Corridor, true, true, false, false, floorMaterial, wallMaterial, trimMaterial);
            CreateModule("Maze_Corner_ES", RoomType.Corridor, false, true, true, false, floorMaterial, wallMaterial, trimMaterial);
            CreateModule("Maze_Corner_SW", RoomType.Corridor, false, false, true, true, floorMaterial, wallMaterial, trimMaterial);
            CreateModule("Maze_Corner_WN", RoomType.Corridor, true, false, false, true, floorMaterial, wallMaterial, trimMaterial);
            CreateModule("Maze_TJunction_NES", RoomType.Corridor, true, true, true, false, floorMaterial, wallMaterial, trimMaterial);
            CreateModule("Maze_TJunction_ESW", RoomType.Corridor, false, true, true, true, floorMaterial, wallMaterial, trimMaterial);
            CreateModule("Maze_TJunction_SWN", RoomType.Corridor, true, false, true, true, floorMaterial, wallMaterial, trimMaterial);
            CreateModule("Maze_TJunction_WNE", RoomType.Corridor, true, true, false, true, floorMaterial, wallMaterial, trimMaterial);
            CreateModule("Maze_DeadEnd_N", RoomType.Corridor, true, false, false, false, floorMaterial, wallMaterial, trimMaterial);
            CreateModule("Maze_DeadEnd_E", RoomType.Corridor, false, true, false, false, floorMaterial, wallMaterial, trimMaterial);
            CreateModule("Maze_DeadEnd_S", RoomType.Corridor, false, false, true, false, floorMaterial, wallMaterial, trimMaterial);
            CreateModule("Maze_DeadEnd_W", RoomType.Corridor, false, false, false, true, floorMaterial, wallMaterial, trimMaterial);

            CreateSpecialRoom("Maze_Room_Puzzle", RoomType.Puzzle, floorMaterial, wallMaterial, puzzleMaterial);
            CreateSpecialRoom("Maze_Room_Challenge", RoomType.Challenge, floorMaterial, wallMaterial, hazardMaterial);
            CreateSpecialRoom("Maze_Room_Treasure", RoomType.Treasure, floorMaterial, wallMaterial, treasureMaterial);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Maze prefab library created in " + PrefabFolder);
        }

        [MenuItem("TheCube/Create And Assign Maze Prefab Library")]
        public static void CreateAndAssign()
        {
            Create();

            var modules = new List<MazeModule>();
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabFolder });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var module = prefab != null ? prefab.GetComponent<MazeModule>() : null;
                if (module != null)
                    modules.Add(module);
            }

            var generators = Object.FindObjectsByType<MazeGenerator>(FindObjectsInactive.Exclude);
            for (int i = 0; i < generators.Length; i++)
            {
                var serializedObject = new SerializedObject(generators[i]);
                var modulesProperty = serializedObject.FindProperty("modulePrefabs");
                modulesProperty.arraySize = modules.Count;
                for (int j = 0; j < modules.Count; j++)
                    modulesProperty.GetArrayElementAtIndex(j).objectReferenceValue = modules[j];

                var chunkProperty = serializedObject.FindProperty("chunkPrefab");
                if (chunkProperty != null && modules.Count > 0)
                    chunkProperty.objectReferenceValue = modules[0].gameObject;

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(generators[i]);
            }

            Debug.Log($"Assigned {modules.Count} maze module prefabs to {generators.Length} MazeGenerator object(s) in the open scene.");
        }

        private static void CreateModule(string name, RoomType type, bool northOpen, bool eastOpen, bool southOpen, bool westOpen, Material floorMaterial, Material wallMaterial, Material trimMaterial)
        {
            var root = CreateBase(name, type, northOpen, eastOpen, southOpen, westOpen, floorMaterial, wallMaterial);
            AddTrim(root.transform, trimMaterial);
            SavePrefab(root, name);
        }

        private static void CreateSpecialRoom(string name, RoomType type, Material floorMaterial, Material wallMaterial, Material accentMaterial)
        {
            var root = CreateBase(name, type, true, true, true, true, floorMaterial, wallMaterial);

            if (type == RoomType.Puzzle)
            {
                CreateCube(root.transform, "Puzzle Divider", new Vector3(0f, 1.2f, 0f), new Vector3(6f, 2.4f, 0.25f), accentMaterial);
                CreateCube(root.transform, "Pressure Plate A", new Vector3(-2.5f, 0.08f, 2.5f), new Vector3(1.6f, 0.12f, 1.6f), accentMaterial);
                CreateCube(root.transform, "Pressure Plate B", new Vector3(2.5f, 0.08f, -2.5f), new Vector3(1.6f, 0.12f, 1.6f), accentMaterial);
                CreateCube(root.transform, "Puzzle Block", new Vector3(0f, 0.55f, 2f), Vector3.one, accentMaterial);
            }
            else if (type == RoomType.Challenge)
            {
                for (int i = 0; i < 3; i++)
                    CreateCube(root.transform, "Hazard Step " + (i + 1), new Vector3(-2f + i * 2f, 0.2f, -2.5f + i * 2f), new Vector3(1.3f, 0.3f, 1.3f), accentMaterial);

                CreateCylinder(root.transform, "Spinner Hazard A", new Vector3(-1.8f, 1.1f, 0f), new Vector3(0.25f, 1.6f, 0.25f), Quaternion.Euler(0f, 0f, 90f), accentMaterial);
                CreateCylinder(root.transform, "Spinner Hazard B", new Vector3(1.8f, 1.1f, 0f), new Vector3(0.25f, 1.6f, 0.25f), Quaternion.Euler(0f, 0f, 90f), accentMaterial);
            }
            else if (type == RoomType.Treasure)
            {
                CreateCylinder(root.transform, "Treasure Pedestal", new Vector3(0f, 0.25f, 1.5f), new Vector3(1.2f, 0.25f, 1.2f), Quaternion.identity, accentMaterial);
                CreateCube(root.transform, "Treasure Chest", new Vector3(0f, 0.75f, 1.5f), new Vector3(1.4f, 0.7f, 1f), accentMaterial);
                CreateCube(root.transform, "Treasure Alcove", new Vector3(0f, 1.5f, 3.75f), new Vector3(4f, 3f, 0.25f), accentMaterial);
            }

            SavePrefab(root, name);
        }

        private static GameObject CreateBase(string name, RoomType type, bool northOpen, bool eastOpen, bool southOpen, bool westOpen, Material floorMaterial, Material wallMaterial)
        {
            var root = new GameObject(name);
            var module = root.AddComponent<MazeModule>();
            module.roomType = type;
            module.opensNorth = northOpen;
            module.opensEast = eastOpen;
            module.opensSouth = southOpen;
            module.opensWest = westOpen;
            module.canAdaptOpenings = type != RoomType.Corridor;

            CreateCube(root.transform, "Floor", new Vector3(0f, -0.15f, 0f), new Vector3(RoomSize, 0.3f, RoomSize), floorMaterial);
            module.wallNorth = CreateCube(root.transform, "Wall_North", new Vector3(0f, WallHeight * 0.5f, RoomSize * 0.5f), new Vector3(RoomSize, WallHeight, WallThickness), wallMaterial);
            module.wallEast = CreateCube(root.transform, "Wall_East", new Vector3(RoomSize * 0.5f, WallHeight * 0.5f, 0f), new Vector3(WallThickness, WallHeight, RoomSize), wallMaterial);
            module.wallSouth = CreateCube(root.transform, "Wall_South", new Vector3(0f, WallHeight * 0.5f, -RoomSize * 0.5f), new Vector3(RoomSize, WallHeight, WallThickness), wallMaterial);
            module.wallWest = CreateCube(root.transform, "Wall_West", new Vector3(-RoomSize * 0.5f, WallHeight * 0.5f, 0f), new Vector3(WallThickness, WallHeight, RoomSize), wallMaterial);

            module.wallNorth.SetActive(!northOpen);
            module.wallEast.SetActive(!eastOpen);
            module.wallSouth.SetActive(!southOpen);
            module.wallWest.SetActive(!westOpen);

            module.connectionNorth = CreateConnection(root.transform, "Connection_North", new Vector3(0f, 0f, RoomSize * 0.5f));
            module.connectionEast = CreateConnection(root.transform, "Connection_East", new Vector3(RoomSize * 0.5f, 0f, 0f));
            module.connectionSouth = CreateConnection(root.transform, "Connection_South", new Vector3(0f, 0f, -RoomSize * 0.5f));
            module.connectionWest = CreateConnection(root.transform, "Connection_West", new Vector3(-RoomSize * 0.5f, 0f, 0f));

            return root;
        }

        private static void AddTrim(Transform parent, Material material)
        {
            CreateCube(parent, "Guide Line N", new Vector3(0f, 0.03f, 3.5f), new Vector3(2.5f, 0.06f, 0.18f), material);
            CreateCube(parent, "Guide Line E", new Vector3(3.5f, 0.03f, 0f), new Vector3(0.18f, 0.06f, 2.5f), material);
            CreateCube(parent, "Guide Line S", new Vector3(0f, 0.03f, -3.5f), new Vector3(2.5f, 0.06f, 0.18f), material);
            CreateCube(parent, "Guide Line W", new Vector3(-3.5f, 0.03f, 0f), new Vector3(0.18f, 0.06f, 2.5f), material);
        }

        private static GameObject CreateCube(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            AssignMaterial(go, material);
            return go;
        }

        private static GameObject CreateCylinder(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Quaternion localRotation, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = localRotation;
            go.transform.localScale = localScale;
            AssignMaterial(go, material);
            return go;
        }

        private static Transform CreateConnection(Transform parent, string name, Vector3 localPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            return go.transform;
        }

        private static void AssignMaterial(GameObject go, Material material)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
        }

        private static Material GetMaterial(string name, Color color)
        {
            string path = MaterialFolder + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;

            material = new Material(Shader.Find("Standard"));
            material.color = color;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void SavePrefab(GameObject root, string name)
        {
            string path = PrefabFolder + "/" + name + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
        }

        private static void EnsureFolder(string parent, string folder)
        {
            string path = parent + "/" + folder;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
#endif
