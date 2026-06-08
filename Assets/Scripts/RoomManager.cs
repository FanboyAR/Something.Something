using UnityEngine;

namespace TheCube
{
    public class RoomManager : MonoBehaviour
    {
        [SerializeField] private Transform roomParent;

        public void SpawnRoom(GameObject roomPrefab, Vector3 position, Quaternion rotation)
        {
            Instantiate(roomPrefab, position, rotation, roomParent);
        }

        public void RecycleRoom(GameObject room)
        {
            Destroy(room);
        }
    }
}
