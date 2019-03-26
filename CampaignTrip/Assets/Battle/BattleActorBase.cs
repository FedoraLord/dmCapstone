using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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
    private Dictionary<StatusEffect, List<Stat>> statusEffects = new Dictionary<StatusEffect, List<Stat>>();

    //TODO: REMOVE AND REPLACE
    public GameObject tempAbilityTarget;
    
    public enum StatusEffect
    {
        Bleed,      //Low damage over time. Stacks Damage.
        Blind,      //High chance to miss when attacking.
        Burn,       //High damage over time. Can be put out by teammates.
        Focus,      //Channeling a heal spell. Can be interrupted after x amount of damage taken.
        Freeze,     //Cannot attack enemies or use abilities. Attacks from enemies or allies helps break the ice.
        Invisible,  //Cannot be targeted by enemies.
        Poison,     //Low damage over time. Stacks duration.
        Protected,  //An ally will take all incomming damage for this actor.
        Reflect,    //All incoming damage will be deflected back at the attacker.
        Stun,       //Cannot attack enemies or use abilities.
        Weak,        //Lowers block and attack damage.
        Cure
    }
    
    private class Stat
    {
        //can be the person you are healing or the person protecting you
        public BattleActorBase OtherActor;
        public StatusEffect Type;
        public int RemainingDuration;
        public int DOT;

        public Stat(StatusEffect effect, BattleActorBase otherActor, int duration)
        {
            Type = effect;
            OtherActor = otherActor;
            RemainingDuration = duration;
            DOT = BattleController.Instance.GetDOT(effect);
        }
    }

    #region Initialization

    public void Initialize()
    {
        Health = maxHealth;
        healthBarUI.Init(this);
        damagePopup.Init(this);
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (healthBarUI != null)
            Destroy(healthBarUI.gameObject);
        if (damagePopup != null)
            Destroy(damagePopup.gameObject);
    }

    #endregion

    #region Damage

    [Server]
    public void DispatchDamage(int damage, bool canBlock)
    {
        if (canBlock)
        {
            if (HasStatusEffect(StatusEffect.Protected))
            {
                BattleActorBase protector = GetOtherActor(StatusEffect.Protected);
                protector.TakeBlockedDamage(damage);
            }
            else
            {
                TakeBlockedDamage(damage);
            }
        }
        else
        {
            RpcTakeDamage(damage, 0);
        }
    }

    public abstract void TakeBlockedDamage(int damage);

    [ClientRpc]
    protected void RpcTakeDamage(int damageTaken, int blocked)
    {
        if (animator != null && damageTaken > 0)
        {
            animator.SetTrigger("Hurt");
        }

        Health -= damageTaken;
        HealthBar.SetHealth(Health);
        damagePopup.Display(damageTaken, blocked);
    }

    protected abstract void Die();

    public void Heal(int amount)
    {
        Health += amount;
        HealthBar.SetHealth(Health);
        //TODO: hp popup like damage popup
    }

    #endregion

    #region StatusEffects

    public bool HasStatusEffect(StatusEffect type)
    {
        return statusEffects.ContainsKey(type);
    }

    public BattleActorBase GetOtherActor(StatusEffect type)
    {
        if (statusEffects.ContainsKey(type))
            return statusEffects[type][0].OtherActor;
        return null;
    }

    [Server] //stacks the duration if it is already in the list
    public void AddStatusEffect(StatusEffect effect, BattleActorBase otherActor, int duration)
    {
        RpcAddStatusEffect(effect, otherActor.gameObject, duration);
    }

    [ClientRpc]
    private void RpcAddStatusEffect(StatusEffect type, GameObject otherActor, int duration)
    {
        Stat s = new Stat(type, otherActor.GetComponent<BattleActorBase>(), duration);

        RemoveOnAdd(type);
        if (type == StatusEffect.Cure)
        {
            return;
        }
        
        if (HasStatusEffect(type))
        {
            AddStack(s);
        }
        else
        {
            statusEffects.Add(type, new List<Stat>() { s });
        }

        switch (s.Type)
        {
            case StatusEffect.Bleed:
                OnAddBleed();
                break;
            case StatusEffect.Blind:
                OnAddBlind();
                break;
            case StatusEffect.Burn:
                OnAddBurn();
                break;
            case StatusEffect.Focus:
                OnAddFocus();
                break;
            case StatusEffect.Freeze:
                OnAddFreeze();
                break;
            case StatusEffect.Invisible:
                OnAddInvisible();
                break;
            case StatusEffect.Poison:
                OnAddPoison();
                break;
            case StatusEffect.Protected:
                OnAddProtected();
                break;
            case StatusEffect.Reflect:
                OnAddReflect();
                break;
            case StatusEffect.Stun:
                OnAddStun();
                break;
            case StatusEffect.Weak:
                OnAddWeak();
                break;
            default:
                break;
        }
    }

    private void AddStack(Stat s)
    {
        int maxDuration = 10;
        switch (s.Type)
        {
            case StatusEffect.Bleed:
                //stack damage
                statusEffects[s.Type].Add(s);
                break;
            case StatusEffect.Blind:
            case StatusEffect.Burn:
            case StatusEffect.Freeze:
            case StatusEffect.Poison:
            case StatusEffect.Stun:
            case StatusEffect.Weak:
                //stack duration
                int duration = statusEffects[s.Type][0].RemainingDuration + s.RemainingDuration;
                statusEffects[s.Type][0].RemainingDuration += Mathf.Min(duration, maxDuration);
                break;
            default:
                //dont stack
                break;
        }
    }

    private void RemoveOnAdd(StatusEffect type)
    {
        switch (type)
        {
            case StatusEffect.Burn:
                statusEffects.Remove(StatusEffect.Freeze);
                break;
            case StatusEffect.Freeze:
                statusEffects.Remove(StatusEffect.Burn);
                break;
            case StatusEffect.Protected:
                statusEffects.Remove(StatusEffect.Protected);
                break;
            case StatusEffect.Cure:
                statusEffects.Remove(StatusEffect.Bleed);
                statusEffects.Remove(StatusEffect.Blind);
                statusEffects.Remove(StatusEffect.Burn);
                statusEffects.Remove(StatusEffect.Freeze);
                statusEffects.Remove(StatusEffect.Poison);
                statusEffects.Remove(StatusEffect.Stun);
                statusEffects.Remove(StatusEffect.Weak);
                break;
            default:
                break;
        }
    }

    protected virtual void OnAddBleed() { }
    protected virtual void OnAddBlind() { }
    protected virtual void OnAddBurn() { }
    protected virtual void OnAddFocus() { }
    protected virtual void OnAddFreeze() { }
    protected virtual void OnAddInvisible() { }
    protected virtual void OnAddPoison() { }
    protected virtual void OnAddProtected() { }
    protected virtual void OnAddReflect() { }
    protected virtual void OnAddWeak() { }
    protected virtual void OnAddStun() { }
    
    public IEnumerator ApplySatusEffects()
    {
        List<StatusEffect> types = new List<StatusEffect>(statusEffects.Keys);
        for (int i = 0; i < types.Count; i++)
        {
            List<Stat> s = statusEffects[types[i]];
            yield return DamageOverTime(s);
            DecrementDuration(types[i]);
        }
    }
    
    private void DecrementDuration(StatusEffect type)
    {
        List<Stat> s = statusEffects[type];
        for (int i = 0; i < s.Count; i++)
        {
            s[i].RemainingDuration--;
            if (s[i].RemainingDuration <= 0)
            {
                s.RemoveAt(i);
                i--;
            }
        }

        if (s.Count == 0)
        {
            statusEffects.Remove(type);
        }
    }

    [Server]
    private IEnumerator DamageOverTime(List<Stat> s)
    {
        if (s[0].DOT > 0)
        {
            int damage = 0;
            for (int i = 0; i < s.Count; i++)
            {
                damage += s[i].DOT;
            }
            DispatchDamage(damage, false);
            yield return new WaitForSeconds(0.5f);
        }
    }

#endregion
}