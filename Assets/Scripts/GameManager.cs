using UnityEngine;

namespace TheCube
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private LevelManager levelManager;

        private void Awake()
        {
            if (levelManager == null)
                levelManager = FindAnyObjectByType<LevelManager>();
        }

        public void StartRun()
        {
            if (levelManager != null)
                levelManager.StartRun();
            else
                Debug.LogError("GameManager: LevelManager is not assigned");
        }

        public void SetSeed(int seed)
        {
            if (levelManager != null)
                levelManager.SetInitialSeed(seed);
        }

        public void EndRun()
        {
        }
    }
}
