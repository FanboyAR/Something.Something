using UnityEngine;

namespace TheCube
{
    public class HazardManager : MonoBehaviour
    {
        public void SpawnHazard(GameObject hazardPrefab, Vector3 position)
        {
            Instantiate(hazardPrefab, position, Quaternion.identity);
        }

        public void ClearHazards()
        {
        }
    }
}
