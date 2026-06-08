using UnityEngine;

namespace TheCube
{
    public class SaveSystem : MonoBehaviour
    {
        public void SaveSeed(int seed)
        {
            PlayerPrefs.SetInt("seed", seed);
            PlayerPrefs.Save();
        }

        public int LoadSeed(int defaultSeed = 0)
        {
            return PlayerPrefs.GetInt("seed", defaultSeed);
        }
    }
}
