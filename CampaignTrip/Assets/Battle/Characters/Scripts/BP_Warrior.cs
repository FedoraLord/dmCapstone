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
        //throw new System.NotImplementedException();
    }

    protected override void OverrideTargeting()
    {
        if (selectedAbilityIndex == 1)
        {
            //Cleave (all enemies)
            customTargets = new List<BattleActorBase>(BattleController.Instance.aliveEnemies);
        }
        else
        {
            throw new System.NotImplementedException();
        }
    }
}
