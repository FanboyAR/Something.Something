using UnityEngine;

namespace TheCube
{
    public class SpinningHazard : MonoBehaviour
    {
        public float speed = 45f;
        public float knockbackForce = 7f;
        public float upwardForce = 2f;

        private void Update()
        {
            transform.Rotate(Vector3.up, speed * Time.deltaTime, Space.Self);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            ApplyHit(hit.controller.GetComponent<PlayerController>());
        }

        private void OnCollisionEnter(Collision collision)
        {
            ApplyHit(collision.collider.GetComponentInParent<PlayerController>());
        }

        private void ApplyHit(PlayerController player)
        {
            if (player == null)
                return;

            Vector3 direction = player.transform.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.01f)
                direction = transform.right;

            player.ApplyExternalImpulse(direction.normalized * knockbackForce + Vector3.up * upwardForce);
        }
    }
}
