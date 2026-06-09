using UnityEngine;

namespace TheCube
{
    public class MazeHazard : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null)
                return;

            player.RespawnAtCheckpoint("Hazard hit. Returned to the nearest checkpoint.");
        }
    }
}
