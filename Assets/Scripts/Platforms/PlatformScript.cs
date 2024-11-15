using UnityEngine;

public class PlatformScript : MonoBehaviour
{
    public Transform endPosition;
    public float speed = 2f;

    private bool isActivated = false;

    void Update()
    {
        if (isActivated)
        {
            transform.position = Vector2.MoveTowards(transform.position, endPosition.position, speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, endPosition.position) < 0.1f)
            {
                isActivated = false;
            }
        }
    }

    public void ActivatePlatform()
    {
        isActivated = true;
    }
}
