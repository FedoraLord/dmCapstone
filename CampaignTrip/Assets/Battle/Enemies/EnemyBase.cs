using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static StatusEffect;

#pragma warning disable CS0618, 0649
public class EnemyBase : BattleActorBase
{
    public bool HasTargets { get { return targets != null && targets.Length > 0; } }

    private int[] targets;
    private int remainingBlock;

    #region Initialization

    private void Start()
    {
        BattleController.Instance.OnEnemySpawned(this);
        Initialize();
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
    public void DispatchDamage(int damage)
	{
        RpcTakeDamage(damage);
	}
    
    [ClientRpc]
	private void RpcTakeDamage(int damage)
	{
        TakeDamage(damage);
	}

    public void TakeDamage(int damage)
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
            Health -= damageTaken;
            HealthBar.SetHealth(Health);
        }

        damagePopup.Display(damageTaken, initialBlock - remainingBlock);
    }

    protected override void Die()
    {
        BattleController.Instance.OnEnemyDeath(this);

        if (isServer)
        {
            NetworkServer.Destroy(gameObject);
        }
    }

    #endregion

    #region Attack

    public void OnPlayerPhaseStart()
    {
        remainingBlock = blockAmount;
        ChooseTargets();

        if (HasStatusEffect(StatusEffectType.Stun))
            RpcUpdateTargets(new int[0]);
        else
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
        HealthBar.SetTargets(targets);
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

    #region StatusEffects

    protected override void OnAddStun()
    {
        base.OnAddStun();
        HealthBar.SetTargets();

        if (isServer)
        {
            RpcUpdateTargets(new int[0]);
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