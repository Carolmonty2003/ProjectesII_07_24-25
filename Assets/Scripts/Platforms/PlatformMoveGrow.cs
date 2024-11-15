using UnityEngine;

namespace GoodbyeBuddy {
    public class PlatformMoveGrow : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public Transform leftLimit;
        public Transform rightLimit;

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerInput playerInput = collision.gameObject.GetComponent<PlayerInput>();

                if (playerInput != null && playerInput.Gather().Grow)
                {
                    float direction = collision.transform.position.x > transform.position.x ? -1 : 1;

                    if ((direction < 0 && transform.position.x > leftLimit.position.x) ||
                        (direction > 0 && transform.position.x < rightLimit.position.x))
                    {
                        MovePlatform(direction);
                    }
                }
            }
        }

        private void MovePlatform(float direction)
        {
            transform.position = new Vector2(transform.position.x + direction * moveSpeed * Time.deltaTime, transform.position.y);
        }
    }
}
