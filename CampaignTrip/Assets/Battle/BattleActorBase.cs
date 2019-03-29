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
    public int BlockAmount { get { return blockAmount; } }
    public int MaxHealth { get; private set; }
    public Transform UITransform { get { return uiTransform; } }

    public int Health
    {
        get { return health; }
        protected set
        {
            int hp = Mathf.Clamp(value, 0, MaxHealth);
            if (IsAlive && hp == 0)
            {
                Die();
            }
            health = hp;
            HealthBar.UpdateHealth();
        }
    }
    
    protected HealthBarUI HealthBar { get { return healthBarUI; } }

    [SerializeField] protected Animator animator;
    [SerializeField] protected DamagePopup damagePopup;
    [SerializeField] protected HealthBarUI healthBarUI;
    [SerializeField] protected Transform uiTransform;
    [SerializeField] protected int attacksPerTurn;
    [SerializeField] protected int basicDamage;
    [SerializeField] protected int blockAmount;
    [SerializeField] protected int health;
    
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
        Weak,       //Lowers block and attack damage.
        Cure
    }
    
    private class Stat
    {
        //can be the person you are healing or the person protecting you
        public BattleActorBase OtherActor;
        public StatusEffect Type;
        public int HealthOnRemove;
        public int RemainingDuration;
        public int DOT;

        public Stat(StatusEffect effect, BattleActorBase otherActor, int duration, int healthGain = 0)
        {
            Type = effect;
            OtherActor = otherActor;
            HealthOnRemove = healthGain;
            RemainingDuration = duration;
            DOT = BattleController.Instance.GetDOT(effect);
        }
    }

    #region Initialization

    public void Initialize()
    {
        MaxHealth = health;
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

    public virtual void OnPlayerPhaseStart()
    {
        if (HasStatusEffect(StatusEffect.Focus))
        {
            Stat s = statusEffects[StatusEffect.Focus][0];
            if (s.RemainingDuration <= 0)
            {
                RemoveStatusEffect(StatusEffect.Focus);
            }
        }
    }

    #endregion

    [Server]
    protected bool TryAttack(BattleActorBase target)
    {
        if (HasStatusEffect(StatusEffect.Blind) && UnityEngine.Random.Range(0, 100) < 80)
        {
            target.RpcMiss();
            return false;
        }
        return true;
    }

    [ClientRpc]
    public void RpcMiss()
    {
        damagePopup.DisplayMiss();
    }

    #region Damage

    [Server]
    public void DispatchDamage(BattleActorBase attacker, int damage, bool canBlock)
    {
        if (canBlock)
        {
            if (HasStatusEffect(StatusEffect.Reflect))
            {
                if (attacker != null)
                {
                    attacker.DispatchDamage(null, damage, false);
                }
            }
            else if (HasStatusEffect(StatusEffect.Protected))
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
        damagePopup.DisplayDamage(damageTaken, blocked);
    }

    protected abstract void Die();

    public void Heal(int amount)
    {
        Health += amount;
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
        if (HasStatusEffect(type))
            return statusEffects[type][0].OtherActor;
        return null;
    }

    [Server] //stacks the duration if it is already in the list
    public void AddStatusEffect(StatusEffect effect, BattleActorBase otherActor, int duration, int healthOnRemove = 0)
    {
        RpcAddStatusEffect(effect, otherActor.gameObject, duration, healthOnRemove);
    }

    [ClientRpc]
    private void RpcAddStatusEffect(StatusEffect type, GameObject otherActor, int duration, int healthOnRemove)
    {
        Stat s = new Stat(type, otherActor.GetComponent<BattleActorBase>(), duration, healthOnRemove);

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
            case StatusEffect.Invisible:
                OnChangeInvisible(true);
                break;
            case StatusEffect.Stun:
                OnAddStun();
                break;
            case StatusEffect.Freeze:
                OnAddFreeze();
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
                RemoveStatusEffect(StatusEffect.Freeze);
                break;
            case StatusEffect.Blind:
                RemoveStatusEffect(StatusEffect.Focus);
                break;
            case StatusEffect.Freeze:
                RemoveStatusEffect(StatusEffect.Burn);
                RemoveStatusEffect(StatusEffect.Focus);
                break;
            case StatusEffect.Stun:
                RemoveStatusEffect(StatusEffect.Focus);
                break;
            case StatusEffect.Protected:
                RemoveStatusEffect(StatusEffect.Protected);
                break;
            case StatusEffect.Cure:
                RemoveStatusEffect(StatusEffect.Bleed);
                RemoveStatusEffect(StatusEffect.Blind);
                RemoveStatusEffect(StatusEffect.Burn);
                RemoveStatusEffect(StatusEffect.Freeze);
                RemoveStatusEffect(StatusEffect.Poison);
                RemoveStatusEffect(StatusEffect.Stun);
                RemoveStatusEffect(StatusEffect.Weak);
                break;
            default:
                break;
        }
    }

    private void RemoveStatusEffect(StatusEffect type)
    {
        if (!HasStatusEffect(type))
            return;
        
        switch (type)
        {
            case StatusEffect.Invisible:
                OnChangeInvisible(false);
                break;
            case StatusEffect.Focus:
                OnRemoveFocus();
                break;
            default:
                break;
        }
        statusEffects.Remove(type);
    }
    
    private void OnChangeInvisible(bool added)
    {
        SpriteRenderer s = GetComponent<SpriteRenderer>();
        Color c = s.color;

        if (added)
        {
            c.a = 0.5f;
            OnAddInvisible();
        }
        else
        {
            c.a = 1;
        }
        s.color = c;
    }

    private void OnRemoveFocus()
    {
        Stat s = statusEffects[StatusEffect.Focus][0];
        if (s.RemainingDuration > 0)
        {
            damagePopup.DisplayInterrupt();
        }
        else
        {
            s.OtherActor.Heal(s.HealthOnRemove);
        }
    }

    protected virtual void OnAddStun() { }
    protected virtual void OnAddInvisible() { }
    protected virtual void OnAddFreeze() { }


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
                if (type == StatusEffect.Focus)
                    return;

                s.RemoveAt(i);
                i--;
            }
        }

        if (s.Count == 0)
        {
            RemoveStatusEffect(type);
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
            DispatchDamage(null, damage, false);
            yield return new WaitForSeconds(0.5f);
        }
    }

#endregion
}