using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649
public class EnemyUI : MonoBehaviour
{
    [HideInInspector] public bool claimed;

    [SerializeField] private Image target1;
    [SerializeField] private Image target2;
    [SerializeField] private Image target3;
    [SerializeField] private Image target4;
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private Text healthText;

    private bool IsDead { get { return health == 0; } }

    private Action<bool> healthBarCallback;
    private Coroutine animateHealthBar;
    private int health;
    private int maxHealth;
    private float currentDisplayedHP;

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
    
    public void Claim(Enemy enemy)
    {
        claimed = true;
        gameObject.SetActive(true);

        Vector3 camPosition = BattleController.Instance.cam.WorldToScreenPoint(enemy.uiTransform.position);
        transform.position = camPosition;

        InitHealth(enemy.health);
        SetTargets(new int[0]);
    }

    public void Unclaim()
    {
        claimed = false;
        gameObject.SetActive(false);
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

    public void InitHealth(int totalHealth)
    {
        health = totalHealth;
        maxHealth = totalHealth;
        currentDisplayedHP = totalHealth;
        UpdateHealthUI(health);
    }

    public void SetHealth(int newHeath, Action<bool> animCallback = null, bool animate = true)
    {
        if (animateHealthBar != null)
        {
            healthBarCallback?.Invoke(IsDead);
            StopCoroutine(animateHealthBar);
            animateHealthBar = null;
        }

        healthBarCallback = animCallback;
        int prevHealth = health;
        health = Mathf.Clamp(newHeath, 0, maxHealth);

        if (animate)
        {
            animateHealthBar = StartCoroutine(AnimateHealthBar(prevHealth, health));
        }
        else
        {
            UpdateHealthUI(health);
            healthBarCallback?.Invoke(IsDead);
        }
    }

    private IEnumerator AnimateHealthBar(int startHealth, int endHealth)
    {
        float animTime = 0;
        float totalAnimTime = 0.5f;

        while (animTime < 1)
        {
            animTime += Time.deltaTime / totalAnimTime;
            currentDisplayedHP = Mathf.Lerp(currentDisplayedHP, endHealth, animTime);
            UpdateHealthUI(currentDisplayedHP);

            yield return new WaitForEndOfFrame();
        }

        animateHealthBar = null;
        healthBarCallback?.Invoke(IsDead);
        healthBarCallback = null;
    }

    private void UpdateHealthUI(float currentHealth)
    {
        healthText.text = currentHealth.ToString("0");

        float percentageHealth = currentHealth / maxHealth;
        Vector3 scale = new Vector3(percentageHealth, 1, 1);
        healthBar.localScale = scale;
    }
}
#pragma warning restore 0649