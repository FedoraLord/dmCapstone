using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityButton : MonoBehaviour
{
    public Button button;
    public Text nameText;
    public Text cooldownText;
    public Image iconImage;

    private float currentFillAmount = 1;

    public void UpdateCooldown(int remainingCooldown, int totalCooldown)
    {
        float r = remainingCooldown;
        float t = totalCooldown;
        float percentage = (t - r) / t;

        cooldownText.text = remainingCooldown.ToString();
        cooldownText.enabled = (remainingCooldown > 0);

        StartCoroutine(AnimateFillAmount(percentage));
    }

    private IEnumerator AnimateFillAmount(float targetValue)
    {
        float animTime = 0;
        float totalAnimTime = 0.5f;
        float startValue = currentFillAmount;

        while (animTime < 1)
        {
            animTime += Time.deltaTime / totalAnimTime;
            currentFillAmount = Mathf.Lerp(startValue, targetValue, animTime);
            iconImage.fillAmount = currentFillAmount;

            yield return new WaitForEndOfFrame();
        }
    }
}
