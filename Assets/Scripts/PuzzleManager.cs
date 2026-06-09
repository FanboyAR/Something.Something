using UnityEngine;

namespace TheCube
{
    public class PuzzleManager : MonoBehaviour
    {
        private ButtonPad button;
        private PuzzleBox box;
        private MazeGate gate;

        public void Register(ButtonPad b, PuzzleBox pb)
        {
            button = b;
            box = pb;
        }

        public void Register(ButtonPad b, PuzzleBox pb, MazeGate progressGate)
        {
            button = b;
            box = pb;
            gate = progressGate;
        }

        private void Update()
        {
            if (button == null || box == null) return;
            if (button.isPressed)
            {
                OnSolved();
            }
        }

        void OnSolved()
        {
            Debug.Log($"PuzzleManager: puzzle solved in {transform.parent?.name}");
            if (gate != null)
                gate.Open();

            Destroy(this);
        }
    }
}
