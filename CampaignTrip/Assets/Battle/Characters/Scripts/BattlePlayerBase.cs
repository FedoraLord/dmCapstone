using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static StatusEffect;

#pragma warning disable CS0618, 0649
public abstract class BattlePlayerBase : BattleActorBase
{
    public static BattlePlayerBase LocalAuthority { get { return PersistentPlayer.localAuthority.battlePlayer; } }
    //public static BP_Mage Mage { get; protected set; }
    //public static BP_Rogue Rogue { get; protected set; }
    //public static BP_Warrior Warrior { get; protected set; }
    //public static BP_Alchemist Alchemist { get; protected set; }
    public static int PlayersUsingAbility;

    public Ability SelectedAbility
    {
        get
        {
            if (selectedAbilityIndex < 0)
                return null;
            return abilities[selectedAbilityIndex];
        }
    }

    public bool AbilityPlayedThisTurn { get; set; }
    
    [SyncVar] public int playerNum;

    [HideInInspector] public PersistentPlayer persistentPlayer;

    [SerializeField] protected Ability ability1;
    [SerializeField] protected Ability ability2;
    [SerializeField] protected Ability ability3;

    protected List<Ability> abilities;
    protected List<BattleActorBase> validTargets;
    protected int selectedAbilityIndex = -1;

    private int attacksRemaining;

    public enum TargetMode { Override, Ally, AllyAndSelf, Enemy }

    [Serializable]
    public class Ability
    {
        //public int AbilityIndex { get; set; }
        public int Damage { get { return damage; } }
        public int Duration { get { return duration; } }
        public TargetMode Targets { get { return targets; } }
        
        public StatusEffect StatusEffect
        {
            get
            {
                if (statusEffect == null)
                    statusEffect = BattleController.Instance.GetStatusEffect(applies);
                return statusEffect;
            }
        }

        [HideInInspector] public int RemainingCooldown;

        [SerializeField] private string abilityName;
        [SerializeField] private TargetMode targets;
        [SerializeField] private StatusEffectType applies;
        [Tooltip("Also used for heal amount")]
        [SerializeField] private int damage;
        [SerializeField] private int duration;
        [SerializeField] private int cooldown;

        private StatusEffect statusEffect;

        //TODO:
        [HideInInspector] public bool IsUpgraded;

        [SerializeField] private Sprite ButtonIcon;
        //END TODO

        public void Use()
        {
            LocalAuthority.AbilityPlayedThisTurn = true;
            RemainingCooldown = cooldown;
        }
    }

    #region Initialization

    public override void OnStartClient()
    {
        StartCoroutine(DelayInitialize());
    }

    private IEnumerator DelayInitialize()
    {
        yield return new WaitUntil(() => BattleController.Instance != null);

        int i = playerNum - 1;
        persistentPlayer = PersistentPlayer.players[i];
        persistentPlayer.battlePlayer = this;

        transform.position = BattleController.Instance.playerSpawnPoints[i].position;
        
        abilities = new List<Ability>() { ability1, ability2, ability3 };

        Initialize();
    }

    [Server]
    public void OnPlayerPhaseStart()
    {
        RpcUpdateAttackBlock(attacksPerTurn);
        RpcOnPlayerPhaseStart();
    }

    [ClientRpc]
    private void RpcOnPlayerPhaseStart()
    {
        LocalAuthority.AbilityPlayedThisTurn = false;
    }

    #endregion

    #region Attack

    private void OnMouseUpAsButton()
    {
        if (IsAlive && IsValidTarget(this))
        {
            if (LocalAuthority.SelectedAbility != null)
            {
                LocalAuthority.OnAbilityTargetChosen(this);
            }
            else
            {
                //TODO: help with fire and ice
            }
        }
    }
    
    [Command]
    public void CmdAttack(GameObject target)
    {
        if (attacksRemaining > 0 && BattleController.Instance.IsPlayerPhase)
        {
            RpcAttack();
            EnemyBase enemy = target.GetComponent<EnemyBase>();
            enemy.DispatchDamage(basicDamage, true);
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

    #region Abilities

    public bool IsValidTarget(BattleActorBase target)
    {
        return validTargets.Contains(target);
        //if (target is EnemyBase)
        //{
        //    if (SelectedAbility == null)
        //    {
        //        //regular attack
        //        return true;
        //    }
        //    else if (SelectedAbility.Targets == TargetMode.Foe)
        //    {
        //        //offensive ability target
        //        return true;
        //    }
        //}
        //else if (target is BattlePlayerBase)
        //{
        //    if (SelectedAbility == null)
        //    {
        //        if (target.HasStatusEffect(StatusEffectType.Burn) || target.HasStatusEffect(StatusEffectType.Freeze))
        //        {
        //            return true;
        //        }
        //    }
        //    else
        //    {
        //        if (SelectedAbility.Targets == TargetMode.Friend)
        //        {

        //        }
        //        else if (SelectedAbility.Targets == TargetMode.Team)
        //    }
        //}
    }

    public void AbilitySelected(int i)
    {
        if (SelectedAbility != null)
        {
            EndAbility();
        }
        else if (!AbilityPlayedThisTurn && abilities[i].RemainingCooldown <= 0)
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

        switch (SelectedAbility.Targets)
        {
            case TargetMode.Override:
                SpecialTargeting();
                break;
            case TargetMode.Ally:
                ShowTargetFriends(false);
                break;
            case TargetMode.AllyAndSelf:
                ShowTargetFriends(true);
                break;
            case TargetMode.Enemy:
                ShowTargetFoes();
                break;
        }
    }

    protected abstract void SpecialTargeting();

    private void ShowTargetFriends(bool canTargetSelf)
    {
        validTargets = new List<BattleActorBase>();
        foreach (PersistentPlayer p in PersistentPlayer.players)
        {
            if (canTargetSelf || p.battlePlayer != this)
            {
                validTargets.Add(p.battlePlayer);
            }
        }
        ToggleTargetIndicators(true);
    }

    private void ShowTargetFoes()
    {
        validTargets = new List<BattleActorBase>();
        foreach (EnemyBase e in BattleController.Instance.aliveEnemies)
        {
            validTargets.Add(e);
        }
        ToggleTargetIndicators(true);
    }

    protected void ToggleTargetIndicators(bool active)
    {
        foreach (BattleActorBase target in validTargets)
        {
            target.tempAbilityTarget.SetActive(active);
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
        if (target == null)
        {
            foreach (EnemyBase e in BattleController.Instance.aliveEnemies)
            {
                UseAbility(e, abilities[i]);
            }
        }
        else
        {
            BattleActorBase actor = target.GetComponent<BattleActorBase>();
            UseAbility(actor, abilities[i]);
        }
    }

    [Server]
    private void UseAbility(BattleActorBase target, Ability ability)
    {
        target.DispatchDamage(ability.Damage, true);
        target.AddStatusEffect(ability.StatusEffect, this, ability.Duration);
    }

    public void EndAbility()
    {
        ToggleTargetIndicators(false);
        validTargets = new List<BattleActorBase>(BattleController.Instance.aliveEnemies);
        selectedAbilityIndex = -1;
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
    public override void DispatchDamage(int damage, bool canBlock)
    {
        if (canBlock)
        {
            if (HasStatusEffect(StatusEffectType.Protected))
            {
                BattleActorBase protector = GetGivenBy(StatusEffectType.Protected);
                protector.DispatchDamage(damage, canBlock);
            }
            else
            {
                int blocked = damage * blockAmount / 100;
                int damageTaken = damage - blocked;
                RpcTakeDamage(damageTaken, blocked);
            }
        }
        else
        {
            RpcTakeDamage(damage, 0);
        }
    }

    protected override void Die()
    {
        
    }

    #endregion
}
#pragma warning restore CS0618