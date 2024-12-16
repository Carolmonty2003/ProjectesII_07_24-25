using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructionMode : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision != null)
        {
            if(collision.gameObject.GetComponent<PlayerController>() != null)
            {
                if (collision.gameObject.GetComponent<PlayerController>().EresMuyGrande())
                {
                    gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                    gameObject.GetComponent<Rigidbody2D>().AddForce((gameObject.transform.position - collision.gameObject.GetComponent<PlayerController>().transform.position).normalized * 500);
                    gameObject.GetComponent<BoxCollider2D>().enabled = false;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (collision.gameObject.GetComponent<PlayerController>() != null)
            {
                if (collision.gameObject.GetComponent<PlayerController>().EresMuyGrande())
                {
                    gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
                    gameObject.GetComponent<Rigidbody2D>().AddForce((gameObject.transform.position - collision.gameObject.GetComponent<PlayerController>().transform.position).normalized * 500);
                    gameObject.GetComponent<BoxCollider2D>().enabled = false;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
