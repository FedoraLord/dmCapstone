using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static BattlePlayerBase;

#pragma warning disable 0618
public class SM_Player : NetworkBehaviour
{
    public RuntimeAnimatorController warriorController;
    public RuntimeAnimatorController rogueController;
    public RuntimeAnimatorController alchemistController;
    public RuntimeAnimatorController mageController;

    public Animator animator;
    public float speed = 3;
    public bool localAuthority;
    public Vector3 velocity;

    [SyncVar]
    public int playernum;

    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();

        velocity = new Vector3(0, 0, 0);
        rb = GetComponent<Rigidbody2D>();
        if (playernum == PersistentPlayer.localAuthority.playerNum)
        {
            localAuthority = true;

            Camera cam = SwitchMazeManager.GetInstance().cam;
            cam.transform.parent = gameObject.transform;
			cam.transform.localPosition = new Vector3(0, 0, -10f);

            DPad.Instance.Setup(this);
        }

        BattlePlayerBase player = players.Where(x => x.playerNum == playernum).First();
        SetAnimator(player.characterType);
    }

    public void SetAnimator(CharacterType type)
    {

        switch (type)
        {
            case CharacterType.Warrior:
                animator.runtimeAnimatorController = warriorController;
                break;

            case CharacterType.Rogue:
                animator.runtimeAnimatorController = rogueController;
                break;

            case CharacterType.Alchemist:
                animator.runtimeAnimatorController = alchemistController;
                break;

            case CharacterType.Mage:
                animator.runtimeAnimatorController = mageController;
                break;
        }
        animator.SetBool("Moving", false);
        animator.SetInteger("Direction", 2);
    }

    void FixedUpdate()
    {
        if (localAuthority)
        {
            AnimatePlayer();
            if (rb.velocity != Vector2.zero || velocity != Vector3.zero)
            {
                rb.velocity = velocity.normalized * speed;
                
                CmdUpdatePosition(rb.velocity, transform.position);
            }
        }

    }

    void AnimatePlayer()
    {
        if (velocity.magnitude > 0)
        {
            animator.SetBool("Moving", true);
            if (velocity.x > 0)
            {
                animator.SetInteger("Direction", 1);
            }
            else if (velocity.x < 0)
            {
                animator.SetInteger("Direction", 3);
            }
            else if (velocity.y < 0)
            {
                animator.SetInteger("Direction", 2);
            }
            else
            {
                animator.SetInteger("Direction", 0);
            }
        }
        else
        {
            animator.SetBool("Moving", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("WinArea"))
        {
            SwitchMazeManager.GetInstance().PlayerEnteredWinArea();
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("WinArea"))
        {
            SwitchMazeManager.GetInstance().PlayerLeftWinArea();
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