using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoodbyeBuddy {
    public class DamageObject : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.transform.CompareTag("Player")){
                Debug.Log("Player Died");
                collision.transform.GetComponent<PlayerRespawn>().PlayerDied();
            }
        }
    }
}
