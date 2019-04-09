using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using static StatusEffect;

#pragma warning disable CS0618, 0649
public abstract class BattleActorBase : NetworkBehaviour
{
    public bool IsAlive { get { return Health > 0; } }
    public BattleStats BattleStats { get { return battleStats; } }
    public Transform UITransform { get { return uiTransform; } }
    public int AttackDamage
    {
        get
        {
            if (HasStatusEffect(Stat.Weak))
                return battleStats.BasicDamage / 2;
            return battleStats.BasicDamage;
        }
    }
    public int Health
    {
        get { return health; }
        protected set
        {
            int hp = Mathf.Clamp(value, 0, battleStats.MaxHealth);
            if (IsAlive && hp == 0)
            {
                Die();
            }
            health = hp;
            HealthBar.UpdateHealth();
        }
    }
    public int RemainingBlock
    {
        get { return remainingBlock; }
        protected set
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
    public HealthBarUI HealthBar { get { return healthBarUI; } }

    [SerializeField] protected Animator animator;
    [SerializeField] protected DamagePopup damagePopup;
    [SerializeField] protected HealthBarUI healthBarUI;
    [SerializeField] protected StatusEffectOverlays overlays;
    [SerializeField] protected Transform uiTransform;
    [SerializeField] protected BattleStats battleStats;
    [SerializeField] protected bool TEST_immortal;

    private int health;
    private int remainingBlock;

    private Dictionary<Stat, List<StatusEffect>> statusEffects = new Dictionary<Stat, List<StatusEffect>>();

    //TODO: REMOVE AND REPLACE
    public GameObject tempAbilityTarget;
    
    public enum BattleAnimation
    {
        Attack, Hurt, Die, Revive
    }
    
    #region Initialization

    [Server]
    public virtual void OnPlayerPhaseStart()
    {
        if (!IsAlive && TEST_immortal)
        {
            Heal(battleStats.MaxHealth);
            PlayAnimation(BattleAnimation.Revive);
        }
    }

    private void OnValidate()
    {
        //BattleStats.AppliedEffects.Stats = new Stat[0];
        //BattleStats.Immunities.Stats = new Stat[0];

        //Validate Debuffs
        for (int i = 0; i < BattleStats.Immunities.Stats.Length; i++)
        {
            if (BattleStats.Immunities.Stats[i] != Stat.None && !Debuffs.Contains(BattleStats.Immunities.Stats[i]))
            {
                List<Stat> temp = BattleStats.Immunities.Stats.ToList();
                temp.RemoveAt(i);
                i--;
                BattleStats.Immunities.Stats = temp.ToArray();
            }
        }

        //Serialize Stat Pools
        BattleStats.Immunities.BuffPool = Debuffs.Where(x => !BattleStats.Immunities.Stats.Contains(x)).ToArray();
        BattleStats.AppliedEffects.BuffPool = All.Where(x => !BattleStats.AppliedEffects.Stats.Contains(x)).ToArray();
    }

    public void SetReferences(Animator a, DamagePopup d, HealthBarUI h, StatusEffectOverlays o, Transform UITransform, GameObject abilityTarget)
    {
        animator = a;
        damagePopup = d;
        healthBarUI = h;
        overlays = o;
        uiTransform = UITransform;
        tempAbilityTarget = abilityTarget;
    }

    private void Start()
    {
        StartCoroutine(DelayInitialize());
    }

    private IEnumerator DelayInitialize()
    {
        yield return new WaitUntil(() => BattleController.Instance != null);
        Initialize();
    }

    protected virtual void Initialize()
    {
        health = battleStats.MaxHealth;
        healthBarUI.Init(this);
        damagePopup.Init(this);
    }

    private void OnDestroy()
    {
        if (healthBarUI != null)
            Destroy(healthBarUI.gameObject);
        if (damagePopup != null)
            Destroy(damagePopup.gameObject);
    }
    
    #endregion

    #region Attack

    //Make sure this is called on the machine that owns this object: i.e. the LocalAuthority (for players) or the server (for enemies)
    public virtual void PlayAnimation(BattleAnimation type)
    {
        switch (type)
        {
            case BattleAnimation.Attack:
                animator.SetTrigger("Attack");
                break;
            case BattleAnimation.Hurt:
                animator.SetTrigger("Hurt");
                break;
            case BattleAnimation.Die:
                animator.SetTrigger("Die");
                break;
            case BattleAnimation.Revive:
                animator.SetTrigger("Revive");
                break;
        }
    }

    [Server]
    protected bool TryAttack()
    {
        if (HasStatusEffect(Stat.Blind))
        {
            return UnityEngine.Random.Range(0, 100) > 80;
        }
        return true;
    }
    
    #endregion

    #region Damage

    [Server]
    public abstract int TakeBlockedDamage(int damage);

    [ClientRpc]
    private void RpcUpdateBlock(int value)
    {
        RemainingBlock = value;
    }

    protected virtual void Die()
    {
        PlayAnimation(BattleAnimation.Die);
    }

    [Server]
    public void DispatchBlockableDamage(BattleActorBase attacker, int damage = 0)
    {
        DispatchBlockableDamage(new List<BattleActorBase>() { attacker }, damage);
    }

    [Server]
    public void DispatchBlockableDamage(List<BattleActorBase> attackers, int damage = 0)
    {
        if (HasStatusEffect(Stat.Freeze))
        {
            RpcDecrementDuration(Stat.Freeze, attackers.Count);
            RpcDisplayStat(Stat.Freeze, 0);
        }

        if (HasStatusEffect(Stat.Reflect))
        {
            foreach (BattleActorBase attacker in attackers)
            {
                int damageDealt = (damage > 0) ? damage : attacker.AttackDamage;
                attacker.TakeBlockedDamage(damageDealt);
            }
        }
        else
        {
            int sumDamage = 0;
            if (damage > 0)
            {
                sumDamage = damage * attackers.Count;
            }
            else
            {
                foreach (BattleActorBase attacker in attackers)
                {
                    sumDamage += attacker.AttackDamage;
                }
            }

            if (HasStatusEffect(Stat.Protected))
            {
                BattleActorBase protector = GetOtherActor(Stat.Protected);
                protector.TakeBlockedDamage(sumDamage);
            }
            else
            {
                TakeBlockedDamage(sumDamage);
            }
        }
    }
    
    [Server]
    protected void TakeDamage(int damageTaken, int blocked)
    {
        if (damageTaken > 0)
            PlayAnimation(BattleAnimation.Hurt);
        RpcTakeDamage(damageTaken, blocked);
    }

    [ClientRpc]
    protected void RpcTakeDamage(int damageTaken, int blocked)
    {
        Health -= damageTaken;
        damagePopup.DisplayDamage(damageTaken, blocked);
    }

    [ClientRpc]
    public void RpcMiss()
    {
        damagePopup.DisplayMiss();
    }
    
    public void Heal(int amount)
    {
        Health += amount;
        //TODO: hp popup like damage popup
    }

    #endregion

    #region StatusEffects
    
    protected virtual void OnAddBleed() {}
    protected virtual void OnAddBlind() {}
    protected virtual void OnAddBurn() {}
    protected virtual void OnAddFocus() {}
    protected virtual void OnAddFreeze() {}
    protected virtual void OnAddInvisible() {}
    protected virtual void OnAddPoison() {}
    protected virtual void OnAddProtected() {}
    protected virtual void OnAddReflect() {}
    protected virtual void OnAddStun() {}
    protected virtual void OnAddWeak() {}
    protected virtual void OnAddCure() {}

    public bool HasStatusEffect(Stat type)
    {
        return statusEffects.ContainsKey(type);
    }

    public BattleActorBase GetOtherActor(Stat type)
    {
        if (HasStatusEffect(type))
            return statusEffects[type][0].LinkedActor;
        return null;
    }

    [Server] //stacks the duration if it is already in the list
    public void AddStatusEffect(Stat effect, BattleActorBase otherActor, int duration, int healthOnRemove = 0)
    {
        if (!battleStats.Immunities.Stats.Any(x => x == effect))
        {
            RemoveOnAdd(effect);
            RpcAddStatusEffect(effect, otherActor.gameObject, duration, healthOnRemove);
        }
        else
        {
            RpcDisplayImmunePopup(effect);
        }
    }

    [ClientRpc]
    private void RpcDisplayImmunePopup(Stat effect)
    {
        damagePopup.DisplayMessage("Immune", GetStatColor(effect));
    }

    [ClientRpc]
    private void RpcAddStatusEffect(Stat type, GameObject otherActor, int duration, int healthOnRemove)
    {
        BattleController.Instance.PlaySoundEffect(type);
        StatusEffect s = new StatusEffect(type, otherActor.GetComponent<BattleActorBase>(), duration, healthOnRemove);
        
        if (type == Stat.Cure)
        {
            Heal(healthOnRemove);
            return;
        }

        damagePopup.DisplayStat(GetStatColor(type), duration, "+");
        overlays.ToggleOverlay(type, true);

        if (HasStatusEffect(type))
            AddStack(s);
        else
            statusEffects.Add(type, new List<StatusEffect>() { s });

        switch (s.Type)
        {
            case Stat.Bleed:
                OnAddBleed();
                break;
            case Stat.Blind:
                OnAddBlind();
                break;
            case Stat.Burn:
                OnAddBurn();
                break;
            case Stat.Focus:
                OnAddFocus();
                break;
            case Stat.Freeze:
                OnAddFreeze();
                break;
            case Stat.Invisible:
                OnAddInvisible();
                break;
            case Stat.Poison:
                OnAddPoison();
                break;
            case Stat.Protected:
                OnAddProtected();
                break;
            case Stat.Reflect:
                OnAddReflect();
                break;
            case Stat.Stun:
                OnAddStun();
                break;
            case Stat.Weak:
                OnAddWeak();
                break;
            case Stat.Cure:
                OnAddCure();
                break;
            default:
                break;
        }
    }

    private void AddStack(StatusEffect s)
    {
        int maxDuration = 10;
        switch (s.Type)
        {
            case Stat.Bleed:
                //stack damage
                statusEffects[s.Type].Add(s);
                break;
            case Stat.Blind:
            case Stat.Burn:
            case Stat.Freeze:
            case Stat.Poison:
            case Stat.Stun:
            case Stat.Weak:
                //stack duration
                int duration = statusEffects[s.Type][0].RemainingDuration + s.RemainingDuration;
                statusEffects[s.Type][0].RemainingDuration = Mathf.Min(duration, maxDuration);
                break;
            default:
                //dont stack
                break;
        }
    }

    [Server]
    private void RemoveOnAdd(Stat type)
    {
        switch (type)
        {
            case Stat.Burn:
                RemoveStatusEffect(Stat.Freeze);
                break;
            case Stat.Blind:
                RemoveStatusEffect(Stat.Focus);
                break;
            case Stat.Freeze:
                RemoveStatusEffect(Stat.Burn);
                RemoveStatusEffect(Stat.Focus);
                break;
            case Stat.Stun:
                RemoveStatusEffect(Stat.Focus);
                break;
            case Stat.Protected:
                RemoveStatusEffect(Stat.Protected);
                break;
            case Stat.Cure:
                RemoveStatusEffect(Stat.Bleed);
                RemoveStatusEffect(Stat.Blind);
                RemoveStatusEffect(Stat.Burn);
                RemoveStatusEffect(Stat.Freeze);
                RemoveStatusEffect(Stat.Poison);
                RemoveStatusEffect(Stat.Stun);
                RemoveStatusEffect(Stat.Weak);
                break;
            default:
                break;
        }
    }

    [Server]
    public void RemoveStatusEffect(Stat type)
    {
        if (HasStatusEffect(type))
            RpcRemoveStatusEffect(type);
    }

    [ClientRpc]
    private void RpcRemoveStatusEffect(Stat type)
    {
        if (!HasStatusEffect(type))
            return;

        switch (type)
        {
            case Stat.Focus:
                OnRemoveFocus();
                break;
            default:
                break;
        }
        statusEffects.Remove(type);
        overlays.ToggleOverlay(type, false);
    }

    private void OnRemoveFocus()
    {
        StatusEffect s = statusEffects[Stat.Focus][0];
        if (s.RemainingDuration > 0)
        {
            damagePopup.DisplayInterrupt();
        }
        else
        {
            s.LinkedActor.Heal(s.HealthOnRemove);
        }
    }
    
    [Server]
    public bool ApplyDOT(Stat type)
    {
        if (!HasStatusEffect(type))
            return false;

        int dot = 0;
        for (int i = 0; i < statusEffects[type].Count; i++)
        {
            dot += statusEffects[type][i].DOT;
        }

        RpcDisplayStat(type, -1);
        BattleController.Instance.RpcPlaySoundEffect(type);
        TakeDamage(dot, 0);
        return true;
    }

    [ClientRpc]
    public void RpcDisplayStat(Stat type, int durationOffset)
    {
        List<StatusEffect> stats = statusEffects[type];
        damagePopup.DisplayStat(GetStatColor(type), stats[stats.Count - 1].RemainingDuration + durationOffset);
    }

    private Color GetStatColor(Stat type)
    {
        switch (type)
        {
            case Stat.Bleed:
            case Stat.Weak:
                return Color.red;
            case Stat.Burn:
                return new Color(1, 0.5f, 0);
            case Stat.Blind:
            case Stat.Invisible:
                return Color.white;
            case Stat.Poison:
            case Stat.Cure:
                return Color.green;
            case Stat.Focus:
            case Stat.Protected:
            case Stat.Stun:
                return Color.yellow;
            case Stat.Freeze:
                return Color.cyan;
            case Stat.Reflect:
                return Color.magenta;
            default:
                return Color.black;
        }
    }

    [ClientRpc]
    public void RpcDecrementDurations()
    {
        List<Stat> types = new List<Stat>(statusEffects.Keys);
        for (int i = 0; i < types.Count; i++)
        {
            DecrementDuration(types[i], 1);
        }
    }

    [ClientRpc]
    public void RpcDecrementDuration(Stat type, int decBy)
    {
        if (!HasStatusEffect(type))
            return;

        DecrementDuration(type, decBy);
    }

    private void DecrementDuration(Stat type, int decBy)
    {
        List<StatusEffect> s = statusEffects[type];
        for (int i = 0; i < s.Count; i++)
        {
            s[i].RemainingDuration -= decBy;
            if (s[i].RemainingDuration <= 0)
            {
                if (s.Count > 1)
                {
                    s.RemoveAt(i);
                    i--;
                }
                else if (isServer)
                {
                    RemoveStatusEffect(type);
                }
            }
        }
    }

#endregion
}