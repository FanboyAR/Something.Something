using UnityEngine;

namespace TheCube
{
    public class MazeGenerator : MonoBehaviour
    {
        [SerializeField] private int seed = 0;
        [SerializeField] private int initialRooms = 12;

        public int Seed => seed;

        public void GenerateInitialLayout()
        {
        }

        public void GenerateAroundPlayer(Vector3 playerPosition)
        {
        }
    }
}
