using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618, 0649
public class BP_Warrior : BattlePlayerBase
{
    protected override void CustomTargeting()
    {
        if (selectedAbilityIndex == 1)
        {
            customTargets = new List<BattleActorBase>(BattleController.Instance.aliveEnemies);
        }
        else
        {
            throw new System.NotImplementedException();
        }
    }
}
#pragma warning restore CS0618, 0649