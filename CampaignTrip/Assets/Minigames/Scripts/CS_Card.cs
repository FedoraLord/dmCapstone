using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CS_Card : MonoBehaviour
{
    private Sprite sprite;
    public Sprite Sprite
    {
        get { return sprite; }
        set
        {
            sprite = value;
            dataChanged = true;
        }
    }
    private bool dataChanged;
    private SpriteRenderer spriteRenderer;

    public int index;


    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        Sprite = spriteRenderer.sprite;
    }

    // Update is called once per frame
    void Update()
    {
        if (dataChanged)
        {
            spriteRenderer.sprite = sprite;
            dataChanged = false;
        }
    }

    private void OnMouseDown()
    {
        CS_Manager.Instance.CardSelected(this);
    }
}
