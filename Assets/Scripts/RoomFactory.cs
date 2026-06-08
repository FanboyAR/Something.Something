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
            // Add a narrow inner corridor with side panels and crates for cover.
            CreateWall(parent, new Vector3(-2.5f, 1.5f, 0f), new Vector3(0.3f, 3f, 8f), new Color(0.75f, 0.75f, 0.8f));
            CreateWall(parent, new Vector3(2.5f, 1.5f, 0f), new Vector3(0.3f, 3f, 8f), new Color(0.75f, 0.75f, 0.8f));

            CreateBlock(parent, new Vector3(-3.2f, 0.5f, -2.5f), "Crate A", new Color(0.45f, 0.3f, 0.2f));
            CreateBlock(parent, new Vector3(3.2f, 0.5f, 2.5f), "Crate B", new Color(0.45f, 0.3f, 0.2f));
            CreateBlock(parent, new Vector3(-1.5f, 0.5f, 2.5f), "Crate C", new Color(0.45f, 0.3f, 0.2f));

            var bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bench.name = "Rest Bench";
            bench.transform.parent = parent;
            bench.transform.localScale = new Vector3(3f, 0.3f, 0.6f);
            bench.transform.localPosition = new Vector3(0f, 0.15f, 4f);
            SetColor(bench, new Color(0.4f, 0.4f, 0.5f));

            var lamp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lamp.name = "Hanging Lamp";
            lamp.transform.parent = parent;
            lamp.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            lamp.transform.localPosition = new Vector3(0f, 2.7f, 0f);
            SetColor(lamp, new Color(1f, 0.9f, 0.7f));
        }

        private void CreatePuzzleDetail(Transform parent, float roomSize)
        {
            CreatePlate(parent, new Vector3(-3f, 0.01f, 1.5f), "Pressure Plate A");
            CreatePlate(parent, new Vector3(3f, 0.01f, 1.5f), "Pressure Plate B");
            CreatePlate(parent, new Vector3(0f, 0.01f, -1.5f), "Pressure Plate C");

            CreateBlock(parent, new Vector3(0f, 0.5f, 3f), "Puzzle Cube", new Color(0.35f, 0.74f, 0.98f));

            CreateWall(parent, new Vector3(0f, 1.5f, 0f), new Vector3(6f, 3f, 0.3f), new Color(0.7f, 0.7f, 0.85f));
            CreateWall(parent, new Vector3(-2.2f, 1.5f, -2.5f), new Vector3(0.3f, 3f, 4f), new Color(0.7f, 0.7f, 0.85f));

            var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "Puzzle Platform";
            platform.transform.parent = parent;
            platform.transform.localScale = new Vector3(4f, 0.2f, 4f);
            platform.transform.localPosition = new Vector3(0f, 0.1f, -3f);
            SetColor(platform, new Color(0.55f, 0.55f, 0.7f));

            var gateLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gateLeft.name = "Puzzle Gate Left";
            gateLeft.transform.parent = parent;
            gateLeft.transform.localScale = new Vector3(0.3f, 2f, 0.8f);
            gateLeft.transform.localPosition = new Vector3(-1.5f, 1f, 4.5f);
            SetColor(gateLeft, new Color(0.4f, 0.4f, 0.5f));

            var gateRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gateRight.name = "Puzzle Gate Right";
            gateRight.transform.parent = parent;
            gateRight.transform.localScale = new Vector3(0.3f, 2f, 0.8f);
            gateRight.transform.localPosition = new Vector3(1.5f, 1f, 4.5f);
            SetColor(gateRight, new Color(0.4f, 0.4f, 0.5f));

            var topBeam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topBeam.name = "Puzzle Gate Top";
            topBeam.transform.parent = parent;
            topBeam.transform.localScale = new Vector3(3.6f, 0.3f, 0.3f);
            topBeam.transform.localPosition = new Vector3(0f, 2f, 4.5f);
            SetColor(topBeam, new Color(0.4f, 0.4f, 0.5f));
        }

        private void CreateChallengeDetail(Transform parent, float roomSize)
        {
            var spinnerA = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spinnerA.name = "Spinning Hazard A";
            spinnerA.transform.parent = parent;
            spinnerA.transform.localScale = new Vector3(0.4f, 0.1f, 3.5f);
            spinnerA.transform.localPosition = new Vector3(-2f, 1.2f, 0f);
            spinnerA.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            SetColor(spinnerA, new Color(0.9f, 0.2f, 0.2f));
            spinnerA.AddComponent<Spinner>();

            var spinnerB = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spinnerB.name = "Spinning Hazard B";
            spinnerB.transform.parent = parent;
            spinnerB.transform.localScale = new Vector3(0.4f, 0.1f, 3.5f);
            spinnerB.transform.localPosition = new Vector3(2f, 1.2f, 0f);
            spinnerB.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            SetColor(spinnerB, new Color(0.9f, 0.25f, 0.25f));
            spinnerB.AddComponent<Spinner>();

            for (int i = 0; i < 4; i++)
            {
                var step = GameObject.CreatePrimitive(PrimitiveType.Cube);
                step.name = "Challenge Step " + (i + 1);
                step.transform.parent = parent;
                step.transform.localScale = new Vector3(1.5f, 0.3f, 1.5f);
                step.transform.localPosition = new Vector3(-3f + i * 2f, 0.15f, -2.5f + i * 1.5f);
                SetColor(step, new Color(0.75f, 0.35f, 0.35f));
            }

            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Challenge Partition";
            wall.transform.parent = parent;
            wall.transform.localScale = new Vector3(6f, 2.5f, 0.3f);
            wall.transform.localPosition = new Vector3(0f, 1.25f, 2.5f);
            SetColor(wall, new Color(0.7f, 0.3f, 0.3f));
        }

        private void CreateTreasureDetail(Transform parent, float roomSize)
        {
            CreateWall(parent, new Vector3(0f, 1.5f, -2.5f), new Vector3(6f, 3f, 0.3f), new Color(0.75f, 0.7f, 0.3f));
            CreateWall(parent, new Vector3(-2.5f, 1f, 0f), new Vector3(0.3f, 2f, 4f), new Color(0.75f, 0.7f, 0.3f));
            CreateWall(parent, new Vector3(2.5f, 1f, 0f), new Vector3(0.3f, 2f, 4f), new Color(0.75f, 0.7f, 0.3f));

            var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Treasure Pedestal";
            pedestal.transform.parent = parent;
            pedestal.transform.localScale = new Vector3(1.5f, 0.5f, 1.5f);
            pedestal.transform.localPosition = new Vector3(0f, 0.25f, 3f);
            SetColor(pedestal, new Color(0.6f, 0.5f, 0.15f));

            var chest = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chest.name = "Treasure Chest";
            chest.transform.parent = parent;
            chest.transform.localScale = new Vector3(1.2f, 0.7f, 1f);
            chest.transform.localPosition = new Vector3(0f, 0.45f, 3.8f);
            SetColor(chest, new Color(0.45f, 0.25f, 0.05f));

            var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "Treasure Orb";
            orb.transform.parent = parent;
            orb.transform.localScale = Vector3.one * 0.8f;
            orb.transform.localPosition = new Vector3(0f, 1.1f, 3.8f);
            SetColor(orb, new Color(1f, 0.9f, 0.2f));

            var leftPillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leftPillar.name = "Treasure Pillar Left";
            leftPillar.transform.parent = parent;
            leftPillar.transform.localScale = new Vector3(0.3f, 2f, 0.3f);
            leftPillar.transform.localPosition = new Vector3(-1.5f, 1f, 3.5f);
            SetColor(leftPillar, new Color(0.7f, 0.55f, 0.25f));

            var rightPillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rightPillar.name = "Treasure Pillar Right";
            rightPillar.transform.parent = parent;
            rightPillar.transform.localScale = new Vector3(0.3f, 2f, 0.3f);
            rightPillar.transform.localPosition = new Vector3(1.5f, 1f, 3.5f);
            SetColor(rightPillar, new Color(0.7f, 0.55f, 0.25f));

            var topBeam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topBeam.name = "Treasure Arch";
            topBeam.transform.parent = parent;
            topBeam.transform.localScale = new Vector3(3.6f, 0.3f, 0.3f);
            topBeam.transform.localPosition = new Vector3(0f, 2f, 3.5f);
            SetColor(topBeam, new Color(0.7f, 0.55f, 0.25f));
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
