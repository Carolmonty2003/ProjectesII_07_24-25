using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearingPlatform : MonoBehaviour
{
    [Header("Platform Settings")]
    public float groupAVisibleTime = 2f;
    public float groupAInvisibleTime = 1f;
    public float groupBVisibleTime = 2f;
    public float groupBInvisibleTime = 1f;
    public bool isGroupA = true; 

    private SpriteRenderer spriteRenderer;
    private Collider2D platformCollider;
    private static bool isGroupAActive = true;
    private bool isVisible;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        platformCollider = GetComponent<Collider2D>();

        isVisible = isGroupA == isGroupAActive;
        SetPlatformState(isVisible);

        if (IsFirstPlatform())
        {
            StartCoroutine(ToggleGroupState());
        }
    }

    IEnumerator ToggleGroupState()
    {
        while (true)
        {
            isGroupAActive = !isGroupAActive;

            float visibleTime = isGroupAActive ? groupAVisibleTime : groupBVisibleTime;
            float invisibleTime = isGroupAActive ? groupAInvisibleTime : groupBInvisibleTime;

            foreach (var platform in FindObjectsOfType<DisappearingPlatform>())
            {
                platform.UpdatePlatformState();
            }

            yield return new WaitForSeconds(visibleTime);

            isGroupAActive = !isGroupAActive;

            foreach (var platform in FindObjectsOfType<DisappearingPlatform>())
            {
                platform.UpdatePlatformState();
            }

            yield return new WaitForSeconds(invisibleTime);
        }
    }

    void UpdatePlatformState()
    {
        isVisible = isGroupA == isGroupAActive;
        SetPlatformState(isVisible);
    }

    void SetPlatformState(bool state)
    {
        spriteRenderer.enabled = state;
        platformCollider.enabled = state;
    }

    bool IsFirstPlatform()
    {
        return FindObjectsOfType<DisappearingPlatform>()[0] == this;
    }
}
