using UnityEngine;

namespace TheCube
{
    public class MazeGate : MonoBehaviour
    {
        private bool isOpen;
        private bool setCheckpointOnOpen;
        private Vector3 checkpointPosition;

        public void SetCheckpointOnOpen(Vector3 position)
        {
            checkpointPosition = position;
            setCheckpointOnOpen = true;
        }

        public void Open()
        {
            if (isOpen)
                return;

            isOpen = true;
            if (setCheckpointOnOpen)
                MazeRunState.SetCheckpoint(checkpointPosition);

            gameObject.SetActive(false);
            Debug.Log("Progress gate opened.");
        }
    }
}
