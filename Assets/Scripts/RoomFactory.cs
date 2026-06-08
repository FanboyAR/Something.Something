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
            // Clean corridor with a clear path and cover on the sides.
            CreateWall(parent, new Vector3(-2f, 1.5f, 0f), new Vector3(0.2f, 3f, 8f), new Color(0.78f, 0.78f, 0.82f));
            CreateWall(parent, new Vector3(2f, 1.5f, 0f), new Vector3(0.2f, 3f, 8f), new Color(0.78f, 0.78f, 0.82f));

            CreateBlock(parent, new Vector3(-2.5f, 0.5f, -2f), "Cover Crate A", new Color(0.43f, 0.33f, 0.22f));
            CreateBlock(parent, new Vector3(2.5f, 0.5f, 2f), "Cover Crate B", new Color(0.43f, 0.33f, 0.22f));

            var panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            panel.name = "Side Panel";
            panel.transform.parent = parent;
            panel.transform.localScale = new Vector3(0.2f, 1.5f, 4f);
            panel.transform.localPosition = new Vector3(0f, 1f, 3f);
            SetColor(panel, new Color(0.65f, 0.65f, 0.7f));

            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "Corridor Marker";
            marker.transform.parent = parent;
            marker.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);
            marker.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            SetColor(marker, new Color(0.9f, 0.6f, 0.2f));
        }

        private void CreatePuzzleDetail(Transform parent, float roomSize)
        {
            var divider = GameObject.CreatePrimitive(PrimitiveType.Cube);
            divider.name = "Puzzle Divider";
            divider.transform.parent = parent;
            divider.transform.localScale = new Vector3(6f, 2.5f, 0.2f);
            divider.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            SetColor(divider, new Color(0.72f, 0.72f, 0.82f));

            CreatePlate(parent, new Vector3(-2f, 0.01f, 3f), "Plate Left");
            CreatePlate(parent, new Vector3(2f, 0.01f, 3f), "Plate Right");
            CreatePlate(parent, new Vector3(0f, 0.01f, -3f), "Plate Center");

            CreateBlock(parent, new Vector3(0f, 0.5f, 0f), "Puzzle Cube", new Color(0.35f, 0.74f, 0.98f));
            CreateBlock(parent, new Vector3(-1.8f, 0.5f, -1.2f), "Puzzle Block A", new Color(0.5f, 0.5f, 0.9f));
            CreateBlock(parent, new Vector3(1.8f, 0.5f, 1.2f), "Puzzle Block B", new Color(0.5f, 0.5f, 0.9f));

            CreateWall(parent, new Vector3(0f, 1.5f, -4.5f), new Vector3(6f, 3f, 0.2f), new Color(0.7f, 0.7f, 0.82f));
            CreateWall(parent, new Vector3(-2.8f, 1.5f, 2.5f), new Vector3(0.2f, 3f, 4f), new Color(0.7f, 0.7f, 0.82f));
            CreateWall(parent, new Vector3(2.8f, 1.5f, 2.5f), new Vector3(0.2f, 3f, 4f), new Color(0.7f, 0.7f, 0.82f));
        }

        private void CreateChallengeDetail(Transform parent, float roomSize)
        {
            var hazardA = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hazardA.name = "Spinning Hazard A";
            hazardA.transform.parent = parent;
            hazardA.transform.localScale = new Vector3(0.3f, 0.1f, 3.2f);
            hazardA.transform.localPosition = new Vector3(-1.5f, 1.2f, 0f);
            hazardA.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            SetColor(hazardA, new Color(0.9f, 0.25f, 0.25f));
            hazardA.AddComponent<Spinner>();

            var hazardB = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hazardB.name = "Spinning Hazard B";
            hazardB.transform.parent = parent;
            hazardB.transform.localScale = new Vector3(0.3f, 0.1f, 3.2f);
            hazardB.transform.localPosition = new Vector3(1.5f, 1.2f, 0f);
            hazardB.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            SetColor(hazardB, new Color(0.9f, 0.25f, 0.25f));
            hazardB.AddComponent<Spinner>();

            for (int i = 0; i < 3; i++)
            {
                var step = GameObject.CreatePrimitive(PrimitiveType.Cube);
                step.name = "Challenge Step " + (i + 1);
                step.transform.parent = parent;
                step.transform.localScale = new Vector3(1.2f, 0.2f, 1.2f);
                step.transform.localPosition = new Vector3(-1.5f + i * 1.5f, 0.1f, -3f + i * 2f);
                SetColor(step, new Color(0.75f, 0.35f, 0.35f));
            }

            var ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ramp.name = "Challenge Ramp";
            ramp.transform.parent = parent;
            ramp.transform.localScale = new Vector3(2f, 0.2f, 3f);
            ramp.transform.localPosition = new Vector3(0f, 0.1f, 3f);
            SetColor(ramp, new Color(0.65f, 0.35f, 0.35f));
            ramp.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
        }

        private void CreateTreasureDetail(Transform parent, float roomSize)
        {
            CreateWall(parent, new Vector3(0f, 1.5f, -2.5f), new Vector3(6f, 3f, 0.2f), new Color(0.75f, 0.7f, 0.3f));
            CreateWall(parent, new Vector3(-2.5f, 1.5f, 1f), new Vector3(0.2f, 2f, 4f), new Color(0.75f, 0.7f, 0.3f));
            CreateWall(parent, new Vector3(2.5f, 1.5f, 1f), new Vector3(0.2f, 2f, 4f), new Color(0.75f, 0.7f, 0.3f));

            var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Treasure Pedestal";
            pedestal.transform.parent = parent;
            pedestal.transform.localScale = new Vector3(1f, 0.4f, 1f);
            pedestal.transform.localPosition = new Vector3(0f, 0.2f, 3f);
            SetColor(pedestal, new Color(0.6f, 0.5f, 0.15f));

            var chest = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chest.name = "Treasure Chest";
            chest.transform.parent = parent;
            chest.transform.localScale = new Vector3(1f, 0.6f, 0.8f);
            chest.transform.localPosition = new Vector3(0f, 0.5f, 3.7f);
            SetColor(chest, new Color(0.45f, 0.25f, 0.05f));

            var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "Treasure Orb";
            orb.transform.parent = parent;
            orb.transform.localScale = Vector3.one * 0.7f;
            orb.transform.localPosition = new Vector3(0f, 1f, 3.7f);
            SetColor(orb, new Color(1f, 0.9f, 0.2f));

            var arch = GameObject.CreatePrimitive(PrimitiveType.Cube);
            arch.name = "Treasure Arch";
            arch.transform.parent = parent;
            arch.transform.localScale = new Vector3(2.2f, 0.3f, 0.3f);
            arch.transform.localPosition = new Vector3(0f, 1.8f, 3.5f);
            SetColor(arch, new Color(0.7f, 0.55f, 0.25f));
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
