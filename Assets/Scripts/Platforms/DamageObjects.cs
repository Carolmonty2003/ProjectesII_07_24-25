using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageObjects : MonoBehaviour 
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        { 
            if (!collision.gameObject.GetComponent<PlayerController>().EresMuyGrande())
            {
                Debug.Log("Player Died");
                collision.transform.GetComponent<PlayerRespawn>().PlayerDied();
            }
        }
    }

    public void PlatformSpikes(){
        
    }
}