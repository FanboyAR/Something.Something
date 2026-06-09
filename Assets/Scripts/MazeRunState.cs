using UnityEngine;

namespace TheCube
{
    public static class MazeRunState
    {
        public static bool HasExitKey { get; private set; }
        public static MazeKeyPickup CarriedKey { get; private set; }
        public static Vector3 CheckpointPosition { get; private set; }

        public static void Reset()
        {
            HasExitKey = false;
            CarriedKey = null;
            CheckpointPosition = new Vector3(0f, 1f, 0f);
        }

        public static void SetCheckpoint(Vector3 position)
        {
            CheckpointPosition = position;
        }

        public static void CollectExitKey(MazeKeyPickup key)
        {
            HasExitKey = true;
            CarriedKey = key;
            Debug.Log("Exit key picked up. Bring it to the finish portal.");
        }

        public static void CompleteLevel()
        {
            if (CarriedKey != null)
                Object.Destroy(CarriedKey.gameObject);

            HasExitKey = false;
            CarriedKey = null;
        }
    }
}
