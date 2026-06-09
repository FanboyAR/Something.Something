using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheCube
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private MazeGenerator mazeGenerator;
        [SerializeField] private Transform player;
        [SerializeField] private int initialWidth = 8;
        [SerializeField] private int initialHeight = 8;
        [SerializeField] private float preloadDistance = 30f;
        [SerializeField] private int initialSeed = 0;

        private int currentIndex = 0;
        private string currentSceneName;
        private string nextSceneName;
        private Vector3 currentExitPos;
        private Vector3 nextExitPos;

        private bool isPreloading = false;
        private Scene currentScene;

        private void Awake()
        {
            if (mazeGenerator == null)
                mazeGenerator = FindAnyObjectByType<MazeGenerator>();
        }

        public void StartRun()
        {
            if (player == null)
            {
                var pgo = GameObject.Find("Player");
                if (pgo != null)
                    player = pgo.transform;
                else if (Camera.main != null)
                    player = Camera.main.transform;
            }

            Debug.Log($"LevelManager.StartRun: player={(player!=null?player.name:"<null>")}, mazeGenerator={(mazeGenerator!=null?mazeGenerator.name:"<null>")}");

            if (mazeGenerator == null)
            {
                Debug.LogError("LevelManager.StartRun: mazeGenerator is null — cannot generate level. Ensure MazeGenerator is present and assigned.");
                return;
            }

            StartCoroutine(GenerateAndEnterInitial());
        }

        public void SetInitialSeed(int seed)
        {
            initialSeed = seed;
        }

        IEnumerator GenerateAndEnterInitial()
        {
            currentSceneName = $"Level_{currentIndex}";
            int seed = initialSeed != 0 ? initialSeed : Random.Range(1, int.MaxValue);
            Debug.Log($"LevelManager.GenerateAndEnterInitial: starting generation for {currentSceneName} seed={seed} size={initialWidth}x{initialHeight}");
            yield return StartCoroutine(mazeGenerator.GenerateLevelCoroutine(currentSceneName, initialWidth, initialHeight, seed, (scene, exitPos) =>
            {
                currentScene = scene;
                currentExitPos = exitPos;
            }));

            Debug.Log($"LevelManager.GenerateAndEnterInitial: generation complete for {currentSceneName}. currentExitPos={currentExitPos}");

            // Move player to start (move the player root GameObject to the new scene)
            if (player != null)
            {
                player.position = new Vector3(0, 1f, 0);
                var playerRoot = GetPlayerRoot(player);
                if (playerRoot != null)
                    SceneManager.MoveGameObjectToScene(playerRoot, currentScene);
            }

            // Start monitoring for preload
            StartCoroutine(WatchForPreload());
        }

        IEnumerator WatchForPreload()
        {
            while (true)
            {
                if (!isPreloading && player != null)
                {
                    float d = Vector3.Distance(player.position, currentExitPos);
                    if (d <= preloadDistance)
                    {
                        StartCoroutine(PreloadNext());
                    }
                }
                yield return new WaitForSeconds(0.5f);
            }
        }

        IEnumerator PreloadNext()
        {
            isPreloading = true;
            int nextIndex = currentIndex + 1;
            nextSceneName = $"Level_{nextIndex}";

            yield return StartCoroutine(mazeGenerator.GenerateLevelCoroutine(nextSceneName, initialWidth, initialHeight, Random.Range(1, int.MaxValue), (scene, exitPos) =>
            {
                nextExitPos = exitPos;
            }));

            // Wait until player crosses into next scene (simple distance check to next exit center)
            while (Vector3.Distance(player.position, nextExitPos) > preloadDistance / 2f)
            {
                yield return null;
            }

            // Move player to next scene and unload previous
            Scene nextScene = SceneManager.GetSceneByName(nextSceneName);
            if (nextScene.IsValid())
            {
                var playerRoot = GetPlayerRoot(player);
                if (playerRoot != null)
                    SceneManager.MoveGameObjectToScene(playerRoot, nextScene);
                // unload previous
                if (currentScene.IsValid())
                    SceneManager.UnloadSceneAsync(currentScene);

                currentScene = nextScene;
                currentSceneName = nextSceneName;
                currentExitPos = nextExitPos;
                currentIndex = nextIndex;
                isPreloading = false;
            }
        }

        GameObject GetPlayerRoot(Transform t)
        {
            if (t == null) return null;
            var pc = t.GetComponentInParent<PlayerController>();
            if (pc != null) return pc.gameObject;
            var found = GameObject.Find("Player");
            if (found != null) return found;
            return t.gameObject;
        }
    }
}
