using UnityEngine;

namespace TheCube
{
    public class PlayerHitbox : MonoBehaviour
    {
        public PlayerController Player { get; private set; }

        private void Awake()
        {
            Player = GetComponentInParent<PlayerController>();
        }
    }
}
