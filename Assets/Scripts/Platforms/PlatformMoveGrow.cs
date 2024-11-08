using UnityEngine;

namespace GoodbyeBuddy {
    public class PlatformMoveGrow : MonoBehaviour
    {
        public float moveSpeed = 5f;           // Velocidad de movimiento
        public Transform leftLimit;            // Límite izquierdo
        public Transform rightLimit;           // Límite derecho

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerInput playerInput = collision.gameObject.GetComponent<PlayerInput>();

                if (playerInput != null && playerInput.Gather().Grow)
                {
                    // Verifica si el jugador está a la derecha o izquierda de la plataforma
                    float direction = collision.transform.position.x > transform.position.x ? -1 : 1;

                    // Mueve la plataforma si no ha alcanzado el límite correspondiente
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
            // Mueve la plataforma en la dirección dada (izquierda o derecha)
            transform.position = new Vector2(transform.position.x + direction * moveSpeed * Time.deltaTime, transform.position.y);
        }
    }
}
