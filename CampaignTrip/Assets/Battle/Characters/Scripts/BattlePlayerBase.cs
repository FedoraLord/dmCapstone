using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static StatusEffect;

#pragma warning disable CS0618, 0649
public abstract class BattlePlayerBase : BattleActorBase
{
    /// <summary>
    /// This value is only maintained on the Server
    /// </summary>
    public static Ability SelectedAbility { get; private set; }
    public static BattlePlayerBase LocalAuthority { get { return PersistentPlayer.localAuthority.battlePlayer; } }
    //public static BP_Mage Mage { get; protected set; }
    //public static BP_Rogue Rogue { get; protected set; }
    //public static BP_Warrior Warrior { get; protected set; }
    //public static BP_Alchemist Alchemist { get; protected set; }
    public static int PlayersUsingAbility;

    public bool AbilityPlayedThisTurn { get; set; }
    public bool IsUsingAbility { get; private set; }
    
    [SyncVar] public int playerNum;

    [HideInInspector] public PersistentPlayer persistentPlayer;

    [SerializeField] protected Ability ability1;
    [SerializeField] protected Ability ability2;
    [SerializeField] protected Ability ability3;

    protected List<Ability> Abilities;

    private int attacksRemaining;

    public enum TargetMode { Auto, Friend, Foe }

    [Serializable]
    public class Ability
    {
        public Action<BattleActorBase> ExecuteAbility { get; set; }
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
        
        public void Bind(Action<BattleActorBase> action)
        {
            ExecuteAbility = action;
        }

        public void Use(BattleActorBase target)
        {
            LocalAuthority.AbilityPlayedThisTurn = true;
            RemainingCooldown = cooldown;
            ExecuteAbility(target);
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
        
        Abilities = new List<Ability>() { ability1, ability2, ability3 };
        ability1.Bind(Ability1);
        ability2.Bind(Ability2);
        ability3.Bind(Ability3);

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
        if (IsAlive && LocalAuthority.IsUsingAbility && SelectedAbility.Targets == TargetMode.Friend)
        {
            LocalAuthority.OnAbilityTargetChosen(this);
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

    public void AbilitySelected(int i)
    {
        if (SelectedAbility != null)
        {
            EndAbility();
        }
        else if (!AbilityPlayedThisTurn && Abilities[i].RemainingCooldown <= 0)
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
        SelectedAbility = Abilities[i];
        IsUsingAbility = true;

        switch (SelectedAbility.Targets)
        {
            case TargetMode.Auto:
                SelectedAbility.Use(null);
                EndAbility();
                break;
            case TargetMode.Friend:
                ToggleTargetFriends(true);
                break;
            case TargetMode.Foe:
                ToggleTargetFoes(true);
                break;
        }
    }

    private void ToggleTargetFriends(bool active)
    {
        foreach (PersistentPlayer p in PersistentPlayer.players)
        {
            p.battlePlayer.tempAbilityTarget.SetActive(active);
        }
    }

    private void ToggleTargetFoes(bool active)
    {
        foreach (EnemyBase e in BattleController.Instance.aliveEnemies)
        {
            e.tempAbilityTarget.SetActive(active);
        }
    }

    public void OnAbilityTargetChosen(BattleActorBase target)
    {
        SelectedAbility.Use(target);
        EndAbility();
    }

    //override these implementations if the ability is a little more complex than what CmdUseAbility handles
    public virtual void Ability1(BattleActorBase target)
    {
        CmdUseAbility(target?.gameObject, 0);
    }

    public virtual void Ability2(BattleActorBase target)
    {
        CmdUseAbility(target?.gameObject, 1);
    }

    public virtual void Ability3(BattleActorBase target)
    {
        CmdUseAbility(target?.gameObject, 2);
    }

    [Command] //This method is used for abilities with a simple setup (attack all/selected enemy(s)/player(s) and apply some status effect)
    protected void CmdUseAbility(GameObject target, int i)
    {
        if (target == null)
        {
            foreach (EnemyBase e in BattleController.Instance.aliveEnemies)
            {
                UseAbility(e, Abilities[i]);
            }
        }
        else
        {
            BattleActorBase actor = target.GetComponent<BattleActorBase>();
            UseAbility(actor, Abilities[i]);
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
        ToggleTargetFriends(false);
        ToggleTargetFoes(false);
        SelectedAbility = null;
        IsUsingAbility = false;
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