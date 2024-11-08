using System.Collections;
using UnityEngine;

namespace GoodbyeBuddy {
    public class PlatformCrackGrow : MonoBehaviour
    {
        private Rigidbody2D rb;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.isKinematic = true; // Se asegura de que la plataforma no caiga de inmediato
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Detecta si el jugador está tocando la plataforma y si está en estado Grow
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerInput playerInput = collision.gameObject.GetComponent<PlayerInput>();
                
                if (playerInput != null && playerInput.Gather().Grow)
                {
                    StartCoroutine(DropPlatform());
                }
            }
        }

        private IEnumerator DropPlatform()
        {
            // Espera un pequeño tiempo antes de que la plataforma caiga
            yield return new WaitForSeconds(0.1f);
            rb.isKinematic = false; // Desactiva el modo cinemático para que la física controle la plataforma
        }
    }
}
