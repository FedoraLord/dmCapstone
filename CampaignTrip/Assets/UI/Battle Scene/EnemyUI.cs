using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649
public class EnemyUI : HealthBarUI
{
    [SerializeField] private Image target1;
    [SerializeField] private Image target2;
    [SerializeField] private Image target3;
    [SerializeField] private Image target4;

    private List<Image> targets;
    private List<Image> Targets
    {
        get
        {
            if (targets == null)
            {
                targets = new List<Image>()
                {
                    target1, target2, target3, target4
                };
            }
            return targets;
        }
    }

    protected override void OnClaim()
    {
        SetTargets(new int[0]);
    }

    public void SetTargets(int[] playerTargets)
    {
        for (int i = 0; i < playerTargets.Length; i++)
        {
            int playerIndex = playerTargets[i];
            CharacterData character = PersistentPlayer.players[playerIndex].character;
            Targets[i].sprite = character.Icon;
            Targets[i].enabled = true;
        }
        
        for (int i = playerTargets.Length; i < 4; i++)
        {
            Targets[i].enabled = false;
        }
    }
}
#pragma warning restore 0649