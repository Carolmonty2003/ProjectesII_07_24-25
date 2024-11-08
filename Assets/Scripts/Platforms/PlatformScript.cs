using UnityEngine;

public class PlatformScript : MonoBehaviour
{
    public Transform endPosition; // El punto final al que se moverá la plataforma
    public float speed = 2f; // Velocidad de movimiento

    private bool isActivated = false;

    void Update()
    {
        if (isActivated)
        {
            // Mueve la plataforma hacia la posición final
            transform.position = Vector2.MoveTowards(transform.position, endPosition.position, speed * Time.deltaTime);

            // Verifica si la plataforma ha llegado a la posición final
            if (Vector2.Distance(transform.position, endPosition.position) < 0.1f)
            {
                isActivated = false; // Detiene el movimiento al llegar al destino
            }
        }
    }

    // Método para activar la plataforma desde el botón
    public void ActivatePlatform()
    {
        isActivated = true;
    }
}
