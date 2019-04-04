using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarData
{
    public int CurrentValue;

    private Coroutine Animation;
    private HealthBarUI Proxy;
    private RectTransform Bar;
    private Text Text;
    private int MaxValue;
    private float DisplayedValue;

    public BarData(HealthBarUI proxy, RectTransform bar, Text text, int maxValue, int startingValue)
    {
        Proxy = proxy;
        Bar = bar;
        Text = text;
        MaxValue = maxValue;
        DisplayedValue = CurrentValue = startingValue;
        UpdateValue(startingValue);
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
        if (Text != null)
        {
            Text.text = value.ToString("0");
        }

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