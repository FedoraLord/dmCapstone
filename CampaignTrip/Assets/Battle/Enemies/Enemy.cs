using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Enemy : NetworkBehaviour
{
    public int maxHealth = 100;
    public Transform uiTransform;

    [SyncVar]
	public int health;

	public bool isAlive { get { return health > 0; } }

    [SerializeField] private SpriteRenderer tmpDamageIndicator;

    private EnemyUI UI;

    private void Start()
    {
        if (NetworkWrapper.IsHost)
        {
            health = maxHealth;
        }

        UI = BattleController.Instance.ClaimEnemyUI(this);
    }

    private void OnMouseUpAsButton()
	{
        BattlePlayer.LocalAuthority.CmdAttack(gameObject);
	}
    
	public void TakeDamage(int damage)
	{
        if (!isServer)
            return;

		health -= damage;
        RpcTakeDamage();
	}

	[ClientRpc]
	protected void RpcTakeDamage()
	{
        //TODO: play damage animation

        UI.SetHealth(health, true, testcallback);
        StartCoroutine(ShowDamageIndicator());

        if (!isAlive)
        {
            Destroy(gameObject);
        }

  //      BattleController.Instance.CmdTryEndWave();
		//gameObject.SetActive(false);
	}

    private int asdf;
    private void testcallback()
    {
        asdf++;
        Debug.Log("Animation finished! : " + asdf);
    }

    private IEnumerator ShowDamageIndicator()
    {
        tmpDamageIndicator.enabled = true;
        yield return new WaitForSeconds(.3f);
        tmpDamageIndicator.enabled = false;
    }
}
