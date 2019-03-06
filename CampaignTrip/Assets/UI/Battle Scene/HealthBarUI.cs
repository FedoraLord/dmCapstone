using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649
public class HealthBarUI : MonoBehaviour
{
    [HideInInspector] public bool isClaimed;

    [SerializeField] private RectTransform healthBar;
    [SerializeField] private Text healthText;

    private bool IsDead { get { return health == 0; } }

    private Action<bool> healthBarCallback;
    private Coroutine animateHealthBar;
    private int health;
    private int maxHealth;
    private float currentDisplayedHP;

    public void Claim(Vector3 worldPosition, int maxHp, Camera cam)
    {
        isClaimed = true;
        gameObject.SetActive(true);

        Vector3 camPosition = cam.WorldToScreenPoint(worldPosition);
        transform.position = camPosition;

        InitHealth(maxHp);
        OnClaim();
    }

    protected virtual void OnClaim()
    {

    }

    public void Unclaim()
    {
        isClaimed = false;
        gameObject.SetActive(false);
    }

    public void InitHealth(int maxHp)
    {
        health = maxHp;
        maxHealth = maxHp;
        currentDisplayedHP = maxHp;
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