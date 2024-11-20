using UnityEngine;
using UnityEngine.InputSystem;

namespace GoodbyeBuddy
{
    public class PlayerInput : MonoBehaviour
    {
        void OnMove(InputValue value)
        {
            Move = value;
        }
        void OnJump(InputValue value)
        {
            Debug.Log("aa");
        }
        void OnInteraction(InputValue value)
        {
            Debug.Log("aa");
        }


    }



}
