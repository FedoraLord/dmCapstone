using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649 
[CreateAssetMenu(fileName = "NewStatusEffect", menuName = "Data Object/StatusEffect")]
public class StatusEffect : ScriptableObject
{
    public int Duration { get { return duration; } }
    public int EffectAmount { get { return effectAmount; } }
    public StatusEffectType Type { get { return type; } }
    
    [SerializeField] private int duration;
    [SerializeField] private int effectAmount;
    [SerializeField] private StatusEffectType type;

    public enum StatusEffectType
    {
        Bleed,
        Blind,
        Burn,
        Focus,
        Freeze,
        Invisible,
        Poison,
        Protected,
        Reflect,
        Stun,
        Weak
    }
}
#pragma warning restore 0649 