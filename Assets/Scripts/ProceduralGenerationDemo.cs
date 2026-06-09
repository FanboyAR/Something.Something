using UnityEngine;

namespace TheCube
{
    /// <summary>
    /// Demo script for showcasing procedural maze generation.
    /// This script sets up and manages a demo scene where players can walk through
    /// a procedurally generated maze.
    /// </summary>
    public class ProceduralGenerationDemo : MonoBehaviour
    {
        [SerializeField] private MazeGenerator mazeGenerator;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private PlayerController playerController;

        [Header("Demo Settings")]
        [SerializeField] private bool autoStartGeneration = true;
        [SerializeField] private int demoSeed = 12345; // Use a fixed seed for consistent demos

        private void Awake()
        {
            if (mazeGenerator == null)
                mazeGenerator = FindAnyObjectByType<MazeGenerator>();

            if (gameManager == null)
                gameManager = FindAnyObjectByType<GameManager>();

            if (playerController == null)
                playerController = FindAnyObjectByType<PlayerController>();
        }

        private void Start()
        {
            if (autoStartGeneration)
            {
                InitializeDemo();
            }
        }

        /// <summary>
        /// Initializes the procedural generation demo.
        /// </summary>
        public void InitializeDemo()
        {
            if (mazeGenerator == null)
            {
                Debug.LogError("ProceduralGenerationDemo: MazeGenerator reference not assigned!");
                return;
            }

            if (gameManager == null)
            {
                Debug.LogError("ProceduralGenerationDemo: GameManager reference not assigned!");
                return;
            }

            // Set the seed for reproducible generation on the GameManager/LevelManager
            gameManager.SetSeed(demoSeed);

            Debug.Log($"[ProceduralGenerationDemo] Starting maze generation with seed: {demoSeed}");

            // Start the game run which triggers level generation and streaming
            gameManager.StartRun();

            // Reset player position to origin for better visibility
            if (playerController != null)
            {
                playerController.transform.position = new Vector3(0, 5, 0);
                playerController.transform.rotation = Quaternion.identity;
            }

            Debug.Log("[ProceduralGenerationDemo] Demo initialized - Walk around to explore the procedurally generated maze!");
        }

        /// <summary>
        /// Regenerates the maze with a new seed.
        /// </summary>
        public void RegenerateMaze()
        {
            // Generate a new random seed and restart run
            int newSeed = Random.Range(1, int.MaxValue);
            gameManager.SetSeed(newSeed);

            Debug.Log($"[ProceduralGenerationDemo] Regenerating maze with new seed: {newSeed}");
            gameManager.StartRun();
        }

        /// <summary>
        /// Regenerates the maze with a specific seed.
        /// </summary>
        public void RegenerateMazeWithSeed(int seed)
        {
            gameManager.SetSeed(seed);
            Debug.Log($"[ProceduralGenerationDemo] Regenerating maze with seed: {seed}");
            gameManager.StartRun();
        }

        private void Update()
        {
            // Allow regeneration with R key
            if (Input.GetKeyDown(KeyCode.R))
            {
                RegenerateMaze();
            }

            // Display current seed and controls
            if (Input.GetKeyDown(KeyCode.H))
            {
                DisplayHelpText();
            }
        }

        private void DisplayHelpText()
        {
            Debug.Log("=== Procedural Generation Demo Controls ===\n" +
                     "WASD - Move\n" +
                     "Mouse - Look around\n" +
                     "Space - Jump\n" +
                     "Shift - Run\n" +
                     "R - Regenerate maze with new seed\n" +
                     "H - Show this help text\n" +
                     $"Configured Seed: {demoSeed}");
        }
    }
}
