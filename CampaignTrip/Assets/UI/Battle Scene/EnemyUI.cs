using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour
{
    [HideInInspector] public bool claimed;

    [SerializeField] private Image target1;
    [SerializeField] private Image target2;
    [SerializeField] private Image target3;
    [SerializeField] private Image target4;
    [SerializeField] private RectTransform healthBar;
    [SerializeField] private Text healthText;

    private Action healthBarCallback;
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
        SetTargets(new List<CharacterData>());
    }

    public void SetTargets(List<CharacterData> characters)
    {
        for (int i = 0; i < characters.Count; i++)
        {
            Targets[i].sprite = characters[i].icon;
            Targets[i].enabled = true;
        }

        for (int i = characters.Count; i < 4; i++)
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

    public void SetHealth(int newHeath, bool animate = true, Action animCallback = null)
    {
        if (animateHealthBar != null)
        {
            healthBarCallback?.Invoke();
            StopCoroutine(animateHealthBar);
            animateHealthBar = null;
        }

        int prevHealth = health;
        health = Mathf.Clamp(newHeath, 0, maxHealth);

        if (animate)
        {
            animateHealthBar = StartCoroutine(AnimateHealthBar(prevHealth, health));
        }
        else
        {
            UpdateHealthUI(health);
            healthBarCallback?.Invoke();
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
        healthBarCallback?.Invoke();
    }

    private void UpdateHealthUI(float currentHealth)
    {
        healthText.text = currentHealth.ToString("0");

        float percentageHealth = currentHealth / maxHealth;
        Vector3 scale = new Vector3(percentageHealth, 1, 1);
        healthBar.localScale = scale;
    }
}
