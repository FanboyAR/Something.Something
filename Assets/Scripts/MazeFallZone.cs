using UnityEngine;

namespace TheCube
{
    public class MazeFallZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player == null)
                return;

            player.RespawnAtCheckpoint("Fell into the pit. Returned to the nearest checkpoint.");
        }
    }
}
