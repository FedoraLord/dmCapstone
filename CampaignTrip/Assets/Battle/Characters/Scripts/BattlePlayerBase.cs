using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618
public class BattlePlayerBase : NetworkBehaviour
{
    public static BattlePlayerBase LocalAuthority { get { return PersistentPlayer.localAuthority.battlePlayer; } }

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
    private int attacksRemaining;
    private int blockAmount;
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
        int blocked = e.basicDamage * blockAmount / 100;
        int damageTaken = e.basicDamage - blocked;
        RpcTakeDamage(damageTaken, blocked);
    }

    [ClientRpc]
    private void RpcTakeDamage(int damageTaken, int blocked)
    {
        damagePopup.Display(damageTaken, blocked, uiTransform.position);

        health = Math.Max(health - damageTaken, 0);
        healthBar.SetHealth(health);
    }

    #endregion
}
#pragma warning restore CS0618