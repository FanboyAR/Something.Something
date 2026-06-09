using UnityEngine;

namespace TheCube
{
    public class ButtonPad : MonoBehaviour
    {
        public bool isPressed;

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PuzzleBox>() != null)
            {
                isPressed = true;
                Debug.Log($"ButtonPad: pressed by {other.name}");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<PuzzleBox>() != null)
            {
                isPressed = false;
            }
        }

        private void Awake()
        {
            // ensure trigger collider
            var col = GetComponent<Collider>();
            if (col == null)
            {
                var sphere = gameObject.AddComponent<SphereCollider>();
                sphere.isTrigger = true;
                sphere.radius = 0.6f;
            }
            else
            {
                col.isTrigger = true;
            }
        }
    }
}
