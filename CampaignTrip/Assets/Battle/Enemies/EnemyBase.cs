using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618, 0649
public class EnemyBase : BattleActorBase
{
    protected override HealthBarUI HealthBar { get { return enemyHealthBar; } set { enemyHealthBar = value as EnemyUI; } }

    private EnemyUI enemyHealthBar;
    
    private int[] targets;
    private int remainingBlock;

    #region Initialization

    private void Start()
    {
        if (isServer)
        {
            BattleController.Instance.OnEnemyReady();
        }

        Health = maxHealth;
        HealthBar = BattleController.Instance.ClaimEnemyUI(this);
        damagePopup = BattleController.Instance.ClaimDamagePopup();
    }
    
    #endregion

    #region Damage

    private void OnMouseUpAsButton()
	{
        if (IsAlive)
        {
            if (BattlePlayerBase.LocalAuthority.IsUsingAbility && BattlePlayerBase.SelectedAbility.Targets == BattlePlayerBase.TargetMode.Foe)
            {
                BattlePlayerBase.LocalAuthority.OnAbilityTargetChosen(this);
            }
            else
            {
                BattlePlayerBase.LocalAuthority.CmdAttack(gameObject);
            }
        }
	}

    [Server]
    public void ApplyStatusEffect()
    {

    }

    [Server]
    public void TakeDamage(int damage)
	{
        RpcTakeDamage(damage);
	}
    
    [ClientRpc]
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
            Health = Mathf.Max(Health - damageTaken, 0);
            HealthBar.SetHealth(Health, OnHealthBarAnimComplete);
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

    private void Die()
    {
        //TODO play death animation

        HealthBar.Unclaim();
        
        if (isServer)
        {
            BattleController.Instance.OnEnemyDeath(this);
            NetworkServer.Destroy(gameObject);
        }
    }

    #endregion

    #region Attack

    public void OnPlayerPhaseStart()
    {
        remainingBlock = blockAmount;
        ChooseTargets();
        RpcUpdateTargets(targets);
    }

    protected virtual void ChooseTargets()
    {
        //picks targets randomly unless overridden
        targets = GetRandomNPlayers(attacksPerTurn);
    }

    [ClientRpc]
    private void RpcUpdateTargets(int[] newTargets)
    {
        targets = newTargets;
        enemyHealthBar.SetTargets(targets);
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
            PersistentPlayer.players[t].battlePlayer.TakeDamage(this);
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