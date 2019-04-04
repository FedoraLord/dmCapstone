using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BattleActorBase;

public class Stat
{
    //can be the person you are healing or the person protecting you
    public BattleActorBase LinkedActor;
    public StatusEffect Type;
    public int HealthOnRemove;
    public int RemainingDuration;
    public int DOT;
    public bool HasDOT;

    public Stat(StatusEffect effect, BattleActorBase linkedActor, int duration, int healthGain = 0)
    {
        Type = effect;
        LinkedActor = linkedActor;
        HealthOnRemove = healthGain;
        RemainingDuration = duration;

        int dot = BattleController.Instance.GetDOT(effect);
        if (dot > -1)
        {
            DOT = dot;
            HasDOT = true;
        }
    }
}