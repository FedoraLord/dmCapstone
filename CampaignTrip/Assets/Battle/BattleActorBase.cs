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
    public List<StatusEffect> StatusEffects { get; protected set; }
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

    private Dictionary<StatusEffectType, int> remainingDurations;
    private int health;

    //TODO: REMOVE AND REPLACE
    public GameObject tempAbilityTarget;

    public void Initialize()
    {
        Health = maxHealth;
        healthBarUI.Init(this);
        damagePopup.Init(this);
        StatusEffects = new List<StatusEffect>();
        remainingDurations = new Dictionary<StatusEffectType, int>();
    }

    private void OnDestroy()
    {
        if (healthBarUI != null)
            Destroy(healthBarUI.gameObject);
        if (damagePopup != null)
            Destroy(damagePopup.gameObject);
    }

    protected abstract void Die();

    public bool HasStatusEffect(StatusEffectType type)
    {
        return remainingDurations.ContainsKey(type);
    }

    [Server] //stacks the duration if it is already in the list
    public void AddStatusEffect(StatusEffect add)
    {
        RpcAddStatusEffect(add.Type);
    }

    [ClientRpc]
    private void RpcAddStatusEffect(StatusEffectType type)
    {
        StatusEffect add = BattleController.Instance.GetStatusEffect(type);

        if (HasStatusEffect(type))
        {
            int current = remainingDurations[type];
            remainingDurations[type] = current + add.Duration;
            return;
        }

        StatusEffects.Add(add);
        remainingDurations.Add(add.Type, add.Duration);

        switch (type)
        {
            case StatusEffectType.Bleed:
                break;
            case StatusEffectType.Blind:
                break;
            case StatusEffectType.Burn:
                OnAddBurn();
                break;
            case StatusEffectType.Focus:
                break;
            case StatusEffectType.Freeze:
                OnAddFreeze();
                break;
            case StatusEffectType.Invisible:
                break;
            case StatusEffectType.Poison:
                break;
            case StatusEffectType.Protected:
                break;
            case StatusEffectType.Reflect:
                break;
            case StatusEffectType.Stun:
                OnAddStun();
                break;
            case StatusEffectType.Weak:
                break;
            default:
                break;
        }
    }

    protected void OnAddBurn()
    {
        if (HasStatusEffect(StatusEffectType.Freeze))
        {
            remainingDurations.Remove(StatusEffectType.Freeze);
        }
    }

    protected void OnAddFreeze()
    {
        if (HasStatusEffect(StatusEffectType.Burn))
        {
            remainingDurations.Remove(StatusEffectType.Burn);
        }
    }

    protected virtual void OnAddStun()
    {
        
    }

    [Server]
    public IEnumerator ApplySatusEffects()
    {
        for (int i = 0; i < StatusEffects.Count; i++)
        {
            StatusEffect s = StatusEffects[i];
            yield return DamageOverTime(s);

            int duration = remainingDurations[s.Type] - 1;
            if (duration <= 0)
            {
                remainingDurations.Remove(s.Type);
                StatusEffects.RemoveAt(i);
                i--;
            }
            else
            {
                remainingDurations[s.Type] = duration;
            }
        }
    }

    [Server]
    private IEnumerator DamageOverTime(StatusEffect s)
    {
        switch (s.Type)
        {
            case StatusEffectType.Bleed:
            case StatusEffectType.Burn:
            case StatusEffectType.Freeze:
            case StatusEffectType.Poison:
                RpcTakeStatusEffectDamage(s.Type);
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
    
    [ClientRpc]
    private void RpcTakeStatusEffectDamage(StatusEffectType type)
    {
        StatusEffect s = BattleController.Instance.GetStatusEffect(type);
        Health -= s.EffectAmount;
        HealthBar.SetHealth(health);
        damagePopup.Display(s.EffectAmount, 0);
    }
}
#pragma warning restore CS0618, 0649