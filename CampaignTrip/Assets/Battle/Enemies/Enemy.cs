﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618, 0649
public class Enemy : NetworkBehaviour
{
    public int health;

    public int numTargets = 1;
    public int basicDamage = 5;
    public int maxBlock;
    public int maxHealth = 100;
    public Transform uiTransform;

	public bool isAlive { get { return health > 0; } }

    [SerializeField] private SpriteRenderer tmpDamageIndicator;

    private DamagePopup damagePopup;
    private EnemyUI healthBar;
    private int[] targets;
    private int remainingBlock;

    #region Initialization

    private void Start()
    {
        if (isServer)
        {
            BattleController.Instance.OnEnemyReady();
        }

        health = maxHealth;
        healthBar = BattleController.Instance.ClaimEnemyUI(this);
        damagePopup = BattleController.Instance.ClaimDamagePopup();
    }

    public void OnStartPlayerPhase()
    {
        remainingBlock = maxBlock;
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

    [Server]
    public void TakeDamage(int damage)
	{
        RpcTakeDamage(damage);
	}
    
	private void RpcTakeDamage(int damage)
	{
        //TODO: play damage animation

        int damageTaken = damage;
        int initialBlock = remainingBlock;

        if (remainingBlock > 0)
        {
            remainingBlock = Mathf.Max(remainingBlock - damage, 0);
            damageTaken = damage - remainingBlock;
        }

        if (damageTaken > 0)
        {
            health = Mathf.Max(health - damageTaken, 0);
            healthBar.SetHealth(health, OnHealthBarAnimComplete);
        }

        damagePopup.Display(damageTaken, initialBlock - remainingBlock, uiTransform.position);
	}
    
    private void OnHealthBarAnimComplete(bool isDead)
    {
        if (isDead)
        {
            Die();
        }
    }

    //private IEnumerator ShowDamageIndicator()
    //{
    //    tmpDamageIndicator.enabled = true;
    //    yield return new WaitForSeconds(.3f);
    //    tmpDamageIndicator.enabled = false;
    //}

    private void Die()
    {
        //TODO play death animation

        healthBar.Unclaim();
        
        if (isServer)
        {
            BattleController.Instance.OnEnemyDeath(this);
            NetworkServer.Destroy(gameObject);
        }
    }

    #endregion

    #region Attack

    public void OnAttackTimerBegin()
    {
        ChooseTargets();
        RpcUpdateTargets(targets);
    }

    protected virtual void ChooseTargets()
    {
        //picks targets randomly unless overridden
        targets = GetRandomNPlayers(numTargets);
    }

    [ClientRpc]
    private void RpcUpdateTargets(int[] newTargets)
    {
        targets = newTargets;
        healthBar.SetTargets(targets);
    }

    protected int[] GetRandomNPlayers(int n)
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

        return result.ToArray();
    }
    
    [Server]
    public void Attack()
    {
        foreach (int t in targets)
        {
            PersistentPlayer.players[t].battlePlayer.AccumulateDamage(this);
        }
    }

    #endregion

    public virtual void OnMinigameSuccess()
    {

    }

    public virtual void OnMinigameFailed()
    {

    }
}
#pragma warning restore CS0618, 0649