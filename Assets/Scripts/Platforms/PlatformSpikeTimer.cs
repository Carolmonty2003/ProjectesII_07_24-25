using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSpikeTimer : MonoBehaviour
{
    public float spikeInterval = 5f;
    public float spikeDuration = 3f;

    private SpriteRenderer spriteRenderer;
    private Collider2D spikeCollider;

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
            ActivateSpikes();

            yield return new WaitForSeconds(spikeDuration);

            DeactivateSpikes();

            yield return new WaitForSeconds(spikeInterval);
        }
    }

    private void ActivateSpikes()
    {
        spriteRenderer.enabled = true;
        spikeCollider.enabled = true;
    }

    private void DeactivateSpikes()
    {
        spriteRenderer.enabled = false;
        spikeCollider.enabled = false;
    }
}

