using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SM_Door : MonoBehaviour
{
    public Sprite closedSprite;
    public Sprite openSprite;
    SpriteRenderer spriteRenderer;

    public SM_Switch controllingSwitch;

    public BoxCollider2D closedCollider;
    public BoxCollider2D openColliderLeft;
    public BoxCollider2D openColliderRight;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (controllingSwitch.isPressed)
        {
            spriteRenderer.sprite = openSprite;

            closedCollider.enabled = false;
            openColliderLeft.enabled = true;
            openColliderRight.enabled = true;
        }
        else
        {
            spriteRenderer.sprite = closedSprite;

            closedCollider.enabled = true;
            openColliderLeft.enabled = false;
            openColliderRight.enabled = false;
        }
    }
}
