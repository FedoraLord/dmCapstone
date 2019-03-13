﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618
public class BattlePlayer : NetworkBehaviour
{
    public static BattlePlayer LocalAuthority { get { return PersistentPlayer.localAuthority.battlePlayer; } }

    public int health;

    [SyncVar]
    public int playerNum;

    public Transform uiTransform;
    public int attacksPerTurn;
    public int basicDamage;
    public int maxHealth;

    [HideInInspector]
    public PersistentPlayer persistentPlayer;

    [SerializeField] private Animator animator;

    private bool initialized;
    private DamagePopup damagePopup;
    private int damageToTake;
    private int attacksRemaining;
    private HealthBarUI healthBar;

    #region Initialization

    public override void OnStartClient()
    {
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        yield return new WaitUntil(() => BattleController.Instance != null);

        int i = playerNum - 1;
        persistentPlayer = PersistentPlayer.players[i];
        persistentPlayer.battlePlayer = this;
        transform.position = BattleController.Instance.playerSpawnPoints[i];
        healthBar = BattleController.Instance.ClaimPlayerUI(this);
        damagePopup = BattleController.Instance.ClaimDamagePopup();
        
        health = maxHealth;
        initialized = true;
    }

    #endregion

    #region Attack

    public void OnPlayerPhaseStart()
    {
        attacksRemaining = attacksPerTurn;
    }

    [Command]
    public void CmdAttack(GameObject target)
    {
        if (attacksRemaining > 0 && BattleController.Instance.IsPlayerPhase)
        {
            attacksRemaining--;
            RpcTriggerAttackAnimation();
            Enemy enemy = target.GetComponent<Enemy>();
            enemy.TakeDamage(basicDamage);
        }
    }

    [ClientRpc]
    private void RpcTriggerAttackAnimation()
    {
        animator.SetTrigger("Attack");
    }

    #endregion

    #region Damage

    [Server]
    public void AccumulateDamage(Enemy e)
    {
        damageToTake += e.basicDamage;
    }

    [Server]
    public void TakeAccumulatedDamage()
    {
        if (damageToTake > 0)
        {
            int newHealth = health - damageToTake;
            newHealth = Mathf.Clamp(newHealth, 0, maxHealth);
            damageToTake = 0;
            RpcTakeDamage(newHealth);
        }
    }

    [ClientRpc]
    private void RpcTakeDamage(int newHealth)
    {
        //test
        damagePopup.Display(health - newHealth, 0, uiTransform.position);

        health = newHealth;
        healthBar.SetHealth(newHealth);
    }

    #endregion
}
#pragma warning restore CS0618