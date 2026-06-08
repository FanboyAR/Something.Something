using UnityEngine;

namespace TheCube
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private MazeGenerator mazeGenerator;

        public void StartRun()
        {
            mazeGenerator.GenerateInitialLayout();
        }

        public void EndRun()
        {
        }
    }
}
