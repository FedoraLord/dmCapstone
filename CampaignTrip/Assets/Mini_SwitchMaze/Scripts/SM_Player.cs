using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SM_Player : MonoBehaviour
{
    public float speed = 3;
    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Vector3 velocity = new Vector3();

        if (Input.GetKey(KeyCode.W))
        {
            velocity += transform.up;
        }
        if (Input.GetKey(KeyCode.D))
        {
            velocity += transform.right;
        }
        if (Input.GetKey(KeyCode.A))
        {
            velocity += -transform.right;
        }
        if (Input.GetKey(KeyCode.S))
        {
            velocity += -transform.up;
        }

        velocity = velocity.normalized * speed;
        rb.velocity = velocity;

    }
}
