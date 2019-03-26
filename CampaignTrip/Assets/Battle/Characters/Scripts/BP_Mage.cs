using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable 0618
public class BP_Mage : BattlePlayerBase
{
    [Server]
    protected override void OnAbilityUsed()
    {
        //throw new System.NotImplementedException();
    }

    protected override void OverrideTargeting()
    {
        throw new System.NotImplementedException();
    }
}
