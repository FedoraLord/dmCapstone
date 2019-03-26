using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable 0618
public class BP_Alchemist : BattlePlayerBase
{
    [Server]
    protected override void OnAbilityUsed()
    {
        if (selectedAbilityIndex == 2)
        {
            //Molotov Flask (burn all enemies)
            foreach (EnemyBase enemy in BattleController.Instance.aliveEnemies)
            {
                enemy.AddStatusEffect(StatusEffect.Burn, this, SelectedAbility.Duration);
            }
        }
        else
        {
            throw new System.NotImplementedException();
        }
    }

    protected override void OverrideTargeting()
    {
        throw new System.NotImplementedException();
    }
}
