using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 0649 
[CreateAssetMenu(fileName = "NewStatusEffect", menuName = "Data Object/StatusEffect")]
public class StatusEffect : ScriptableObject
{
    public int EffectAmount { get { return effectAmount; } }
    public StatusEffectType Type { get { return type; } }
    
    [SerializeField] private int effectAmount;
    [SerializeField] private StatusEffectType type;

    public enum StatusEffectType
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
        Weak        //Lowers block and attack damage.
    }
}
#pragma warning restore 0649 