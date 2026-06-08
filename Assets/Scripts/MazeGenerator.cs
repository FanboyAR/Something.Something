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
            if (seed == 0)
            {
                seed = Random.Range(1, int.MaxValue);
            }

            Debug.Log($"MazeGenerator: Generating {initialRooms} initial rooms (seed={seed})");

            // Placeholder: reserve space for initial rooms. Actual room creation occurs
            // in the streaming generator later.
            for (int i = 0; i < initialRooms; i++)
            {
                // no-op for now; this uses the `initialRooms` field to avoid compiler warnings
            }

        public void GenerateAroundPlayer(Vector3 playerPosition)
        {
        }
    }
}
