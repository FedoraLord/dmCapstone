using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect
{
    public static List<Stat> DOTs = new List<Stat>()
    {
        Stat.Bleed,
        Stat.Burn,
        Stat.Poison
    };
    public static List<Stat> Buffs = new List<Stat>()
    {
        Stat.Invisible,
        Stat.Protected,
        Stat.Reflect,
        Stat.Focus,
        Stat.Cure
    };
    public static List<Stat> Debuffs = new List<Stat>()
    {
        Stat.Bleed,
        Stat.Blind,
        Stat.Burn,
        Stat.Freeze,
        Stat.Poison,
        Stat.Stun,
        Stat.Weak
    };
    public static List<Stat> All = new List<Stat>()
    {
        Stat.Bleed,
        Stat.Blind,
        Stat.Burn,
        Stat.Focus,
        Stat.Freeze, 
        Stat.Invisible,
        Stat.Poison,
        Stat.Protected,
        Stat.Reflect,
        Stat.Stun,
        Stat.Weak,
        Stat.Cure
    };

    //can be the person you are healing or the person protecting you
    public BattleActorBase LinkedActor;
    public Stat Type;
    public int HealthOnRemove;
    public int RemainingDuration;

    public bool HasDOT { get { return DOTs.Contains(Type); } }
    public int DOT
    {
        get
        {
            switch (Type)
            {
                case Stat.Bleed:
                    return 5;
                case Stat.Burn:
                    return RemainingDuration * 2;
                case Stat.Poison:
                    return 5;
                default:
                    return 0;
            }
        }
    }
    
    public enum Stat
    {
        None,
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
    
    public StatusEffect(Stat effect, BattleActorBase linkedActor, int duration, int healthGain = 0)
    {
        Type = effect;
        LinkedActor = linkedActor;
        HealthOnRemove = healthGain;
        RemainingDuration = duration;
    }
}