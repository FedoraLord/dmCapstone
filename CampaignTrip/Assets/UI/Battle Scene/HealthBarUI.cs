using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649
public class HealthBarUI : BattleActorUI
{
    [HideInInspector] public bool isClaimed;

    [SerializeField] private RectTransform healthBar;
    [SerializeField] private RectTransform blockBar;
    [SerializeField] private Text healthText;

    private bool IsDead { get { return healthData.CurrentValue == 0; } }

    //private Action healthBarCallback;
    //private Coroutine animateHealthBar;
    //private int block;
    //private int health;
    //private int maxBlock;
    //private int maxHealth;
    //private float currentDisplayedHP;

    private BarData healthData;
    private BarData blockData;

    private class BarData
    {
        public int CurrentValue;

        private Coroutine Animation;
        private HealthBarUI Proxy;
        private RectTransform Bar;
        private Text Text;
        private int MaxValue;
        private float DisplayedValue;

        public BarData(HealthBarUI proxy, RectTransform bar, Text text, int maxValue)
        {
            Proxy = proxy;
            Bar = bar;
            Text = text;
            DisplayedValue = MaxValue = CurrentValue = maxValue;
            UpdateValue(maxValue);
        }

        public void Animate(int targetValue)
        {
            if (targetValue != CurrentValue)
            {
                if (Animation != null)
                {
                    Proxy.StopCoroutine(Animation);
                }
                CurrentValue = targetValue;
                Animation = Proxy.StartCoroutine(AnimationRoutine(targetValue));
            }
            
        }

        private void UpdateValue(float value)
        {
            Text.text = value.ToString("0");

            if (MaxValue == 0)
            {
                Bar.localScale = new Vector3(0, 1, 1);
            }
            else
            {
                float percentage = value / MaxValue;
                Vector3 scale = new Vector3(percentage, 1, 1);
                Bar.localScale = scale;
            }
        }

        private IEnumerator AnimationRoutine(int targetValue)
        {
            float animTime = 0;
            float totalAnimTime = 0.25f;
            float startValue = DisplayedValue;

            while (animTime < 1)
            {
                animTime += Time.deltaTime / totalAnimTime;
                DisplayedValue = Mathf.Lerp(startValue, targetValue, animTime);
                UpdateValue(DisplayedValue);

                yield return new WaitForEndOfFrame();
            }

            Animation = null;
        }
    }

    public override void Init(BattleActorBase actor)
    {
        base.Init(actor);

        SetTargets();

        int block = (actor is EnemyBase) ? actor.BlockAmount : 0;
        blockData = new BarData(this, blockBar, healthText, actor.BlockAmount);
        healthData = new BarData(this, healthBar, healthText, actor.MaxHealth);
    }

    public void UpdateValue()
    {
        if (owner is EnemyBase)
        {
            blockData.Animate(owner.BlockAmount);
        }
        healthData.Animate(owner.Health);
    }

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