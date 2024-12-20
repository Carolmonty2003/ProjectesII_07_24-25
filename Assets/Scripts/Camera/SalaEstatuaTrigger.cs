using UnityEngine;
using Cinemachine;
using System.Collections;

public class SalaEstatuaTrigger : MonoBehaviour
{
    public CinemachineVirtualCamera camaraJugador; // Cámara que sigue al jugador
    public CinemachineVirtualCamera camaraEstatua; // Cámara que muestra la estatua
    public PlayerController playerController; // Controlador del jugador

    [Header("Duración de la Animación")]
    public float duracionAnimacion = 2f; // Tiempo para simular la animación de la estatua

    private bool eventoActivado = false; // Bandera para asegurarnos de que ocurre solo una vez
    private Animator playerAnimator; // Referencia al Animator del jugador

    private void Start()
    {
        if (playerController != null)
        {
            playerAnimator = playerController.GetComponent<Animator>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !eventoActivado)
        {
            StartCoroutine(ActivarEvento());
        }
    }

    private IEnumerator ActivarEvento()
    {
        eventoActivado = true;

        // 1. Bloquear el control del jugador y detener su movimiento
        if (playerController != null)
        {
            playerController.enabled = false;

            // Detener el movimiento, por si el jugador está en movimiento
            playerController.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

            // Forzar el estado de animación Idle
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("IsWalking", false);
                playerAnimator.SetBool("IsJumping", false);
                playerAnimator.SetBool("IsFalling", false);
                playerAnimator.SetBool("IsGliding", false);
            }
        }

        // 2. Cambiar a la cámara de la estatua
        camaraJugador.Priority = 0; // Bajar prioridad de la cámara del jugador
        camaraEstatua.Priority = 10; // Aumentar prioridad de la cámara de la estatua

        // 3. Simular la animación (espera la duración)
        Debug.Log("Reproduciendo animación de la estatua...");
        yield return new WaitForSeconds(duracionAnimacion);

        // 4. Cambiar de nuevo a la cámara del jugador
        camaraEstatua.Priority = 0; // Bajar prioridad de la cámara de la estatua
        camaraJugador.Priority = 10; // Aumentar prioridad de la cámara del jugador

        // 5. Restaurar el control del jugador
        if (playerController != null)
        {
            playerController.enabled = true;

            // Dejar que el Animator vuelva a comportarse de manera normal
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("IsWalking", false);
            }
        }

        Debug.Log("Animación terminada, control restaurado.");
    }
}
