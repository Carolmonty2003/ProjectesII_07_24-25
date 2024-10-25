using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionCheck : MonoBehaviour
{
    public UnityEvent collisionStay, collisionExit;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerStay2D(Collider2D collision) => collisionStay.Invoke();
    private void OnTriggerExit2D(Collider2D collision) => collisionExit.Invoke();

}
