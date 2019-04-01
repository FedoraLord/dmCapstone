using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable 0618
public class BP_Warrior : BattlePlayerBase
{
    [Server]
    protected override void OnAbilityUsed()
    {
        
    }

    protected override void OverrideTargeting()
    {
        throw new System.NotImplementedException();
    }
}
