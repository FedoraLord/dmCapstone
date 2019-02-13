using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SM_Player : MonoBehaviour
{
    public float speed = 3;
    private Rigidbody2D rb;
    private Vector3 movementTarget;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        movementTarget = rb.transform.position;
    }

    void FixedUpdate()
    {
        if (rb.transform.position == movementTarget)
        {
            if (Input.GetKey(KeyCode.D))
            {
                movementTarget += Vector3.right;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                movementTarget += Vector3.down;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                movementTarget += Vector3.left;
            }
            else if (Input.GetKey(KeyCode.W))
            {
                movementTarget += Vector3.up;
            }
        }
        rb.transform.position = Vector3.MoveTowards(rb.transform.position, movementTarget, Time.deltaTime * speed);
    }
}
