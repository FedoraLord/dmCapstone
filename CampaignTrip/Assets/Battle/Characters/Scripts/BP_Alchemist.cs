using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static StatusEffect;

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
                enemy.AddStatusEffect(Stat.Burn, this, SelectedAbility.Duration);
            }
        }
    }

    protected override void OverrideTargeting()
    {
        throw new System.NotImplementedException();
    }
}
