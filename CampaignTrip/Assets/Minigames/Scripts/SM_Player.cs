using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618
public class SM_Player : NetworkBehaviour
{
    public float speed = 3;
    public bool localAuthority;
    public Vector3 velocity;

    [SyncVar]
    public int playernum;

    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        velocity = new Vector3(0, 0, 0);
        rb = GetComponent<Rigidbody2D>();
        if (playernum == PersistentPlayer.localAuthority.playerNum)
        {
            localAuthority = true;
            Camera.main.transform.parent = gameObject.transform;
			Camera.main.transform.localPosition = new Vector3(0, 0, -10f);
            DPad.Instance.Setup(this);
        }
    }

    void FixedUpdate()
    {
        if (localAuthority)
        {
            if (rb.velocity != Vector2.zero || velocity != Vector3.zero)
            {
                rb.velocity = velocity.normalized * speed;
                
                CmdUpdatePosition(rb.velocity, this.transform.position);
            }

        }

    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("WinArea"))
        {
            MinigameManager.Instance.numPlayersWon++;
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("WinArea"))
        {
            MinigameManager.Instance.numPlayersWon--;
        }
    }

    [Command]
    private void CmdUpdatePosition(Vector2 velocity, Vector3 playerPosition)
    {
        RpcUpdatePosition(velocity, playerPosition);
    }

    [ClientRpc]
    private void RpcUpdatePosition(Vector2 velocity, Vector3 playerPosition)
    {
        if (!localAuthority)
        {
            transform.position = playerPosition;
            rb.velocity = velocity;
        }
    }
}
#pragma warning restore CS0618