using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618, 0649
public class EnemyBase : BattleActorBase
{
    public bool HasTargets { get { return targets != null && targets.Length > 0; } }
    public int RemainingBlock
    {
        get { return remainingBlock; }
        private set
        {
            if (remainingBlock != value)
            {
                remainingBlock = value;
                HealthBar.UpdateBlock();

                if (isServer)
                    RpcUpdateBlock(value);
            }
        }
    }

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

    private void OnMouseDown()
    {
        if (BattlePlayerBase.LocalAuthority.IsAlive && IsAlive)
        {
            BattlePlayerBase localPlayer = BattlePlayerBase.LocalAuthority;
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

    [ClientRpc]
    private void RpcUpdateBlock(int value)
    {
        RemainingBlock = value;
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
            yield return new WaitForSeconds(0.5f);
            NetworkServer.Destroy(gameObject);
        }
    }

    #endregion

    #region Attack

    [Server]
    public override void OnPlayerPhaseStart()
    {
        base.OnPlayerPhaseStart();
        RemainingBlock = (HasStatusEffect(StatusEffect.Weak) ? 0 : blockAmount);
        
        if (HasStatusEffect(StatusEffect.Stun) || HasStatusEffect(StatusEffect.Freeze))
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
                if (p.HasStatusEffect(StatusEffect.Invisible))
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
        int n = Mathf.Clamp(attacksPerTurn, 0, validTargets.Count);
        List<int> choices = new List<int>();

        for (int i = 0; i < validTargets.Count; i++)
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
    public void AttackPlayer(List<BattleController.EnemyAttack> groupAttack, int playerIndex)
    {
        if (targets.Contains(playerIndex))
        {
            BattleController.EnemyAttack attack = new BattleController.EnemyAttack();
            attack.hit = TryAttack();
            attack.apply = StatusEffect.None;

            if (attack.hit)
            {
                if (chanceToApply > Random.Range(0, 100))
                {
                    attack.apply = applies;
                    attack.duration = statDuration;
                }
                attack.attacker = this;
            }
            groupAttack.Add(attack);
        }
    }

    [SerializeField] private StatusEffect applies = StatusEffect.None;
    [SerializeField] private int chanceToApply;
    [SerializeField] private int statDuration;
    
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

    public virtual void OnMinigameSuccess()
    {

    }

    public virtual void OnMinigameFailed()
    {

    }
}
#pragma warning restore CS0618, 0649