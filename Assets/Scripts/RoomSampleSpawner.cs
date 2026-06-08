using UnityEngine;

namespace TheCube
{
    public class RoomSampleSpawner : MonoBehaviour
    {
        public RoomFactory factory;
        private bool hasSpawned;

        private void Awake()
        {
            if (factory == null)
            {
                factory = FindObjectOfType<RoomFactory>();
                if (factory == null)
                {
                    var factoryGO = new GameObject("RoomFactory");
                    factory = factoryGO.AddComponent<RoomFactory>();
                }
            }
        }

        private void Start()
        {
            if (!hasSpawned)
            {
                SpawnSampleRooms();
            }
        }

        public void SpawnSampleRooms()
        {
            if (factory == null)
            {
                Debug.LogWarning("RoomSampleSpawner could not find or create a RoomFactory.");
                return;
            }

            if (hasSpawned)
                return;

            const float offset = 15f;
            factory.CreateRoom(RoomType.Corridor, new Vector3(-offset, 0f, 0f), Quaternion.identity);
            factory.CreateRoom(RoomType.Puzzle, new Vector3(0f, 0f, 0f), Quaternion.identity);
            factory.CreateRoom(RoomType.Challenge, new Vector3(offset, 0f, 0f), Quaternion.identity);
            factory.CreateRoom(RoomType.Treasure, new Vector3(offset * 2f, 0f, 0f), Quaternion.identity);
            hasSpawned = true;
        }
    }
}
