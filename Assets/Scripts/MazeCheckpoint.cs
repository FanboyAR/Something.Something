using UnityEngine;

namespace TheCube
{
    public class MazeCheckpoint : MonoBehaviour
    {
        [SerializeField] private Transform respawnPoint;

        public void SetRespawnPoint(Transform target)
        {
            respawnPoint = target;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<PlayerController>() == null)
                return;

            Vector3 position = respawnPoint != null ? respawnPoint.position : transform.position + Vector3.up;
            MazeRunState.SetCheckpoint(position);
        }
    }
}
