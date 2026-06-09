using UnityEngine;

namespace TheCube
{
    public class MazeGateTrigger : MonoBehaviour
    {
        [SerializeField] private MazeGate gate;
        [SerializeField] private string message = "Gate switch activated.";
        private bool activated;

        public void SetGate(MazeGate targetGate)
        {
            gate = targetGate;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (activated || other.GetComponentInParent<PlayerController>() == null)
                return;

            activated = true;
            Debug.Log(message);

            if (gate != null)
                gate.Open();
        }
    }
}
