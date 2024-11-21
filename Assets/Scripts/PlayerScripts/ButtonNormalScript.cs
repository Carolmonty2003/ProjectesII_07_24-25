using UnityEngine;

namespace GoodbyeBuddy
{
    public class ButtonNormalScript : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                if (other.TryGetComponent(out PlayerController player))
                {
                    //player.IsBottonActive = true;
                    //var playerInput = player.GetComponent<PlayerInput>();
                    //if (playerInput != null)
                    //{
                    //    //playerInput.IsButtonNormalPressed = true;
                    //}
                }
            }
        }
    }
}