using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618, 0649
public class Enemy : NetworkBehaviour
{
    public int maxHealth = 100;
    public Transform uiTransform;

    [SyncVar(hook = "OnHealthChanged")]
    public int health;

	public bool isAlive { get { return health > 0; } }

    [SerializeField] private SpriteRenderer tmpDamageIndicator;

    private bool initialized;
    private EnemyUI UI;
    private int[] targets;

    #region Initialization

    private void Start()
    {
        if (NetworkWrapper.IsHost)
        {
            health = maxHealth;
        }

        UI = BattleController.Instance.ClaimEnemyUI(this);
        BattleController.Instance.OnEnemyReady();
        initialized = true;
    }

    #endregion

    #region Damage

    private void OnMouseUpAsButton()
	{
        if (isAlive)
        {
            BattlePlayer.LocalAuthority.CmdAttack(gameObject);
        }
	}
    
	public void TakeDamage(int damage)
	{
        if (!isServer)
            return;

		health -= damage;
	}
    
	private void OnHealthChanged(int hp)
	{
        if (!initialized)
            return;
        
        //TODO: play damage animation

        UI.SetHealth(hp, OnHealthBarAnimComplete);
        StartCoroutine(ShowDamageIndicator());
	}
    
    private void OnHealthBarAnimComplete(bool isDead)
    {
        if (isDead)
        {
            Die();
        }
    }

    private IEnumerator ShowDamageIndicator()
    {
        tmpDamageIndicator.enabled = true;
        yield return new WaitForSeconds(.3f);
        tmpDamageIndicator.enabled = false;
    }

    private void Die()
    {
        //TODO play death animation

        UI.Unclaim();
        gameObject.SetActive(false);
        
        if (isServer)
        {
            BattleController.Instance.OnEnemyDeath(this);
            NetworkServer.Destroy(gameObject);
        }
    }

    #endregion

    #region Attacking

    public void OnAttackTimerBegin()
    {
        ChooseTargets();
        RpcUpdateTargets(targets);
    }

    protected virtual void ChooseTargets()
    {
        //by default, picks a random player 0-3
        int choice = Random.Range(0, PersistentPlayer.players.Count);
        targets = new int[] { choice };
    }

    [ClientRpc]
    private void RpcUpdateTargets(int[] newTargets)
    {
        targets = newTargets;
        UI.SetTargets(targets);
    }

    protected void GetRandomNPlayers(int n)
    {
        Mathf.Clamp(n, 0, PersistentPlayer.players.Count);
        List<int> choices = new List<int>();
        for (int i = 0; i < PersistentPlayer.players.Count; i++)
        {
            choices.Add(i);
        }

        List<int> result = new List<int>();
        while (n > 0)
        {
            int i = Random.Range(0, choices.Count);
            result.Add(choices[i]);
            choices.RemoveAt(i);
            n--;
        }
    }

    #endregion
}
#pragma warning restore CS0618, 0649