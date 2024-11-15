using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    public PlatformScript platformScript;

    private bool isPressed = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isPressed)
        {
            isPressed = true;
            platformScript.ActivatePlatform();
            GetComponent<SpriteRenderer>().color = Color.gray;
        }
    }
}
