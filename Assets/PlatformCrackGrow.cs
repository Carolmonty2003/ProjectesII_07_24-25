using System.Collections;
using UnityEngine;

public class PlatformCrackGrow : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isPlayerOnPlatform = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
            
            if (playerController != null && playerController.Growing)
            {
                isPlayerOnPlatform = true;
                StartCoroutine(DropPlatform());
            } 
            else 
            {
                isPlayerOnPlatform = false;
            }
        }
    }

    private IEnumerator DropPlatform()
    {
        yield return new WaitForSeconds(0.5f);
        rb.isKinematic = false;
    }
}
