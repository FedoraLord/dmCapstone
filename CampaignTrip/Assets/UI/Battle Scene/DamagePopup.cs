using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamagePopup : BattleActorUI
{
    public Text damageText;
    public Text blockText;
    public Text messageText;
    public Text dotText;
    
    private Camera mainCamera;
    private Coroutine damageAnimation;
    private Coroutine blockAnimation;
    private Coroutine messageAnimation;
    private Coroutine dotAnimation;
    private int animationsPlaying;

    private void Start()
    {
        damageText.gameObject.SetActive(false);
        blockText.gameObject.SetActive(false);
        messageText.gameObject.SetActive(false);
        dotText.gameObject.SetActive(false);
    }

    public override void Init(BattleActorBase actor)
    {
        base.Init(actor);
        mainCamera = Camera.main;
        dotText.transform.position = mainCamera.WorldToScreenPoint(actor.transform.position);
    }

    public void DisplayDamage(int damage, int blocked)
    {
        StopAnimation(damageAnimation);
        StopAnimation(blockAnimation);

        blockText.enabled = (blocked > 0);
        blockText.text = string.Format(" ({0})", blocked);
        damageText.enabled = (damage > 0);
        damageText.text = string.Format("-{0}", damage);

        damageAnimation = StartCoroutine(AnimatePopup(damageText, true, () => damageAnimation = null));
        blockAnimation = StartCoroutine(AnimatePopup(blockText, true, () => blockAnimation = null));
    }

    public void DisplayMiss()
    {
        DisplayMessage("Miss");
    }

    public void DisplayInterrupt()
    {
        DisplayMessage("Interrupt");
    }

    public void DisplayMessage(string msg)
    {
        DisplayMessage(msg, Color.gray);
    }

    public void DisplayMessage(string msg, Color color)
    {
        messageText.CrossFadeColor(color, 0, false, false);
        messageText.text = msg;
        StopAnimation(messageAnimation);
        messageAnimation = StartCoroutine(AnimatePopup(messageText, false, () => messageAnimation = null));
    }

    public void DisplayStat(Color color, int remainingDuration, string prefix = "")
    {
        remainingDuration = Mathf.Max(remainingDuration, 0);

        dotText.color = color;
        dotText.text = prefix + remainingDuration.ToString();
        dotText.gameObject.SetActive(true);

        StopAnimation(dotAnimation);
        dotAnimation = StartCoroutine(AnimateStat(() => dotAnimation = null));
    }

    private IEnumerator AnimateStat(Action callback)
    {
        dotText.CrossFadeAlpha(1, 0, false);
        yield return new WaitForSeconds(0.5f);

        dotText.CrossFadeAlpha(0, 0.2f, false);
        yield return new WaitForSeconds(0.2f);

        dotText.gameObject.SetActive(false);
        callback();
    }

    private void StopAnimation(Coroutine animation)
    {
        if (animation != null)
        {
            StopCoroutine(animation);
            animationsPlaying--;
        }
    }

    private IEnumerator AnimatePopup(Text text, bool allowMultiple, Action callback = null)
    {
        if (!allowMultiple)
        {
            yield return new WaitUntil(() => animationsPlaying == 0);
        }

        animationsPlaying++;
        text.gameObject.SetActive(true);

        float animTime = 0;
        float totalAnimTime = 0.3f;
        Vector3 start = mainCamera.WorldToScreenPoint(owner.UITransform.position);
        Vector3 end = mainCamera.WorldToScreenPoint(mainCamera.ScreenToWorldPoint(start) + Vector3.up * 0.3f);

        text.CrossFadeAlpha(0, 0, false);
        yield return new WaitForEndOfFrame();
        text.CrossFadeAlpha(1, 0.2f, false);

        while (animTime < 1)
        {
            text.transform.position = Vector3.Lerp(start, end, animTime);
            yield return new WaitForEndOfFrame();
            animTime += Time.deltaTime / totalAnimTime;
        }

        yield return new WaitForSeconds(0.3f);

        float fadeOutTime = 1f;
        text.CrossFadeAlpha(0, fadeOutTime, false);

        yield return new WaitForSeconds(fadeOutTime);

        animationsPlaying--;
        callback?.Invoke();
        text.gameObject.SetActive(false);
    }
}
