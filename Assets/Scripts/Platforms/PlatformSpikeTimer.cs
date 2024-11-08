using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSpikeTimer : MonoBehaviour
{
    public float spikeInterval = 5f;        // Tiempo entre activaciones de los pinchos
    public float spikeDuration = 3f;        // Tiempo que los pinchos están activos

    private SpriteRenderer spriteRenderer;  // Referencia al SpriteRenderer
    private Collider2D spikeCollider;       // Referencia al Collider2D

     private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spikeCollider = GetComponent<Collider2D>();

        StartCoroutine(SpikeCycle());
    }

     private IEnumerator SpikeCycle()
     {
        while (true)
        {
            // Activa los pinchos
            ActivateSpikes();

            // Espera el tiempo que los pinchos están activos
            yield return new WaitForSeconds(spikeDuration);

               // Desactiva los pinchos
            DeactivateSpikes();

            // Espera el tiempo de intervalo entre activaciones
            yield return new WaitForSeconds(spikeInterval);
        }
    }

    private void ActivateSpikes()
    {
        spriteRenderer.enabled = true;  // Muestra el sprite de los pinchos
        spikeCollider.enabled = true;   // Activa el daño de los pinchos
    }

    private void DeactivateSpikes()
    {
        spriteRenderer.enabled = false; // Oculta el sprite de los pinchos
        spikeCollider.enabled = false;  // Desactiva el daño de los pinchos
    }
}

