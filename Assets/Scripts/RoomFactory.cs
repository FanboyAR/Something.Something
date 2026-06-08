using UnityEngine;

namespace TheCube
{
    public class RoomFactory : MonoBehaviour
    {
        public GameObject CreateRoom(RoomType type, Vector3 position, Quaternion rotation)
        {
            const float roomSize = 10f;
            const float wallHeight = 3f;
            const float wallThickness = 0.5f;

            var root = new GameObject(type + " Room");
            root.transform.position = position;
            root.transform.rotation = rotation;
            var room = root.AddComponent<Room>();
            room.roomType = type;

            CreateFloor(root.transform, roomSize);
            CreateWall(root.transform, new Vector3(0f, wallHeight * 0.5f, roomSize * 0.5f), new Vector3(roomSize, wallHeight, wallThickness), new Color(0.9f, 0.9f, 0.9f));
            CreateWall(root.transform, new Vector3(0f, wallHeight * 0.5f, -roomSize * 0.5f), new Vector3(roomSize, wallHeight, wallThickness), new Color(0.9f, 0.9f, 0.9f));
            CreateWall(root.transform, new Vector3(roomSize * 0.5f, wallHeight * 0.5f, 0f), new Vector3(wallThickness, wallHeight, roomSize), new Color(0.9f, 0.9f, 0.9f));
            CreateWall(root.transform, new Vector3(-roomSize * 0.5f, wallHeight * 0.5f, 0f), new Vector3(wallThickness, wallHeight, roomSize), new Color(0.9f, 0.9f, 0.9f));
            CreateCeiling(root.transform, roomSize, wallHeight);

            switch (type)
            {
                case RoomType.Corridor:
                    CreateCorridorDetail(root.transform, roomSize);
                    break;
                case RoomType.Puzzle:
                    CreatePuzzleDetail(root.transform, roomSize);
                    break;
                case RoomType.Challenge:
                    CreateChallengeDetail(root.transform, roomSize);
                    break;
                case RoomType.Treasure:
                    CreateTreasureDetail(root.transform, roomSize);
                    break;
            }

            return root;
        }

        private void CreateFloor(Transform parent, float size)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.parent = parent;
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localRotation = Quaternion.identity;
            floor.transform.localScale = Vector3.one * (size / 10f);
            SetColor(floor, new Color(0.8f, 0.8f, 0.8f));
        }

        private void CreateWall(Transform parent, Vector3 localPosition, Vector3 scale, Color color)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";
            wall.transform.parent = parent;
            wall.transform.localPosition = localPosition;
            wall.transform.localScale = scale;
            SetColor(wall, color);
        }

        private void CreateCeiling(Transform parent, float size, float height)
        {
            var ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling";
            ceiling.transform.parent = parent;
            ceiling.transform.localScale = new Vector3(size, 0.2f, size);
            ceiling.transform.localPosition = new Vector3(0f, height, 0f);
            SetColor(ceiling, new Color(0.95f, 0.95f, 0.95f));
        }

        private void CreateCorridorDetail(Transform parent, float roomSize)
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "Corridor Pillar";
            pillar.transform.parent = parent;
            pillar.transform.localScale = new Vector3(1f, 3f, 1f);
            pillar.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            SetColor(pillar, new Color(0.65f, 0.65f, 0.75f));

            var bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bench.name = "Bench";
            bench.transform.parent = parent;
            bench.transform.localScale = new Vector3(2f, 0.4f, 0.8f);
            bench.transform.localPosition = new Vector3(-2.5f, 0.2f, 0f);
            SetColor(bench, new Color(0.45f, 0.45f, 0.55f));
        }

        private void CreatePuzzleDetail(Transform parent, float roomSize)
        {
            CreatePlate(parent, new Vector3(-2f, 0.01f, 0f), "Pressure Plate");
            CreatePlate(parent, new Vector3(2f, 0.01f, 0f), "Pressure Plate");
            CreateBlock(parent, new Vector3(0f, 0.5f, 0f), "Puzzle Cube", new Color(0.35f, 0.74f, 0.98f));

            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "Puzzle Pillar";
            pillar.transform.parent = parent;
            pillar.transform.localScale = new Vector3(0.5f, 1.5f, 0.5f);
            pillar.transform.localPosition = new Vector3(2.5f, 0.75f, 2f);
            SetColor(pillar, new Color(0.55f, 0.6f, 0.9f));
        }

        private void CreateChallengeDetail(Transform parent, float roomSize)
        {
            var hazard = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hazard.name = "Spinning Hazard";
            hazard.transform.parent = parent;
            hazard.transform.localScale = new Vector3(0.5f, 0.1f, 3f);
            hazard.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            hazard.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            SetColor(hazard, new Color(0.9f, 0.25f, 0.25f));
            hazard.AddComponent<Spinner>();

            var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "Challenge Platform";
            platform.transform.parent = parent;
            platform.transform.localScale = new Vector3(4f, 0.2f, 4f);
            platform.transform.localPosition = new Vector3(0f, 0.1f, -2f);
            SetColor(platform, new Color(0.7f, 0.35f, 0.35f));
        }

        private void CreateTreasureDetail(Transform parent, float roomSize)
        {
            var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Treasure Pedestal";
            pedestal.transform.parent = parent;
            pedestal.transform.localScale = new Vector3(1.5f, 0.5f, 1.5f);
            pedestal.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            SetColor(pedestal, new Color(0.6f, 0.5f, 0.15f));

            var treasure = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            treasure.name = "Treasure Orb";
            treasure.transform.parent = parent;
            treasure.transform.localScale = Vector3.one * 0.8f;
            treasure.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            SetColor(treasure, new Color(1f, 0.85f, 0.2f));

            var gem = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gem.name = "Gem";
            gem.transform.parent = parent;
            gem.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            gem.transform.localPosition = new Vector3(0.8f, 0.9f, 0.8f);
            SetColor(gem, new Color(0.3f, 0.95f, 1f));
        }

        private void CreatePlate(Transform parent, Vector3 localPosition, string name)
        {
            var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = name;
            plate.transform.parent = parent;
            plate.transform.localScale = new Vector3(2f, 0.1f, 2f);
            plate.transform.localPosition = localPosition;
            SetColor(plate, new Color(0.6f, 0.6f, 0.6f));
        }

        private void CreateBlock(Transform parent, Vector3 localPosition, string name, Color color)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.parent = parent;
            block.transform.localScale = new Vector3(1f, 1f, 1f);
            block.transform.localPosition = localPosition;
            SetColor(block, color);
        }

        private void SetColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = new Material(Shader.Find("Standard"));
                renderer.sharedMaterial.color = color;
            }
        }
    }
}
