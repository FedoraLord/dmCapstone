using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618, 0649
public abstract class BattlePlayerBase : BattleActorBase
{
    public static BattlePlayerBase LocalAuthority { get { return PersistentPlayer.localAuthority.battlePlayer; } }
    //public static BP_Mage Mage { get; protected set; }
    //public static BP_Rogue Rogue { get; protected set; }
    //public static BP_Warrior Warrior { get; protected set; }
    //public static BP_Alchemist Alchemist { get; protected set; }
    public static int PlayersUsingAbility;

    public static List<BattlePlayerBase> players { get { return PersistentPlayer.players.Select(x => x.battlePlayer).ToList(); } }

    public bool CanPlayAbility { get; set; }
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
                    return new List<BattleActorBase>(BattleController.Instance.aliveEnemies);
                default:
                    Debug.LogErrorFormat("Valid targets not implemented for {0}", SelectedAbility.Targets);
                    return new List<BattleActorBase>();
            }
        }
    }
    protected List<BattleActorBase> customTargets;
    protected int selectedAbilityIndex = -1;

    private int attacksRemaining;

    public enum TargetGroup { Override, Ally, Self, AllyAndSelf, Enemy }

    [Serializable]
    public class Ability
    {
        public AbilityButton AButton { get; set; }
        public bool TargetAll { get { return targetAll; } }
        public int Damage { get { return damage; } }
        public int Duration { get { return duration; } }
        public TargetGroup Targets { get { return targetGroup; } }
        public StatusEffect Applies { get { return applies; } }

        [HideInInspector] public int RemainingCooldown;
        
        [SerializeField] private string abilityName;
        [SerializeField] private int damage;
        [SerializeField] private int duration;
        [SerializeField] private int cooldown;
        [SerializeField] private bool targetAll;
        [SerializeField] private TargetGroup targetGroup;
        [SerializeField] private StatusEffect applies;
        [SerializeField] private Sprite buttonIcon;
        
        //TODO:
        [HideInInspector] public bool IsUpgraded;
        
        public void SetButton(AbilityButton button)
        {
            AButton = button;
            AButton.nameText.text = abilityName;
            if (buttonIcon != null)
            {
                AButton.iconImage.sprite = buttonIcon;
            }
        }

        public void Use()
        {
            LocalAuthority.CanPlayAbility = false;
            RemainingCooldown = cooldown + 1;
            UpdateButtonUI();
        }

        public void DecrementCooldown()
        {
            if (RemainingCooldown > 0)
                RemainingCooldown--;

            if (AButton != null)
                UpdateButtonUI();
        }

        private void UpdateButtonUI()
        {
            AButton.button.interactable = (RemainingCooldown <= 0);
            AButton.UpdateCooldown(RemainingCooldown, cooldown + 1);
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

        transform.position = BattleController.Instance.battleCam.PlayerSpawnPoints[i].position;

        Abilities = new List<Ability>() { ability1, ability2, ability3 };
        BattleController.Instance.OnBattlePlayerSpawned(this);

        Initialize();
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
        if (HasStatusEffect(StatusEffect.Stun) || HasStatusEffect(StatusEffect.Freeze))
        {
            attacksRemaining = 0;
            blockAmount = 90;
            CanPlayAbility = false;
        }
        else
        {
            attacksRemaining = attacksPerTurn;
            blockAmount = (HasStatusEffect(StatusEffect.Weak) ? 0 : 90);
            CanPlayAbility = true;
        }

        if (this == LocalAuthority)
            BattleController.Instance.UpdateAttackBlockUI(attacksRemaining, blockAmount);

        foreach (Ability a in Abilities)
            a.DecrementCooldown();
    }

    #endregion

    #region Attack

    private void OnMouseUpAsButton()
    {
        if (IsAlive)
        {
            if (LocalAuthority.SelectedAbility != null && LocalAuthority.IsValidTarget(this))
            {
                LocalAuthority.OnAbilityTargetChosen(this);
            }
            else if (HasStatusEffect(StatusEffect.Burn))
            {

            }
            else if (HasStatusEffect(StatusEffect.Freeze))
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

        if (!HasStatusEffect(StatusEffect.Weak))
            blockAmount = (int)Mathf.Min((float)attacksRemaining / attacksPerTurn * 100f, 90);

        if (this == LocalAuthority)
        {
            animator.SetTrigger("Attack");
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

    protected abstract void OverrideTargeting();
    protected abstract void OnAbilityUsed();
    
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
        DisableAbilityButtons();
        EndAbility();
    }

    private void DisableAbilityButtons()
    {
        foreach (Ability a in Abilities)
        {
            a.AButton.button.interactable = false;
        }
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
        if (a.Applies != StatusEffect.None)
        {
            if (a.Applies == StatusEffect.Cure || a.Applies == StatusEffect.Focus)
            {
                target.AddStatusEffect(a.Applies, this, a.Duration, a.Damage);
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
        int blocked = damage * blockAmount / 100;
        int damageTaken = damage - blocked;
        RpcTakeDamage(damageTaken, blocked);
        return damageTaken;
    }

    protected override void Die()
    {
        
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

    #endregion
}
