using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618
public abstract class BattlePlayerBase : BattleActorBase
{
    public static BattlePlayerBase LocalAuthority { get { return PersistentPlayer.localAuthority.battlePlayer; } }
    
    [SyncVar] public int playerNum;

    [HideInInspector] public PersistentPlayer persistentPlayer;

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
        Health = maxHealth;
        HealthBar = BattleController.Instance.ClaimPlayerUI(this);
        damagePopup = BattleController.Instance.ClaimDamagePopup();
    }

    #endregion

    #region Attack

    //public abstract void Ability1();
    //public abstract void Ability2();
    //public abstract void Ability3();

    [Server]
    public void OnPlayerPhaseStart()
    {
        RpcUpdateAttackBlock(attacksPerTurn);
    }

    [Command]
    public void CmdAttack(GameObject target)
    {
        if (attacksRemaining > 0 && BattleController.Instance.IsPlayerPhase)
        {
            RpcAttack();
            EnemyBase enemy = target.GetComponent<EnemyBase>();
            enemy.TakeDamage(basicDamage);
        }
    }

    [ClientRpc]
    private void RpcAttack()
    {
        UpdateAttackBlock(attacksRemaining - 1);
        if (localPlayerAuthority)
        {
            animator.SetTrigger("Attack");
        }
    }

    [ClientRpc]
    private void RpcUpdateAttackBlock(int newAttacksRemaining)
    {
        UpdateAttackBlock(newAttacksRemaining);
    }

    private void UpdateAttackBlock(int newAttacksRemaining)
    {
        attacksRemaining = newAttacksRemaining;
        blockAmount = (int)Mathf.Min((float)attacksRemaining / attacksPerTurn * 100f, 90);

        if (this == LocalAuthority)
        {
            BattleController.Instance.UpdateAttackBlockUI(attacksRemaining, blockAmount);
        }
    }

    #endregion

    #region Damage

    [Server]
    public void TakeDamage(EnemyBase e)
    {
        int blocked = e.BasicDamage * blockAmount / 100;
        int damageTaken = e.BasicDamage - blocked;
        RpcTakeDamage(damageTaken, blocked);
    }

    [ClientRpc]
    private void RpcTakeDamage(int damageTaken, int blocked)
    {
        damagePopup.Display(damageTaken, blocked, uiTransform.position);

        Health = Math.Max(Health - damageTaken, 0);
        HealthBar.SetHealth(Health);
    }

    #endregion
}
#pragma warning restore CS0618