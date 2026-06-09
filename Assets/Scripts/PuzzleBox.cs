using UnityEngine;

namespace TheCube
{
    public class PuzzleBox : MonoBehaviour
    {
        public Vector3 setupAnchor;

        public void ResetToAnchor()
        {
            transform.localPosition = setupAnchor;
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}
