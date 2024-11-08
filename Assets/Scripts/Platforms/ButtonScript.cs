using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    public PlatformScript platformScript; // Referencia al script de la plataforma

    private bool isPressed = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isPressed)
        {
            isPressed = true;
            platformScript.ActivatePlatform(); // Llama a la función en el script de la plataforma
            // Opcional: Cambia el aspecto del botón para indicar que está desactivado
            GetComponent<SpriteRenderer>().color = Color.gray;
        }
    }
}
