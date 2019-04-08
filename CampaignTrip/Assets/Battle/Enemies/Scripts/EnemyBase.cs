using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static StatusEffect;

#pragma warning disable 0618, 0649
public class EnemyBase : BattleActorBase
{
    public bool HasTargets { get { return targets != null && targets.Length > 0; } }

    [SyncVar]
    public int spawnPosition;
    public bool useBossSpawn;

    public EnemyType enemyType;
    public enum EnemyType
    {
        None, GreenSlime, RedSlime, BlueSlime, KingSlime,
        AcornBoi, Bat, CaveBug, Lich
    }

    private int[] targets;

    #region Initialization

    protected override void Initialize()
    {
        if (!(this is Boss))
        {
            battleStats = BuffStatTracker.Instance.GetEnemyStats(enemyType);
        }
        BattleController.Instance.OnEnemySpawned(this);
        base.Initialize();
    }

    #endregion

    #region Damage

    private void OnMouseDown()
    {
        BattlePlayerBase localPlayer = BattlePlayerBase.LocalAuthority;
        if (localPlayer.IsAlive && IsAlive)
        {
            if (localPlayer.IsValidTarget(this))
            {
                if (localPlayer.SelectedAbility != null)
                {
                    localPlayer.OnAbilityTargetChosen(this);
                }
                else
                {
                    localPlayer.CmdAttack(gameObject);
                }
            }
        }
    }

    [Server]
    public override int TakeBlockedDamage(int damage)
    {
        int initialBlock = RemainingBlock;

        if (RemainingBlock > 0)
        {
            RemainingBlock = Mathf.Max(RemainingBlock - damage, 0);
            damage -= initialBlock - RemainingBlock;
        }

        TakeDamage(damage, initialBlock - RemainingBlock);
        return damage;
    }
    
    protected override void Die()
    {
        base.Die();
        BattleController.Instance.OnEnemyDeath(this);
        StartCoroutine(DelayDeath());
    }

    private IEnumerator DelayDeath()
    {
        if (isServer)
        {
            yield return new WaitForSeconds(1f);
            BattleController.Instance.OnEnemyDestroy(this);
            NetworkServer.Destroy(gameObject);
        }
    }

    #endregion

    #region Attack

    [Server]
    public override void OnPlayerPhaseStart()
    {
        base.OnPlayerPhaseStart();
        RemainingBlock = (HasStatusEffect(Stat.Weak) ? 0 : battleStats.BlockAmount);
        
        if (HasStatusEffect(Stat.Stun) || HasStatusEffect(Stat.Freeze))
        {
            RpcUpdateTargets(new int[0]);
        }
        else
        {
            List<BattlePlayerBase> validTargets = new List<BattlePlayerBase>();
            foreach (BattlePlayerBase p in BattlePlayerBase.players)
            {
                if (!p.IsAlive)
                    continue;
                if (p.HasStatusEffect(Stat.Invisible))
                    continue;
                validTargets.Add(p);
            }

            targets = ChooseTargets(validTargets);
            RpcUpdateTargets(targets);
        }
    }

    //picks targets randomly unless overridden
    protected virtual int[] ChooseTargets(List<BattlePlayerBase> validTargets)
    {
        int n = Mathf.Clamp(battleStats.AttacksPerTurn, 0, validTargets.Count);
        List<BattlePlayerBase> choices = new List<BattlePlayerBase>(validTargets);

        List<int> result = new List<int>();
        while (n > 0)
        {
            int i = Random.Range(0, choices.Count);
            result.Add(choices[i].playerNum - 1);
            choices.RemoveAt(i);
            n--;
        }

        return result.ToArray();
    }

    [ClientRpc]
    private void RpcUpdateTargets(int[] newTargets)
    {
        targets = newTargets;
        HealthBar.SetTargets(targets);
		bool doWeDisplayAggro = false;

        foreach (int i in newTargets)
        {
            if (i == PersistentPlayer.localAuthority.playerNum - 1)
            {
                doWeDisplayAggro = true;
                break;
            }
        }

		overlays.ToggleAggro(doWeDisplayAggro);
	}
    
    [Server]
    public void AttackPlayer(List<EnemyAttack> groupAttack, int playerIndex)
    {
        if (targets.Contains(playerIndex))
        {
            EnemyAttack attack = new EnemyAttack();
            attack.hit = TryAttack();
            attack.apply = Stat.None;
            attack.attacker = this;

            if (attack.hit)
            {
                if (battleStats.AppliedEffects.Stats.Length > 0 && battleStats.ChanceToApply > Random.Range(0, 100))
                {
                    attack.apply = battleStats.AppliedEffects.Stats.Random();
                    attack.duration = battleStats.ApplyDuration;
                }
            }
            groupAttack.Add(attack);
        }
    }
    
    public void RemoveTarget(int playerNum)
    {
        List<int> targetsCopy = new List<int>(targets);
        if (targetsCopy.Remove(playerNum - 1))
        {
            targets = targetsCopy.ToArray();
            healthBarUI.SetTargets(targets);
        }
    }

    #endregion

    #region StatusEffects

    protected override void OnAddStun()
    {
        base.OnAddStun();
        LoseTargets();
    }

    protected override void OnAddFreeze()
    {
        base.OnAddFreeze();
        LoseTargets();
    }

    protected override void OnAddWeak()
    {
        base.OnAddWeak();
        RemainingBlock = 0;
    }

    private void LoseTargets()
    {
        HealthBar.SetTargets();

        if (isServer)
        {
            RpcUpdateTargets(new int[0]);
        }
    }

    #endregion
    
}