using UnityEngine;

namespace TheCube
{
    public class MazeModule : MonoBehaviour
    {
        public RoomType roomType;

        [Header("Wall Pieces")]
        public GameObject wallNorth;
        public GameObject wallEast;
        public GameObject wallSouth;
        public GameObject wallWest;

        [Header("Connection Points")]
        public Transform connectionNorth;
        public Transform connectionEast;
        public Transform connectionSouth;
        public Transform connectionWest;

        public bool opensNorth = true;
        public bool opensEast = true;
        public bool opensSouth = true;
        public bool opensWest = true;
        public bool canAdaptOpenings;

        public bool Matches(bool northOpen, bool eastOpen, bool southOpen, bool westOpen)
        {
            if (canAdaptOpenings)
                return true;

            return opensNorth == northOpen
                && opensEast == eastOpen
                && opensSouth == southOpen
                && opensWest == westOpen;
        }

        public void ConfigureWalls(bool northWall, bool eastWall, bool southWall, bool westWall)
        {
            SetActive(wallNorth, northWall);
            SetActive(wallEast, eastWall);
            SetActive(wallSouth, southWall);
            SetActive(wallWest, westWall);
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
                target.SetActive(active);
        }
    }
}
