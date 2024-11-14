using UnityEngine;

namespace GoodbyeBuddy
{
    public class HandleShrinkScript : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                if (other.TryGetComponent(out PlayerController player))
                {
                    //player.ActivateShrink(); // Allow shrinking when lever is activated
                }
            }
        }
    }
}
