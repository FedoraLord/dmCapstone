using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatusEffect;

#pragma warning disable 0649
[Serializable]
public class Ability
{
    public AbilityButton AButton { get; set; }
    public bool TargetAll { get { return targetAll; } }
    public int Damage { get { return damage; } }
    public int Duration { get { return duration; } }
    public TargetGroup Targets { get { return targetGroup; } }
    public Stat Applies { get { return applies; } }

    public enum TargetGroup { Override, Ally, Self, AllyAndSelf, Enemy }

    [HideInInspector] public int RemainingCooldown;

    [SerializeField] private string abilityName;
    [SerializeField] private int damage;
    [SerializeField] private int duration;
    [SerializeField] private int cooldown;
    [SerializeField] private bool targetAll;
    [SerializeField] private TargetGroup targetGroup;
    [SerializeField] private Stat applies;
    [SerializeField] private Sprite buttonIcon;

    //TODO:
    [HideInInspector] public bool IsUpgraded;

    public void SetButton(AbilityButton button)
    {
        AButton = button;
        AButton.nameText.text = abilityName;
        if (buttonIcon != null)
        {
            AButton.iconImage.sprite = buttonIcon;
        }
    }

    public void Use()
    {
        BattlePlayerBase.LocalAuthority.CanPlayAbility = false;
        RemainingCooldown = cooldown + 1;
        UpdateButtonUI();
    }

    public void DecrementCooldown()
    {
        if (RemainingCooldown > 0)
            RemainingCooldown--;
        UpdateButtonUI();
    }

    public void UpdateButtonUI()
    {
        if (AButton == null)
            return;
        BattlePlayerBase player = BattlePlayerBase.LocalAuthority;
        AButton.button.interactable = (player.IsAlive && player.CanPlayAbility && RemainingCooldown <= 0);
        AButton.UpdateCooldown(RemainingCooldown, cooldown + 1);
    }
}