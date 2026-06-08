using UnityEngine;

namespace TheCube
{
    public class RoomSampleSpawner : MonoBehaviour
    {
        public RoomFactory factory;

        private void Start()
        {
            if (factory == null)
            {
                Debug.LogWarning("RoomSampleSpawner requires a RoomFactory reference.");
                return;
            }

            SpawnSampleRooms();
        }

        public void SpawnSampleRooms()
        {
            const float offset = 15f;
            factory.CreateRoom(RoomType.Corridor, new Vector3(-offset, 0f, 0f), Quaternion.identity);
            factory.CreateRoom(RoomType.Puzzle, new Vector3(0f, 0f, 0f), Quaternion.identity);
            factory.CreateRoom(RoomType.Challenge, new Vector3(offset, 0f, 0f), Quaternion.identity);
            factory.CreateRoom(RoomType.Treasure, new Vector3(offset * 2f, 0f, 0f), Quaternion.identity);
        }
    }
}
