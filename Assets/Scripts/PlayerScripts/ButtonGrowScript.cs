using UnityEngine;

namespace GoodbyeBuddy
{
    public class ButtonGrowScript : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out PlayerController player))
            {
                //player.IsBottonActive = true;
                var playerInput = player.GetComponent<PlayerInput>();
                if (playerInput != null)
                {
                    playerInput.IsButtonGrowPressed = true;
                }
            }
        }
    }
}