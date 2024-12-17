using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckGrow : MonoBehaviour
{
    public LevelManager levelManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision != null)
        {
            if (collision.transform.CompareTag("Player"))
            {
                
                Debug.Log("Player Grow2");
                levelManager.isEnter = true;
                
            }
        }
    }
}
