using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static Ability;
using static StatusEffect;

#pragma warning disable CS0618, 0649
public abstract class BattlePlayerBase : BattleActorBase
{
    public static BattlePlayerBase LocalAuthority { get { return PersistentPlayer.localAuthority.battlePlayer; } }
    public static List<BattlePlayerBase> players { get { return PersistentPlayer.players.Select(x => x.battlePlayer).ToList(); } }
    public static int PlayersUsingAbility;
    
    public bool CanPlayAbility
    {
        get { return canPlayAbility; }
        set
        {
            canPlayAbility = value;
            foreach (Ability a in Abilities)
            {
                a.UpdateButtonUI();
            }
        }
    }
    public List<Ability> Abilities { get; private set; }
    public Ability SelectedAbility
    {
        get
        {
            if (selectedAbilityIndex < 0)
                return null;
            return Abilities[selectedAbilityIndex];
        }
    }
    
    [SyncVar] public int playerNum;
    public CharacterType characterType;

    [HideInInspector] public PersistentPlayer persistentPlayer;

    [SerializeField] protected Ability ability1;
    [SerializeField] protected Ability ability2;
    [SerializeField] protected Ability ability3;

    protected List<BattleActorBase> ValidTargets
    {
        get
        {
            TargetGroup group = (SelectedAbility == null) ? TargetGroup.Enemy : SelectedAbility.Targets;
            
            switch (group)
            {
                case TargetGroup.Override:
                    return customTargets;
                case TargetGroup.Self:
                    return new List<BattleActorBase>() { this };
                case TargetGroup.Ally:
                    List<BattleActorBase> targets = new List<BattleActorBase>(players);
                    targets.Remove(this);
                    return targets;
                case TargetGroup.AllyAndSelf:
                    return new List<BattleActorBase>(players);
                case TargetGroup.Enemy:
                    return BattleController.Instance.aliveEnemies.Where(x => !x.HasStatusEffect(Stat.Invisible)).ToList<BattleActorBase>();
                default:
                    Debug.LogErrorFormat("Valid targets not implemented for {0}", SelectedAbility.Targets);
                    return new List<BattleActorBase>();
            }
        }
    }

    protected List<BattleActorBase> customTargets;
    protected int selectedAbilityIndex = -1;
    protected int attacksRemaining;

    private bool canPlayAbility;

    public enum CharacterType { Warrior, Rogue, Alchemist, Mage };

    #region Initialization

    protected override void Initialize()
    {
        int i = playerNum - 1;
        persistentPlayer = PersistentPlayer.players[i];
        persistentPlayer.battlePlayer = this;

        transform.parent = BattleController.Instance.battleCam.PlayerSpawnPoints[i];
        transform.localPosition = Vector3.zero;

        Abilities = new List<Ability>() { ability1, ability2, ability3 };
        BattleController.Instance.OnBattlePlayerSpawned(this);

        battleStats = BuffStatTracker.Instance.GetPlayerStats(CharacterType.Alchemist, battleStats);
        base.Initialize();
    }

    [Server]
    public override void OnPlayerPhaseStart()
    {
        base.OnPlayerPhaseStart();
        RpcOnPlayerPhaseStart();
    }

    [ClientRpc]
    private void RpcOnPlayerPhaseStart()
    {
        if (HasStatusEffect(Stat.Stun) || HasStatusEffect(Stat.Freeze))
        {
            attacksRemaining = 0;
            CanPlayAbility = false;
        }
        else
        {
            attacksRemaining = battleStats.AttacksPerTurn;
            CanPlayAbility = true;
        }
        RemainingBlock = (HasStatusEffect(Stat.Weak) ? 0 : battleStats.BlockAmount);

        if (this == LocalAuthority)
            BattleController.Instance.UpdateAttackBlockUI(attacksRemaining, RemainingBlock);

        foreach (Ability a in Abilities)
            a.DecrementCooldown();
    }

    #endregion

    #region Attack

    private void OnMouseDown()
    {
        if (LocalAuthority.IsAlive && IsAlive)
        {
            if (LocalAuthority.SelectedAbility != null && LocalAuthority.IsValidTarget(this))
            {
                LocalAuthority.OnAbilityTargetChosen(this);
            }
            else if (LocalAuthority.attacksRemaining > 0)
            {
                Stat[] stats = { Stat.Burn, Stat.Freeze };
                foreach (Stat stat in stats)
                {
                    if (HasStatusEffect(stat))
                    {
                        LocalAuthority.CmdHelpWithStat(stat, gameObject);
                        break;
                    }
                }
            }
        }
    }

    [Command]
    public void CmdHelpWithStat(Stat type, GameObject target)
    {
        BattleActorBase t = target.GetComponent<BattleActorBase>();
        t.RpcDisplayStat(type, -1);
        t.RpcDecrementDuration(type, 1);

        RpcAttack();
    }
    
    [Command]
    public void CmdAttack(GameObject target)
    {
        if (attacksRemaining > 0 && BattleController.Instance.IsPlayerPhase)
        {
            RpcAttack();
            EnemyBase enemy = target.GetComponent<EnemyBase>();

            if (TryAttack())
            {
                enemy.DispatchBlockableDamage(this);
            }
            else
            {
                enemy.RpcMiss();
            }
        }
    }

    [ClientRpc]
    private void RpcAttack()
    {
        attacksRemaining--;

        if (!HasStatusEffect(Stat.Weak))
            RemainingBlock = (int)((float)attacksRemaining / battleStats.AttacksPerTurn * battleStats.BlockAmount / 100f);

        if (this == LocalAuthority)
        {
            PlayAnimation(BattleAnimation.Attack);
            BattleController.Instance.UpdateAttackBlockUI(attacksRemaining, RemainingBlock);
        }
    }

    #endregion

    #region Abilities

    protected abstract void OverrideTargeting();
    protected abstract void OnAbilityUsed();

    public bool IsValidTarget(BattleActorBase target)
    {
        return ValidTargets.Contains(target);
    }

    public void AbilitySelected(int i)
    {
        if (SelectedAbility != null)
        {
            EndAbility();
        }
        else if (CanPlayAbility && Abilities[i].RemainingCooldown <= 0)
        {
            CmdUseAbilityRequest(i);
        }
    }

    [Command]
    private void CmdUseAbilityRequest(int i)
    {
        if (BattleController.Instance.IsPlayerPhase)
        {
            PlayersUsingAbility++;
            Target_UseAbilityConfirm(persistentPlayer.connectionToClient, i);
        }
    }

    [TargetRpc]
    private void Target_UseAbilityConfirm(NetworkConnection conn, int i)
    {
        selectedAbilityIndex = i;
        if (SelectedAbility.Targets == TargetGroup.Self)
        {
            OnAbilityTargetChosen(this);
        }
        else if (SelectedAbility.TargetAll)
        {
            OnAbilityTargetChosen(null);
        }
        else
        {
            if (SelectedAbility.Targets == TargetGroup.Override)
            {
                OverrideTargeting();
            }
            ToggleTargetIndicators(true);
        }
    }
    
    protected void ToggleTargetIndicators(bool active)
    {
        if (active)
        {
            foreach (BattleActorBase target in ValidTargets)
            {
                target.tempAbilityTarget.SetActive(true);
            }
        }
        else
        {
            foreach (EnemyBase enemy in BattleController.Instance.aliveEnemies)
            {
                enemy.tempAbilityTarget.SetActive(false);
            }

            foreach (BattlePlayerBase p in players)
            {
                p.tempAbilityTarget.SetActive(false);
            }
        }
    }

    public void OnAbilityTargetChosen(BattleActorBase target)
    {
        CmdUseAbility(target?.gameObject, selectedAbilityIndex);
        SelectedAbility.Use();
        EndAbility();
    }
    
    [Command]
    protected void CmdUseAbility(GameObject target, int i)
    {
        selectedAbilityIndex = i;
        if (target == null)
        {
            if (!SelectedAbility.TargetAll)
            {
                OverrideTargeting();
            }

            foreach (BattleActorBase actor in ValidTargets)
            {
                UseAbility(actor);
            }
        }
        else
        {
            BattleActorBase actor = target.GetComponent<BattleActorBase>();
            UseAbility(actor);
        }
        OnAbilityUsed();
    }

    [Server]
    private void UseAbility(BattleActorBase target)
    {
        if (target is EnemyBase)
        {
            target.DispatchBlockableDamage(this, SelectedAbility.Damage);
        }

        Ability a = SelectedAbility;
        if (a.Applies != Stat.None)
        {
            if (a.Applies == Stat.Cure)
            {
                target.AddStatusEffect(a.Applies, this, a.Duration, a.Damage);
            }
            if (a.Applies == Stat.Focus)
            {
                AddStatusEffect(a.Applies, target, a.Duration, a.Damage);
            }
            else
            {
                target.AddStatusEffect(a.Applies, this, a.Duration, 0);
            }
        }
    }

    public void EndAbility()
    {
        ToggleTargetIndicators(false);
        selectedAbilityIndex = -1;
        customTargets = null;
        CmdEndAbility();
    }

    [Command]
    private void CmdEndAbility()
    {
        PlayersUsingAbility--;
    }

    #endregion

    #region Damage

    [Server]
    public override int TakeBlockedDamage(int damage)
    {
        int blocked = damage * RemainingBlock / 100;
        int damageTaken = damage - blocked;
        TakeDamage(damageTaken, blocked);
        return damageTaken;
    }

    protected override void Die()
    {
        base.Die();
    }

    #endregion

    #region StatusEffects

    protected override void OnAddInvisible()
    {
        base.OnAddInvisible();
        foreach (EnemyBase enemy in BattleController.Instance.aliveEnemies)
        {
            enemy.RemoveTarget(playerNum);
        }
    }

    protected override void OnAddStun()
    {
        base.OnAddStun();
        SetPrivelages(false, false);
    }

    protected override void OnAddFreeze()
    {
        base.OnAddFreeze();
        SetPrivelages(false, false);
    }

    private void SetPrivelages(bool ability, bool attack)
    {
        CanPlayAbility = ability;
        if (!attack)
        {
            if (this == LocalAuthority)
                BattleController.Instance.UpdateAttackBlockUI(0, RemainingBlock);
            attacksRemaining = 0;
        }
    }

    #endregion
}
