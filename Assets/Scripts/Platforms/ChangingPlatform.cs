using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangingPlatform : MonoBehaviour
{
    [Header("Platform Sprites")]
    public Sprite FirstSprite;
    public Sprite SecondSprite;

    [Header("Colliders")]
    public Collider2D FirstCollider;
    public Collider2D SecondCollider;

    [Header("Change Settings")]
    public float changeInterval = 2f;

    private SpriteRenderer spriteRenderer;
    private bool isFirstSprite = true;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sprite = FirstSprite;
        SetColliderState();

        InvokeRepeating(nameof(ChangeShape), changeInterval, changeInterval);
    }

    void ChangeShape()
    {
        isFirstSprite = !isFirstSprite;
        spriteRenderer.sprite = isFirstSprite ? FirstSprite : SecondSprite;

        SetColliderState();
    }

    void SetColliderState()
    {
        FirstCollider.enabled = isFirstSprite;
        SecondCollider.enabled = !isFirstSprite;
    }
}
