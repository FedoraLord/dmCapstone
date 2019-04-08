using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649
public class HealthBarUI : BattleActorUI
{
    [HideInInspector] public bool isClaimed;

    [SerializeField] private Image target1;
    [SerializeField] private Image target2;
    [SerializeField] private Image target3;
    [SerializeField] private Image target4;
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private RectTransform blockBar;
    [SerializeField] private Text healthText;

    private bool IsDead { get { return healthData.CurrentValue == 0; } }
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

    private List<Image> targets;
    private BarData healthData;
    private BarData blockData;
    
    public override void Init(BattleActorBase actor)
    {
        base.Init(actor);

        SetTargets();

        int block = (actor is EnemyBase) ? actor.BattleStats.BlockAmount : 0;
        if (actor is EnemyBase)
        {
            int max = Mathf.Max(actor.BattleStats.BlockAmount, actor.BattleStats.MaxHealth);
            blockData = new BarData(this, blockBar, null, max, actor.BattleStats.BlockAmount);
            healthData = new BarData(this, healthBar, healthText, max, actor.BattleStats.MaxHealth);
        }
        else
        {
            blockData = new BarData(this, blockBar, null, 0, actor.BattleStats.BlockAmount);
            healthData = new BarData(this, healthBar, healthText, actor.BattleStats.MaxHealth, actor.BattleStats.MaxHealth);
        }
    }

    public void UpdateBlock()
    {
        EnemyBase enemy = owner as EnemyBase;
        if (enemy != null)
        {
            blockData.Animate(enemy.RemainingBlock);
        }
    }

    public void UpdateHealth()
    {
        healthData.Animate(owner.Health);
    }
    
    public void SetTargets(int[] playerTargets = null)
    {
        if (playerTargets == null)
            playerTargets = new int[0];

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