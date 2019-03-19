using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    protected List<BattleActorBase> ValidTargets
    {
        get
        {
            if (SelectedAbility == null || SelectedAbility.Targets == TargetMode.Enemy)
            {
                return new List<BattleActorBase>(BattleController.Instance.aliveEnemies);
            }
            else if (SelectedAbility.Targets == TargetMode.OverrideAuto || SelectedAbility.Targets == TargetMode.OverrideSelect)
            {
                return customTargets;
            }
            else
            {
                List<BattleActorBase> targets = PersistentPlayer.players.Select(x => x.battlePlayer).ToList<BattleActorBase>();
                if (SelectedAbility.Targets == TargetMode.Ally)
                    targets.Remove(this);
                return targets;
            }
        }
    }
    protected List<BattleActorBase> customTargets;
    protected int selectedAbilityIndex = -1;

    private int attacksRemaining;

    public enum TargetMode { OverrideAuto, OverrideSelect, Ally, AllyAndSelf, Enemy }

    [Serializable]
    public class Ability
    {
        public int Damage { get { return damage; } }
        public int Duration { get { return duration; } }
        public Sprite ButtonIcon { get { return buttonIcon; } }
        public string Name { get { return abilityName; } }
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

        [Tooltip("Also used for heal amount")]
        [SerializeField] private int damage;
        [SerializeField] private int duration;
        [SerializeField] private int cooldown;
        [SerializeField] private TargetMode targets;
        [SerializeField] private StatusEffectType applies;
        [SerializeField] private string abilityName;
        [SerializeField] private Sprite buttonIcon;

        private StatusEffect statusEffect;

        //TODO:
        [HideInInspector] public bool IsUpgraded;
        
        public void Use()
        {
            LocalAuthority.AbilityPlayedThisTurn = true;
            RemainingCooldown = cooldown + 1;
        }

        public void DecrementCooldown()
        {
            if (RemainingCooldown > 0)
                RemainingCooldown--;
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
        if (this == LocalAuthority)
        {
            BattleController.Instance.OnBattlePlayerSpawned(abilities);
        }
        
        Initialize();
    }

    [Server]
    public void OnPlayerPhaseStart()
    {
        RpcOnPlayerPhaseStart();
    }

    [ClientRpc]
    private void RpcOnPlayerPhaseStart()
    {
        LocalAuthority.AbilityPlayedThisTurn = false;
        UpdateAttackBlock(attacksPerTurn);

        foreach (Ability a in abilities)
        {
            a.DecrementCooldown();
        }
    }

    #endregion

    #region Attack

    private void OnMouseUpAsButton()
    {
        if (IsAlive && LocalAuthority.IsValidTarget(this))
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
        return ValidTargets.Contains(target);
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
        if (SelectedAbility.Targets == TargetMode.OverrideAuto)
        {
            OnAbilityTargetChosen(null);
        }
        else
        {
            if (SelectedAbility.Targets == TargetMode.OverrideSelect)
            {
                CustomTargeting();
            }
            ToggleTargetIndicators(true);
        }
    }

    protected abstract void CustomTargeting();
    
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

            foreach (PersistentPlayer player in PersistentPlayer.players)
            {
                player.battlePlayer.tempAbilityTarget.SetActive(false);
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
            CustomTargeting();
            foreach (BattleActorBase actor in ValidTargets)
            {
                UseAbility(actor, SelectedAbility);
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