using UnityEngine;

namespace TheCube
{
    public class MazeFinishZone : MonoBehaviour
    {
        private bool completed;

        private void OnTriggerEnter(Collider other)
        {
            if (completed || other.GetComponentInParent<PlayerController>() == null)
                return;

            if (!MazeRunState.HasExitKey)
            {
                Debug.Log("The finish is locked. Find the exit key first.");
                return;
            }

            completed = true;
            MazeRunState.CompleteLevel();

            var generator = FindAnyObjectByType<MazeGenerator>();
            if (generator != null)
            {
                generator.AdvanceToNextMaze(other.GetComponentInParent<PlayerController>());
                return;
            }

            Debug.Log("Level complete! No MazeGenerator was found to create the next level.");
        }
    }
}
