using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class InputManagerTest : MonoBehaviour
{
    public static Action jumped = delegate { };
    public static Action jumping;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) 
            jumped.Invoke();
        if(Input.GetKeyUp(KeyCode.Space))
            jumping.Invoke();
    }

}
