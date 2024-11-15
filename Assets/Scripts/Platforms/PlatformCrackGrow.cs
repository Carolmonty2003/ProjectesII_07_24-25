using System.Collections;
using UnityEngine;

namespace GoodbyeBuddy {
    public class PlatformCrackGrow : MonoBehaviour
    {
        private Rigidbody2D rb;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.isKinematic = true;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
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
            yield return new WaitForSeconds(0.1f);
            rb.isKinematic = false;
        }
    }
}
