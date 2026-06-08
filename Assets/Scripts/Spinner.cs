using UnityEngine;

namespace TheCube
{
    public class Spinner : MonoBehaviour
    {
        public float speed = 120f;

        private void Update()
        {
            transform.Rotate(Vector3.up, speed * Time.deltaTime, Space.Self);
        }
    }
}
