using UnityEngine;

namespace GoodbyeBuddy
{
    public class ButtonScript : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out PlayerController player))
            {
                //player.IsBottonActive = true;
                var playerInput = player.GetComponent<PlayerInput>();
                if (playerInput != null)
                {
                    playerInput.IsButtonPressed = true;
                }
            }
        }
        //private void OnTriggerExit2D(Collider2D other)
        //{
        //    if (other.TryGetComponent(out PlayerController player))
        //    {
        //        player.IsBottonActive = false;
        //    }
        //}
    }
}