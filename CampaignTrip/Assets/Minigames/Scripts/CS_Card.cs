using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CS_Card : NetworkBehaviour
{
    public Sprite sprite;

    // Start is called before the first frame update
    void Start()
    {
        if (!sprite)
        {
            GetComponent<SpriteRenderer>().sprite = sprite;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        Console.WriteLine("Clicked on Card");
    }
}
