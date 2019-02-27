using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SM_Player : NetworkBehaviour
{
    public float speed = 3;
    public bool localAuthority;

    [SyncVar]
    public int playernum;

    Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (playernum == PersistentPlayer.localAuthority.playerNum)
        {
            localAuthority = true;
            MinigameManager.Instance.mainCamera.transform.parent = gameObject.transform;
            MinigameManager.Instance.mainCamera.transform.localPosition = new Vector3(0, 0, -10f);
        }
    }

    void FixedUpdate()
    {
        if (localAuthority)
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

            CmdUpdatePosition(this.gameObject);
        }

    }

    [Command]
    private void CmdUpdatePosition(GameObject player)
    {
        RpcUpdatePosition(player);
    }

    [ClientRpc]
    private void RpcUpdatePosition(GameObject player)
    {
        foreach(SM_Player playerObject in FindObjectsOfType<SM_Player>())
        {
            if (player.GetComponent<SM_Player>().playernum == playerObject.playernum)
            {
                playerObject.transform.position = player.transform.position;
            }
        }
    }
}
