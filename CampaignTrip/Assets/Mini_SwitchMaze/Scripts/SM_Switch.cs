using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SM_Switch : MonoBehaviour
{
    public Sprite offSprite;
    public Sprite onSprite;
    private SpriteRenderer sr;

    public bool isPressed;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag.Equals("Player"))
        {
            sr.sprite = onSprite;
            isPressed = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            sr.sprite = offSprite;
            isPressed = false;
        }
    }
}
