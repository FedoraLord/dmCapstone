using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static StatusEffect;

#pragma warning disable CS0618, 0649
public abstract class BattleActorBase : NetworkBehaviour
{
    public bool IsAlive { get { return Health > 0; } }
    public int BasicDamage { get { return basicDamage; } }
    public int MaxHealth { get { return maxHealth; } }
    public Transform UITransform { get { return uiTransform; } }

    public int Health
    {
        get { return health; }
        protected set
        {
            int hp = Mathf.Clamp(value, 0, maxHealth);
            if (IsAlive && hp == 0)
            {
                Die();
            }
            health = hp;
        }
    }
    
    protected HealthBarUI HealthBar { get { return healthBarUI; } }

    [SerializeField] protected Animator animator;
    [SerializeField] protected DamagePopup damagePopup;
    [SerializeField] protected HealthBarUI healthBarUI;
    [SerializeField] protected Transform uiTransform;
    [SerializeField] protected int attacksPerTurn;
    [SerializeField] protected int basicDamage;
    [SerializeField] protected int maxHealth;
    
    protected int blockAmount;

    private int health;
    private Dictionary<StatusEffectType, Stat> statusEffects;

    //TODO: REMOVE AND REPLACE
    public GameObject tempAbilityTarget;

    private class Stat
    {
        public int RemainingDuration;
        public int EffectAmount;
        public StatusEffect Effect;
        public BattleActorBase GivenBy;

        public Stat(StatusEffect s, BattleActorBase givenBy, int duration)
        {
            Effect = s;
            EffectAmount = s.EffectAmount;
            GivenBy = givenBy;
            RemainingDuration = duration;
        }
    }

    public void Initialize()
    {
        Health = maxHealth;
        healthBarUI.Init(this);
        damagePopup.Init(this);
        statusEffects = new Dictionary<StatusEffectType, Stat>();
    }

    private void OnDestroy()
    {
        if (healthBarUI != null)
            Destroy(healthBarUI.gameObject);
        if (damagePopup != null)
            Destroy(damagePopup.gameObject);
    }

    [Server]
    public abstract void DispatchDamage(int damage, bool canBlock);

    [ClientRpc]
    protected void RpcTakeDamage(int damageTaken, int blocked)
    {
        //TODO: play damage animation
        Health -= damageTaken;
        HealthBar.SetHealth(Health);
        damagePopup.Display(damageTaken, blocked);
    }

    protected abstract void Die();

    public bool HasStatusEffect(StatusEffectType type)
    {
        return statusEffects.ContainsKey(type);
    }

    public BattleActorBase GetGivenBy(StatusEffectType type)
    {
        return statusEffects[type].GivenBy;
    }

    [Server] //stacks the duration if it is already in the list
    public void AddStatusEffect(StatusEffect add, BattleActorBase givenBy, int duration)
    {
        RpcAddStatusEffect(add.Type, givenBy.gameObject, duration);
    }

    [ClientRpc]
    private void RpcAddStatusEffect(StatusEffectType type, GameObject givenBy, int duration)
    {
        StatusEffect add = BattleController.Instance.GetStatusEffect(type);
        Stat s = new Stat(add, givenBy.GetComponent<BattleActorBase>(), duration);

        if (TryAddStatusEffect(s))
        {
            statusEffects.Add(type, new Stat(add, givenBy.GetComponent<BattleActorBase>(), duration));
        }
    }

    private bool TryAddStatusEffect(Stat s)
    {
        RemoveOnAdd(s.Effect.Type);

        bool exists = HasStatusEffect(s.Effect.Type);
        if (exists)
        {
            Stack(s);
        }
        
        switch (s.Effect.Type)
        {
            case StatusEffectType.Bleed:
                return OnAddBleed(s) && !exists;
            case StatusEffectType.Blind:
                return OnAddBlind(s) && !exists;
            case StatusEffectType.Burn:
                return OnAddBurn(s) && !exists;
            case StatusEffectType.Focus:
                return OnAddFocus(s) && !exists;
            case StatusEffectType.Freeze:
                return OnAddFreeze(s) && !exists;
            case StatusEffectType.Invisible:
                return OnAddInvisible(s) && !exists;
            case StatusEffectType.Poison:
                return OnAddPoison(s) && !exists;
            case StatusEffectType.Protected:
                return OnAddProtected(s) && !exists;
            case StatusEffectType.Reflect:
                return OnAddReflect(s) && !exists;
            case StatusEffectType.Stun:
                return OnAddStun(s) && !exists;
            case StatusEffectType.Weak:
                return OnAddWeak(s) && !exists;
            default:
                return false;
        }
    }

    private void RemoveOnAdd(StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.Burn:
                statusEffects.Remove(StatusEffectType.Freeze);
                break;
            case StatusEffectType.Freeze:
                statusEffects.Remove(StatusEffectType.Burn);
                break;
            case StatusEffectType.Protected:
                statusEffects.Remove(StatusEffectType.Protected);
                break;
            default:
                break;
        }
    }

    private void Stack(Stat s)
    {
        switch (s.Effect.Type)
        {
            case StatusEffectType.Bleed:
                //stack damage
                statusEffects[s.Effect.Type].EffectAmount += s.EffectAmount;
                break;
            case StatusEffectType.Blind:
            case StatusEffectType.Burn:
            case StatusEffectType.Freeze:
            case StatusEffectType.Poison:
            case StatusEffectType.Stun:
            case StatusEffectType.Weak:
                //stack duration
                statusEffects[s.Effect.Type].RemainingDuration += s.RemainingDuration;
                break;
            default:
                //dont stack
                break;
        }
    }

    private bool OnAddBleed(Stat s)
    {
        return true;
    }
            
    private bool OnAddBlind(Stat s)
    {
        throw new NotImplementedException();
    }
            
    private bool OnAddBurn(Stat s)
    {
        if (HasStatusEffect(StatusEffectType.Freeze))
        {
            statusEffects.Remove(StatusEffectType.Freeze);
        }
        throw new NotImplementedException();
    }
            
    private bool OnAddFocus(Stat s)
    {
        throw new NotImplementedException();
    }
            
    private bool OnAddFreeze(Stat s)
    {
        if (HasStatusEffect(StatusEffectType.Burn))
        {
            statusEffects.Remove(StatusEffectType.Burn);
        }
        throw new NotImplementedException();
    }
            
    private bool OnAddInvisible(Stat s)
    {
        throw new NotImplementedException();
    }
            
    private bool OnAddPoison(Stat s)
    {
        throw new NotImplementedException();
    }
            
    private bool OnAddProtected(Stat s)
    {
        throw new NotImplementedException();
    }
            
    private bool OnAddReflect(Stat s)
    {
        throw new NotImplementedException();
    }
            
    private bool OnAddStun(Stat s)
    {
        OnStun();
        return true;
    }
            
    private bool OnAddWeak(Stat s)
    {
        throw new NotImplementedException();
    }

    protected virtual void OnStun()
    {
        
    }

    [Server]
    public IEnumerator ApplySatusEffects()
    {
        List<Stat> stats = new List<Stat>(statusEffects.Values);
        for (int i = 0; i < stats.Count; i++)
        {
            Stat s = stats[i];
            yield return DamageOverTime(s);

            s.RemainingDuration -= 1;
            if (s.RemainingDuration <= 0)
            {
                statusEffects.Remove(s.Effect.Type);
            }
        }
    }

    [Server]
    private IEnumerator DamageOverTime(Stat s)
    {
        switch (s.Effect.Type)
        {
            case StatusEffectType.Bleed:
            case StatusEffectType.Burn:
            case StatusEffectType.Freeze:
            case StatusEffectType.Poison:
                DispatchDamage(s.EffectAmount, false);
                yield return new WaitForSeconds(0.5f);
                break;
            case StatusEffectType.Blind:
            case StatusEffectType.Focus:
            case StatusEffectType.Invisible:
            case StatusEffectType.Protected:
            case StatusEffectType.Reflect:
            case StatusEffectType.Weak:
            default:
                break;
        }
    }
}
#pragma warning restore CS0618, 0649