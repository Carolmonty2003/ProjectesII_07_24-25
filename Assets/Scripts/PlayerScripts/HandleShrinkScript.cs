using UnityEngine;

namespace GoodbyeBuddy
{
    public class HandleShrinkScript : MonoBehaviour
    {
        [SerializeField] private bool _isActivated;
        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                if (other.TryGetComponent(out PlayerController player))
                {
                    if (!_isActivated)
                    {
                        if (Input.GetKey(KeyCode.E))
                        {
                            _isActivated=true;
                            player.ActivateShrink(); // Allow shrinking when lever is activated
                        }
                    }
                }
            }
        }
    }
}
