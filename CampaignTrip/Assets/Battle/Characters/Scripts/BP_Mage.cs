using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable 0618, 0649
public class BP_Mage : BattlePlayerBase
{
    [SerializeField] private int damageBeforeFocusBreaks;

    private int damageWhileFocused;

    [Server]
    protected override void OnAbilityUsed() { }

    protected override void OverrideTargeting()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnAddFocus()
    {
        base.OnAddFocus();
        damageWhileFocused = 0;
    }

    [Server]
    public override int TakeBlockedDamage(int damage)
    {
        int damageTaken = base.TakeBlockedDamage(damage);
        if (HasStatusEffect(StatusEffect.Focus))
        {
            damageWhileFocused += damageTaken;
            if (damageWhileFocused >= damageBeforeFocusBreaks)
            {
                RemoveStatusEffect(StatusEffect.Focus);
            }
        }
        return damageTaken;
    }
}
