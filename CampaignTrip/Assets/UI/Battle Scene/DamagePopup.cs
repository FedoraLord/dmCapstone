using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamagePopup : BattleActorUI
{
    public Text damageText;
    public Text blockText;
    public Text missText;
    
    private Camera mainCamera;
    private Coroutine damageAnimation;
    private Coroutine blockAnimation;
    private Coroutine missAnimation;

    private void Start()
    {
        damageText.gameObject.SetActive(false);
        blockText.gameObject.SetActive(false);
        missText.gameObject.SetActive(false);
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    public override void Init(BattleActorBase actor)
    {
        base.Init(actor);
    }

    public void DisplayDamage(int damage, int blocked)
    {
        StopAnimation(damageAnimation);
        StopAnimation(blockAnimation);

        blockText.enabled = (blocked > 0);
        blockText.text = string.Format(" ({0})", blocked);
        damageText.enabled = (damage > 0);
        damageText.text = string.Format("-{0}", damage);

        damageAnimation = StartCoroutine(Animate(damageText, () => damageAnimation = null));
        blockAnimation = StartCoroutine(Animate(blockText, () => blockAnimation = null));
    }

    public void DisplayMiss()
    {
        StopAnimation(missAnimation);
        missAnimation = StartCoroutine(Animate(missText, () => missAnimation = null));
    }

    private void StopAnimation(Coroutine animation)
    {
        if (animation != null)
            StopCoroutine(animation);
    }

    private IEnumerator Animate(Text text, Action callback = null)
    {
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

        callback?.Invoke();
        text.gameObject.SetActive(false);
    }
}
