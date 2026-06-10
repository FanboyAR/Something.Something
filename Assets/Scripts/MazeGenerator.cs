using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheCube
{
    // Simple procedural maze generator that instantiates rooms and walls into a newly created Scene.
    public class MazeGenerator : MonoBehaviour
    {
        [Header("Prefabs (optional)")]
        [SerializeField] private GameObject chunkPrefab;
        [SerializeField] private GameObject corridorPrefab;
        [SerializeField] private MazeModule[] modulePrefabs;

        [Header("Layout Settings")]
        public int roomSize = 10;
        [Tooltip("Spacing between chunk centers. Overrides roomSize for placement when > 0.")]
        public int chunkSpacing = 10;
        [Tooltip("Approximate main path length (for testing). Not currently used by DFS generator but exposed for future use.")]
        public int mainPathLength = 8;
        public int sideBranchLength = 2;
        public int maxSideBranches = 2;

        [Header("Standalone Test")]
        [SerializeField] private bool autoGenerateIfNoDemo = true;
        [SerializeField] private int testWidth = 8;
        [SerializeField] private int testHeight = 8;
        [SerializeField] private int testSeed = 12345;

        private bool hasStartedStandaloneGeneration;
        private bool isTransitioningLevel;
        private int currentLevelIndex;
        private int lastSeed;
        private Scene currentGeneratedScene;
        private readonly System.Random seedRandom = new System.Random();

        private void Start()
        {
            if (!ShouldRunStandaloneTest())
                return;

            hasStartedStandaloneGeneration = true;
            int seed = GetInitialSeed();
            StartCoroutine(GenerateLevelCoroutine(GetLevelSceneName(), testWidth, testHeight, seed, (scene, exitPos) =>
            {
                currentGeneratedScene = scene;
                lastSeed = seed;
                PlacePlayerForStandaloneTest();
                Debug.Log($"MazeGenerator: standalone test maze generated. Exit is near {exitPos}.");
            }));
        }

        public void AdvanceToNextMaze(PlayerController player)
        {
            if (isTransitioningLevel)
                return;

            StartCoroutine(AdvanceToNextMazeCoroutine(player));
        }

        private IEnumerator AdvanceToNextMazeCoroutine(PlayerController player)
        {
            isTransitioningLevel = true;

            Scene previousScene = currentGeneratedScene;
            currentLevelIndex++;
            int seed = GetNextSeed();

            yield return StartCoroutine(GenerateLevelCoroutine(GetLevelSceneName(), testWidth, testHeight, seed, (scene, exitPos) =>
            {
                currentGeneratedScene = scene;
                lastSeed = seed;
            }));

            PlacePlayerAtStart(player);

            if (previousScene.IsValid() && previousScene.isLoaded)
                yield return SceneManager.UnloadSceneAsync(previousScene);

            isTransitioningLevel = false;
            Debug.Log($"Advanced to maze level {currentLevelIndex + 1} with seed {lastSeed}.");
        }

        public IEnumerator GenerateLevelCoroutine(string sceneName, int width, int height, int seed, System.Action<Scene, Vector3> onComplete)
        {
            Debug.Log($"MazeGenerator: GenerateLevelCoroutine(scene={sceneName}, width={width}, height={height}, seed={seed})");
            Random.InitState(seed);

            // Create new scene
            Scene newScene = SceneManager.CreateScene(sceneName);
            Debug.Log($"MazeGenerator: created scene '{sceneName}' (valid={newScene.IsValid()})");

            // Generate maze using DFS
            Cell[,] grid = new Cell[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = new Cell(x, y);

            Stack<Cell> stack = new Stack<Cell>();
            Cell current = grid[0, 0];
            current.visited = true;
            int visitedCount = 1;
            int total = width * height;

            while (visitedCount < total)
            {
                var neighbors = GetUnvisitedNeighbors(current, grid, width, height);
                if (neighbors.Count > 0)
                {
                    var next = neighbors[Random.Range(0, neighbors.Count)];
                    RemoveWallBetween(current, next);
                    stack.Push(current);
                    current = next;
                    current.visited = true;
                    visitedCount++;
                }
                else if (stack.Count > 0)
                {
                    current = stack.Pop();
                }
                else
                {
                    break;
                }
            }

            MazeRunState.Reset();
            int[,] distances = CalculateDistances(grid, width, height);
            Vector2Int finishCell = FindFarthestCell(distances, width, height);
            Vector2Int keyCell = FindKeyCell(distances, width, height, distances[finishCell.x, finishCell.y]);
            float spacing = chunkSpacing > 0 ? chunkSpacing : roomSize;

            // Instantiate rooms
            for (int x = 0; x < width; x++)
            {
                // log row
                for (int y = 0; y < height; y++)
                {
                    var cell = grid[x, y];
                    Vector3 roomPos = new Vector3(x * spacing, 0, y * spacing);
                    GameObject roomGO;
                    RoomType roomType = ChooseRoomType(x, y, finishCell, keyCell, HasForwardNeighbor(cell, grid, width, height, distances));

                    MazeModule modulePrefab = FindModulePrefab(
                        roomType,
                        !cell.wallNorth,
                        !cell.wallEast,
                        !cell.wallSouth,
                        !cell.wallWest);

                    if (modulePrefab != null)
                    {
                        roomGO = Instantiate(modulePrefab.gameObject, roomPos, Quaternion.identity);
                        roomGO.name = $"Room_{x}_{y}";

                        var module = roomGO.GetComponent<MazeModule>();
                        if (module != null)
                            module.ConfigureWalls(cell.wallNorth, cell.wallEast, cell.wallSouth, cell.wallWest);
                    }
                    else if (chunkPrefab != null)
                    {
                        roomGO = Instantiate(chunkPrefab, roomPos, Quaternion.identity);
                        roomGO.name = $"Room_{x}_{y}";

                        var module = roomGO.GetComponent<MazeModule>();
                        if (module != null)
                            module.ConfigureWalls(cell.wallNorth, cell.wallEast, cell.wallSouth, cell.wallWest);
                    }
                    else
                    {
                        roomGO = new GameObject($"Room_{x}_{y}");
                        roomGO.transform.position = roomPos;

                        // Floor
                        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        floor.name = "Floor";
                        floor.transform.SetParent(roomGO.transform, false);
                        floor.transform.localScale = new Vector3(roomSize, 0.5f, roomSize);
                        floor.transform.localPosition = new Vector3(0, -0.25f, 0);

                        // Walls
                        float half = roomSize / 2f;
                        if (cell.wallNorth)
                        {
                            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            wall.transform.SetParent(roomGO.transform, false);
                            wall.transform.localScale = new Vector3(roomSize, 3f, 0.5f);
                            wall.transform.localPosition = new Vector3(0, 1f, half);
                            wall.name = "Wall_North";
                        }
                        if (cell.wallSouth)
                        {
                            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            wall.transform.SetParent(roomGO.transform, false);
                            wall.transform.localScale = new Vector3(roomSize, 3f, 0.5f);
                            wall.transform.localPosition = new Vector3(0, 1f, -half);
                            wall.name = "Wall_South";
                        }
                        if (cell.wallEast)
                        {
                            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            wall.transform.SetParent(roomGO.transform, false);
                            wall.transform.localScale = new Vector3(0.5f, 3f, roomSize);
                            wall.transform.localPosition = new Vector3(half, 1f, 0);
                            wall.name = "Wall_East";
                        }
                        if (cell.wallWest)
                        {
                            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            wall.transform.SetParent(roomGO.transform, false);
                            wall.transform.localScale = new Vector3(0.5f, 3f, roomSize);
                            wall.transform.localPosition = new Vector3(-half, 1f, 0);
                            wall.name = "Wall_West";
                        }
                    }

                    // Optional puzzle placement: simple rule - place a puzzle if random chance
                    if (roomType == RoomType.Puzzle)
                    {
                        MazeDirection forwardDirection = GetForwardDirection(cell, grid, width, height, distances);
                        MazeGate gate = CreateProgressGate(roomGO.transform, forwardDirection);

                        // Spawn a button and box
                        GameObject buttonGO = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        buttonGO.name = "ButtonPad";
                        buttonGO.transform.SetParent(roomGO.transform, false);
                        buttonGO.transform.localScale = new Vector3(1f, 0.2f, 1f);
                        buttonGO.transform.localPosition = new Vector3(-2.5f, 0.1f, 2.5f);
                        var button = buttonGO.AddComponent<ButtonPad>();

                        GameObject boxGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        boxGO.name = "PuzzleBox";
                        boxGO.transform.SetParent(roomGO.transform, false);
                        boxGO.transform.localScale = Vector3.one * 1f;
                        boxGO.transform.localPosition = new Vector3(2.5f, 0.6f, -2.5f);
                        var rb = boxGO.AddComponent<Rigidbody>();
                        rb.mass = 8f;
                        rb.linearDamping = 3f;
                        rb.angularDamping = 4f;
                        var box = boxGO.AddComponent<PuzzleBox>();
                        box.setupAnchor = boxGO.transform.localPosition;

                        var puzzleManagerGO = new GameObject("PuzzleManager");
                        puzzleManagerGO.transform.SetParent(roomGO.transform, false);
                        var pm = puzzleManagerGO.AddComponent<PuzzleManager>();
                        pm.Register(button, box, gate);
                    }

                    CreateWallTopBarriers(roomGO.transform, cell);
                    AddRoomGameplay(roomGO.transform, roomType, x, y, finishCell, keyCell, distances[x, y], cell, grid, width, height, distances);

                    // Move to scene
                    SceneManager.MoveGameObjectToScene(roomGO, newScene);

                    // occasional log for visibility
                    if ((x * height + y) % 20 == 0)
                        Debug.Log($"MazeGenerator: instantiated room {x},{y} into scene {sceneName}");

                    // Yield a frame occasionally to avoid hitches
                    if ((x * height + y) % 20 == 0)
                        yield return null;
                }
            }

            Vector3 exitPos = new Vector3(finishCell.x * spacing, 0, finishCell.y * spacing);

            Debug.Log($"MazeGenerator: finished instantiating rooms for scene {sceneName}. exitPos={exitPos}");
            onComplete?.Invoke(newScene, exitPos);
        }

        bool ShouldRunStandaloneTest()
        {
            if (!autoGenerateIfNoDemo || hasStartedStandaloneGeneration)
                return false;

            if (FindAnyObjectByType<ProceduralGenerationDemo>() != null)
                return false;

            var levelManager = FindAnyObjectByType<LevelManager>();
            if (levelManager != null && levelManager.enabled)
                return false;

            return true;
        }

        void PlacePlayerForStandaloneTest()
        {
            var playerController = FindAnyObjectByType<PlayerController>();
            GameObject playerGO;

            if (playerController != null)
            {
                playerGO = playerController.gameObject;
            }
            else
            {
                playerGO = GameObject.Find("Player");
                if (playerGO == null)
                    playerGO = new GameObject("Player");

                var characterController = playerGO.GetComponent<CharacterController>();
                if (characterController == null)
                    characterController = playerGO.AddComponent<CharacterController>();

                characterController.height = 1.8f;
                characterController.radius = 0.4f;
                characterController.center = new Vector3(0f, 0.9f, 0f);

                playerController = playerGO.GetComponent<PlayerController>();
                if (playerController == null)
                    playerController = playerGO.AddComponent<PlayerController>();
            }

            playerGO.transform.position = new Vector3(0f, 1f, 0f);
            playerGO.transform.rotation = Quaternion.identity;
            playerController.ResetMotion();

            Transform cameraHolder = playerGO.transform.Find("CameraHolder");
            if (cameraHolder == null)
            {
                var cameraHolderGO = new GameObject("CameraHolder");
                cameraHolderGO.transform.SetParent(playerGO.transform, false);
                cameraHolder = cameraHolderGO.transform;
            }

            cameraHolder.localPosition = new Vector3(0f, 0.6f, 0f);
            cameraHolder.localRotation = Quaternion.identity;

            Camera camera = cameraHolder.GetComponent<Camera>();
            if (camera == null)
            {
                camera = cameraHolder.gameObject.AddComponent<Camera>();
            }

            var listener = cameraHolder.GetComponent<AudioListener>();
            if (listener == null)
                cameraHolder.gameObject.AddComponent<AudioListener>();

            cameraHolder.gameObject.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.backgroundColor = new Color(0.19f, 0.3f, 0.47f);

            playerController.cameraHolder = cameraHolder;
            DisableOtherCameras(camera);
        }

        void PlacePlayerAtStart(PlayerController player)
        {
            if (player == null)
            {
                PlacePlayerForStandaloneTest();
                player = FindAnyObjectByType<PlayerController>();
            }

            if (player == null)
                return;

            player.transform.position = new Vector3(0f, 1f, 0f);
            player.transform.rotation = Quaternion.identity;
            player.ResetMotion();

            var playerRoot = player.gameObject;
            if (currentGeneratedScene.IsValid())
                SceneManager.MoveGameObjectToScene(playerRoot, currentGeneratedScene);
        }

        int GetInitialSeed()
        {
            return testSeed != 0 ? testSeed : GetNextSeed();
        }

        int GetNextSeed()
        {
            int seed;
            do
            {
                seed = seedRandom.Next(1, int.MaxValue);
            }
            while (seed == lastSeed);

            return seed;
        }

        string GetLevelSceneName()
        {
            return "MazeLevel_" + currentLevelIndex;
        }

        void DisableOtherCameras(Camera activeCamera)
        {
            var cameras = FindObjectsByType<Camera>(FindObjectsInactive.Include);
            for (int i = 0; i < cameras.Length; i++)
            {
                var camera = cameras[i];
                if (camera == activeCamera)
                    continue;

                if (camera.CompareTag("MainCamera"))
                    camera.gameObject.tag = "Untagged";

                camera.enabled = false;
                var listener = camera.GetComponent<AudioListener>();
                if (listener != null)
                    listener.enabled = false;
            }
        }

        RoomType ChooseRoomType(int x, int y, Vector2Int finishCell, Vector2Int keyCell, bool hasForwardNeighbor)
        {
            if (x == finishCell.x && y == finishCell.y)
                return RoomType.Treasure;

            if (x == 0 && y == 0)
                return RoomType.Corridor;

            if (x == keyCell.x && y == keyCell.y)
                return RoomType.Puzzle;

            if (!hasForwardNeighbor)
                return RoomType.Corridor;

            float roll = Random.value;
            if (roll < 0.08f)
                return RoomType.Puzzle;
            if (roll < 0.12f)
                return RoomType.Challenge;

            return RoomType.Corridor;
        }

        MazeModule FindModulePrefab(RoomType roomType, bool northOpen, bool eastOpen, bool southOpen, bool westOpen)
        {
            if (modulePrefabs != null)
            {
                for (int i = 0; i < modulePrefabs.Length; i++)
                {
                    var module = modulePrefabs[i];
                    if (module != null && module.roomType == roomType && module.Matches(northOpen, eastOpen, southOpen, westOpen))
                        return module;
                }
            }

            if (roomType == RoomType.Corridor && corridorPrefab != null)
            {
                var corridorModule = corridorPrefab.GetComponent<MazeModule>();
                if (corridorModule != null && corridorModule.Matches(northOpen, eastOpen, southOpen, westOpen))
                    return corridorModule;
            }

            if (roomType != RoomType.Corridor)
                return FindModulePrefab(RoomType.Corridor, northOpen, eastOpen, southOpen, westOpen);

            return null;
        }

        void AddRoomGameplay(Transform room, RoomType roomType, int x, int y, Vector2Int finishCell, Vector2Int keyCell, int distanceFromStart, Cell cell, Cell[,] grid, int width, int height, int[,] distances)
        {
            if (x == 0 && y == 0)
            {
                CreateMarker(room, "Start Marker", new Vector3(0f, 0.05f, 0f), new Vector3(3f, 0.1f, 3f), new Color(0.2f, 0.8f, 0.45f));
                CreatePillar(room, "Start Beacon", new Vector3(0f, 1.5f, -2.5f), new Color(0.2f, 0.8f, 0.45f));
                CreateLabel(room, "START", new Vector3(0f, 2.6f, -2.5f), new Color(0.2f, 0.9f, 0.5f));
                return;
            }

            if (x == keyCell.x && y == keyCell.y)
                CreateKeyPickup(room);

            if (x == finishCell.x && y == finishCell.y)
            {
                CreateFinishPortal(room, GetForwardDirection(cell, grid, width, height, distances));
                return;
            }

            if (roomType == RoomType.Challenge)
            {
                MazeDirection forwardDirection = GetForwardDirection(cell, grid, width, height, distances);
                MazeGate gate = CreateProgressGate(room, forwardDirection);

                Vector3 forward = GetDirectionVector(forwardDirection);
                Vector3 right = GetRightVector(forwardDirection);
                CreateChallengePit(room, forwardDirection);
                CreateCheckpoint(room, -forward * 3f);
                CreateSpinningBeam(room, new Vector3(0f, 1f, 0f), forwardDirection);
                CreatePillar(room, "Hazard Warning", new Vector3(0f, 1f, 0f), new Color(0.95f, 0.2f, 0.15f));
                CreateGateSwitch(room, forwardDirection, gate);
                return;
            }

            if (roomType == RoomType.Corridor && distanceFromStart > 1 && Random.value < 0.35f)
                CreateObstacleCluster(room, cell);
        }

        void CreateKeyPickup(Transform parent)
        {
            var key = GameObject.CreatePrimitive(PrimitiveType.Cube);
            key.name = "Exit Key";
            key.transform.SetParent(parent, false);
            key.transform.localPosition = new Vector3(0f, 1.25f, -1.8f);
            key.transform.localRotation = Quaternion.Euler(20f, 35f, 0f);
            key.transform.localScale = Vector3.one * 0.55f;
            SetObjectColor(key, new Color(1f, 0.85f, 0.15f));

            var collider = key.GetComponent<Collider>();
            if (collider != null)
                collider.isTrigger = true;

            key.AddComponent<MazeKeyPickup>();

            CreatePillar(parent, "Key Beacon", new Vector3(0f, 1.2f, -3f), new Color(1f, 0.85f, 0.15f));
            CreateLabel(parent, "KEY", new Vector3(0f, 2.8f, -3f), new Color(1f, 0.85f, 0.15f));
        }

        void CreateFinishPortal(Transform parent, MazeDirection facingDirection)
        {
            Vector3 forward = GetDirectionVector(facingDirection);
            Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);
            var basePlate = CreateMarker(parent, "Finish Plate", forward * 1.5f + new Vector3(0f, 0.06f, 0f), new Vector3(3.6f, 0.12f, 2f), new Color(0.95f, 0.7f, 0.18f));
            basePlate.transform.localRotation = rotation;
            basePlate.AddComponent<MazeFinishZone>();

            var collider = basePlate.GetComponent<Collider>();
            if (collider != null)
                collider.isTrigger = true;

            Vector3 right = new Vector3(forward.z, 0f, -forward.x);
            CreatePillar(parent, "Finish Beacon Left", forward * 2.2f - right * 1.4f + new Vector3(0f, 1.5f, 0f), new Color(0.95f, 0.7f, 0.18f));
            CreatePillar(parent, "Finish Beacon Right", forward * 2.2f + right * 1.4f + new Vector3(0f, 1.5f, 0f), new Color(0.95f, 0.7f, 0.18f));
            CreateLabel(parent, "FINISH", forward * 2.2f + new Vector3(0f, 2.8f, 0f), new Color(0.95f, 0.7f, 0.18f));
        }

        GameObject CreateMarker(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Color color)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = name;
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = localPosition;
            marker.transform.localScale = localScale;
            SetObjectColor(marker, color);
            return marker;
        }

        void CreatePillar(Transform parent, string name, Vector3 localPosition, Color color)
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = name;
            pillar.transform.SetParent(parent, false);
            pillar.transform.localPosition = localPosition;
            pillar.transform.localScale = new Vector3(0.35f, 1.5f, 0.35f);
            SetObjectColor(pillar, color);
        }

        void CreateLabel(Transform parent, string text, Vector3 localPosition, Color color)
        {
            var label = new GameObject(text + " Label");
            label.transform.SetParent(parent, false);
            label.transform.localPosition = localPosition;
            label.transform.localRotation = Quaternion.Euler(65f, 0f, 0f);

            var mesh = label.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.characterSize = 0.45f;
            mesh.fontSize = 72;
            mesh.color = color;
        }

        void CreateHazard(Transform parent, Vector3 localPosition, Vector3 localScale)
        {
            var hazard = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hazard.name = "Reset Hazard";
            hazard.transform.SetParent(parent, false);
            hazard.transform.localPosition = localPosition;
            hazard.transform.localScale = localScale;
            SetObjectColor(hazard, new Color(0.95f, 0.1f, 0.08f));

            var collider = hazard.GetComponent<Collider>();
            if (collider != null)
                collider.isTrigger = true;

            hazard.AddComponent<MazeHazard>();
            hazard.AddComponent<Spinner>();
        }

        void CreateChallengePit(Transform parent, MazeDirection direction)
        {
            Vector3 forward = GetDirectionVector(direction);
            foreach (Transform child in parent)
            {
                if (child.name == "Floor")
                {
                    Destroy(child.gameObject);
                    break;
                }
            }

            Vector3 platformScale = IsNorthSouth(direction) ? new Vector3(5f, 0.3f, 2.2f) : new Vector3(2.2f, 0.3f, 5f);
            CreateMarker(parent, "Entry Platform", -forward * 3.6f + new Vector3(0f, -0.15f, 0f), platformScale, new Color(0.38f, 0.4f, 0.38f));
            CreateMarker(parent, "Exit Platform", forward * 3.6f + new Vector3(0f, -0.15f, 0f), platformScale, new Color(0.38f, 0.4f, 0.38f));
            CreateMarker(parent, "Middle Platform", new Vector3(0f, -0.05f, 0f), new Vector3(2.2f, 0.25f, 2.2f), new Color(0.48f, 0.5f, 0.46f));

            var fallZone = CreateMarker(parent, "Fall Respawn Zone", new Vector3(0f, -1.8f, 0f), new Vector3(14f, 1f, 14f), new Color(0.05f, 0.05f, 0.06f));
            fallZone.GetComponent<Renderer>().enabled = false;
            var collider = fallZone.GetComponent<Collider>();
            if (collider != null)
                collider.isTrigger = true;
            fallZone.AddComponent<MazeFallZone>();
        }

        void CreateCheckpoint(Transform parent, Vector3 localPosition)
        {
            var checkpoint = CreateMarker(parent, "Checkpoint", localPosition + new Vector3(0f, 0.08f, 0f), new Vector3(2f, 0.12f, 2f), new Color(0.15f, 0.75f, 0.9f));
            var collider = checkpoint.GetComponent<Collider>();
            if (collider != null)
                collider.isTrigger = true;

            var respawn = new GameObject("Respawn Point");
            respawn.transform.SetParent(parent, false);
            respawn.transform.localPosition = localPosition + new Vector3(0f, 1f, 0f);

            var checkpointScript = checkpoint.AddComponent<MazeCheckpoint>();
            checkpointScript.SetRespawnPoint(respawn.transform);
        }

        void CreateSpinningBeam(Transform parent, Vector3 localPosition, MazeDirection direction)
        {
            var beam = CreateMarker(parent, "Spinning Beam Hazard", localPosition, new Vector3(0.45f, 0.45f, 5.5f), new Color(0.95f, 0.1f, 0.08f));
            beam.transform.localRotation = IsNorthSouth(direction) ? Quaternion.identity : Quaternion.Euler(0f, 90f, 0f);
            var collider = beam.GetComponent<Collider>();
            if (collider != null)
                collider.isTrigger = false;

            var body = beam.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;

            var hazard = beam.AddComponent<SpinningHazard>();
            hazard.speed = 45f;
            hazard.knockbackForce = 7f;
            hazard.upwardForce = 2f;
        }

        MazeGate CreateProgressGate(Transform parent, MazeDirection direction)
        {
            Vector3 localPosition = GetDoorPosition(direction);
            Vector3 localScale = IsNorthSouth(direction) ? new Vector3(roomSize, 2.6f, 0.35f) : new Vector3(0.35f, 2.6f, roomSize);

            var gateObject = CreateMarker(parent, "Locked Progress Gate", localPosition + Vector3.up * 1.25f, localScale, new Color(0.12f, 0.18f, 0.24f));
            CreateInvisibleBarrier(parent, "Gate Anti-Skip Barrier", localPosition + Vector3.up * 4f, IsNorthSouth(direction) ? new Vector3(roomSize, 3f, 0.6f) : new Vector3(0.6f, 3f, roomSize));

            var gate = gateObject.AddComponent<MazeGate>();
            gate.SetCheckpointOnOpen(parent.TransformPoint(GetDirectionVector(direction) * 4f + Vector3.up));
            return gate;
        }

        void CreateWallTopBarriers(Transform parent, Cell cell)
        {
            float half = roomSize * 0.5f;
            if (cell.wallNorth)
                CreateInvisibleBarrier(parent, "North Wall Anti-Skip Barrier", new Vector3(0f, 4.5f, half), new Vector3(roomSize, 3f, 0.6f));
            if (cell.wallEast)
                CreateInvisibleBarrier(parent, "East Wall Anti-Skip Barrier", new Vector3(half, 4.5f, 0f), new Vector3(0.6f, 3f, roomSize));
            if (cell.wallSouth)
                CreateInvisibleBarrier(parent, "South Wall Anti-Skip Barrier", new Vector3(0f, 4.5f, -half), new Vector3(roomSize, 3f, 0.6f));
            if (cell.wallWest)
                CreateInvisibleBarrier(parent, "West Wall Anti-Skip Barrier", new Vector3(-half, 4.5f, 0f), new Vector3(0.6f, 3f, roomSize));
        }

        GameObject CreateInvisibleBarrier(Transform parent, string name, Vector3 localPosition, Vector3 localScale)
        {
            var barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrier.name = name;
            barrier.transform.SetParent(parent, false);
            barrier.transform.localPosition = localPosition;
            barrier.transform.localScale = localScale;

            var renderer = barrier.GetComponent<Renderer>();
            if (renderer != null)
                renderer.enabled = false;

            return barrier;
        }

        void CreateGateSwitch(Transform parent, MazeDirection direction, MazeGate gate)
        {
            Vector3 switchPosition = GetDoorPosition(direction) * 0.55f;
            switchPosition.y = 0.08f;

            var switchObject = CreateMarker(parent, "Gate Switch", switchPosition, new Vector3(1.6f, 0.12f, 1.6f), new Color(0.2f, 0.65f, 0.95f));
            var collider = switchObject.GetComponent<Collider>();
            if (collider != null)
                collider.isTrigger = true;

            var trigger = switchObject.AddComponent<MazeGateTrigger>();
            trigger.SetGate(gate);
        }

        MazeDirection GetForwardDirection(Cell cell, Cell[,] grid, int width, int height, int[,] distances)
        {
            int currentDistance = distances[cell.x, cell.y];
            MazeDirection fallback = MazeDirection.North;
            bool hasFallback = false;

            foreach (var neighbor in GetOpenNeighbors(cell, grid, width, height))
            {
                MazeDirection direction = GetDirectionTo(cell, neighbor);
                if (!hasFallback)
                {
                    fallback = direction;
                    hasFallback = true;
                }

                if (distances[neighbor.x, neighbor.y] > currentDistance)
                    return direction;
            }

            return fallback;
        }

        bool HasForwardNeighbor(Cell cell, Cell[,] grid, int width, int height, int[,] distances)
        {
            int currentDistance = distances[cell.x, cell.y];
            foreach (var neighbor in GetOpenNeighbors(cell, grid, width, height))
            {
                if (distances[neighbor.x, neighbor.y] > currentDistance)
                    return true;
            }

            return false;
        }

        MazeDirection GetDirectionTo(Cell from, Cell to)
        {
            if (to.y > from.y) return MazeDirection.North;
            if (to.y < from.y) return MazeDirection.South;
            if (to.x > from.x) return MazeDirection.East;
            return MazeDirection.West;
        }

        Vector3 GetDoorPosition(MazeDirection direction)
        {
            float half = roomSize * 0.5f;
            switch (direction)
            {
                case MazeDirection.North:
                    return new Vector3(0f, 0f, half);
                case MazeDirection.East:
                    return new Vector3(half, 0f, 0f);
                case MazeDirection.South:
                    return new Vector3(0f, 0f, -half);
                default:
                    return new Vector3(-half, 0f, 0f);
            }
        }

        Vector3 GetDirectionVector(MazeDirection direction)
        {
            switch (direction)
            {
                case MazeDirection.North:
                    return Vector3.forward;
                case MazeDirection.East:
                    return Vector3.right;
                case MazeDirection.South:
                    return Vector3.back;
                default:
                    return Vector3.left;
            }
        }

        Vector3 GetRightVector(MazeDirection direction)
        {
            Vector3 forward = GetDirectionVector(direction);
            return new Vector3(forward.z, 0f, -forward.x);
        }

        bool IsNorthSouth(MazeDirection direction)
        {
            return direction == MazeDirection.North || direction == MazeDirection.South;
        }

        void CreateObstacleCluster(Transform parent, Cell cell)
        {
            bool northSouth = !cell.wallNorth || !cell.wallSouth;
            int variant = Random.Range(0, 3);

            if (variant == 0)
            {
                CreateBlockingWallWithGap(parent, northSouth, -1.8f);
            }
            else if (variant == 1)
            {
                CreateBlockingWallWithGap(parent, northSouth, 1.8f);
                CreateMarker(parent, "Offset Cover", northSouth ? new Vector3(-2.2f, 0.45f, 1.8f) : new Vector3(1.8f, 0.45f, -2.2f), new Vector3(1.4f, 0.9f, 1.4f), new Color(0.36f, 0.32f, 0.27f));
            }
            else
            {
                CreateMarker(parent, "Narrowing Block A", northSouth ? new Vector3(-2.8f, 0.8f, 0f) : new Vector3(0f, 0.8f, -2.8f), northSouth ? new Vector3(2.8f, 1.6f, 1.2f) : new Vector3(1.2f, 1.6f, 2.8f), new Color(0.36f, 0.32f, 0.27f));
                CreateMarker(parent, "Narrowing Block B", northSouth ? new Vector3(2.8f, 0.8f, 0f) : new Vector3(0f, 0.8f, 2.8f), northSouth ? new Vector3(2.8f, 1.6f, 1.2f) : new Vector3(1.2f, 1.6f, 2.8f), new Color(0.36f, 0.32f, 0.27f));
            }
        }

        void CreateBlockingWallWithGap(Transform parent, bool northSouth, float gapOffset)
        {
            if (northSouth)
            {
                CreateMarker(parent, "Barricade Left", new Vector3(-3.2f, 1f, 0f), new Vector3(3.6f, 2f, 0.45f), new Color(0.36f, 0.32f, 0.27f));
                CreateMarker(parent, "Barricade Right", new Vector3(2.2f, 1f, 0f), new Vector3(2.4f, 2f, 0.45f), new Color(0.36f, 0.32f, 0.27f));
                CreateMarker(parent, "Gap Marker", new Vector3(gapOffset, 0.04f, -0.6f), new Vector3(1.5f, 0.08f, 1.2f), new Color(0.18f, 0.5f, 0.65f));
            }
            else
            {
                CreateMarker(parent, "Barricade Bottom", new Vector3(0f, 1f, -3.2f), new Vector3(0.45f, 2f, 3.6f), new Color(0.36f, 0.32f, 0.27f));
                CreateMarker(parent, "Barricade Top", new Vector3(0f, 1f, 2.2f), new Vector3(0.45f, 2f, 2.4f), new Color(0.36f, 0.32f, 0.27f));
                CreateMarker(parent, "Gap Marker", new Vector3(-0.6f, 0.04f, gapOffset), new Vector3(1.2f, 0.08f, 1.5f), new Color(0.18f, 0.5f, 0.65f));
            }
        }

        void SetObjectColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer == null)
                return;

            renderer.sharedMaterial = new Material(Shader.Find("Standard"));
            renderer.sharedMaterial.color = color;
        }

        int[,] CalculateDistances(Cell[,] grid, int width, int height)
        {
            int[,] distances = new int[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                distances[x, y] = -1;

            var queue = new Queue<Cell>();
            distances[0, 0] = 0;
            queue.Enqueue(grid[0, 0]);

            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                foreach (var neighbor in GetOpenNeighbors(cell, grid, width, height))
                {
                    if (distances[neighbor.x, neighbor.y] >= 0)
                        continue;

                    distances[neighbor.x, neighbor.y] = distances[cell.x, cell.y] + 1;
                    queue.Enqueue(neighbor);
                }
            }

            return distances;
        }

        Vector2Int FindFarthestCell(int[,] distances, int width, int height)
        {
            var best = new Vector2Int(width - 1, height - 1);
            int bestDistance = -1;

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (distances[x, y] > bestDistance)
                {
                    bestDistance = distances[x, y];
                    best = new Vector2Int(x, y);
                }
            }

            return best;
        }

        Vector2Int FindKeyCell(int[,] distances, int width, int height, int finishDistance)
        {
            int targetDistance = Mathf.Max(2, Mathf.RoundToInt(finishDistance * 0.55f));
            var best = new Vector2Int(Mathf.Min(1, width - 1), Mathf.Min(1, height - 1));
            int bestDelta = int.MaxValue;

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if ((x == 0 && y == 0) || distances[x, y] < 2)
                    continue;

                int delta = Mathf.Abs(distances[x, y] - targetDistance);
                if (delta < bestDelta)
                {
                    bestDelta = delta;
                    best = new Vector2Int(x, y);
                }
            }

            return best;
        }

        List<Cell> GetUnvisitedNeighbors(Cell c, Cell[,] grid, int width, int height)
        {
            var list = new List<Cell>();
            int x = c.x, y = c.y;
            if (y + 1 < height && !grid[x, y + 1].visited) list.Add(grid[x, y + 1]);
            if (y - 1 >= 0 && !grid[x, y - 1].visited) list.Add(grid[x, y - 1]);
            if (x + 1 < width && !grid[x + 1, y].visited) list.Add(grid[x + 1, y]);
            if (x - 1 >= 0 && !grid[x - 1, y].visited) list.Add(grid[x - 1, y]);
            return list;
        }

        List<Cell> GetOpenNeighbors(Cell c, Cell[,] grid, int width, int height)
        {
            var list = new List<Cell>();
            int x = c.x, y = c.y;
            if (!c.wallNorth && y + 1 < height) list.Add(grid[x, y + 1]);
            if (!c.wallSouth && y - 1 >= 0) list.Add(grid[x, y - 1]);
            if (!c.wallEast && x + 1 < width) list.Add(grid[x + 1, y]);
            if (!c.wallWest && x - 1 >= 0) list.Add(grid[x - 1, y]);
            return list;
        }

        void RemoveWallBetween(Cell a, Cell b)
        {
            if (a.x == b.x)
            {
                if (a.y < b.y)
                {
                    a.wallNorth = false;
                    b.wallSouth = false;
                }
                else
                {
                    a.wallSouth = false;
                    b.wallNorth = false;
                }
            }
            else if (a.y == b.y)
            {
                if (a.x < b.x)
                {
                    a.wallEast = false;
                    b.wallWest = false;
                }
                else
                {
                    a.wallWest = false;
                    b.wallEast = false;
                }
            }
        }

        class Cell
        {
            public int x, y;
            public bool visited;
            public bool wallNorth = true, wallSouth = true, wallEast = true, wallWest = true;

            public Cell(int x, int y) { this.x = x; this.y = y; }
        }

        enum MazeDirection
        {
            North,
            East,
            South,
            West
        }
    }
}

