using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618, 0649
public abstract class BattleActorBase : NetworkBehaviour
{
    public bool IsAlive { get { return Health > 0; } }
    public Transform UITransform { get { return uiTransform; } }
    public int BasicDamage { get { return basicDamage; } }
    public int Health { get; protected set; }
    public int MaxHealth { get { return maxHealth; } }
    
    protected virtual HealthBarUI HealthBar { get; set; }

    [SerializeField] protected Animator animator;
    [SerializeField] protected Transform uiTransform;
    [SerializeField] protected int attacksPerTurn;
    [SerializeField] protected int basicDamage;
    [SerializeField] protected int maxHealth;

    protected DamagePopup damagePopup;
    protected int attacksRemaining;
    protected int blockAmount;
    
    //TODO: REMOVE AND REPLACE
    public GameObject tempAbilityTarget;
}
#pragma warning restore CS0618, 0649