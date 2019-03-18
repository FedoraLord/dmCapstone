using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamagePopup : BattleActorUI
{
    public Text damageText;
    public Text blockText;
    
    private Camera mainCamera;
    private Coroutine popupAnimation;

    public override void Init(BattleActorBase actor)
    {
        base.Init(actor);
        gameObject.SetActive(false);
    }

    public void Display(int damage, int blocked)
    {
        if (popupAnimation != null)
            StopCoroutine(popupAnimation);

        blockText.enabled = (blocked > 0);
        blockText.text = string.Format("({0})", blocked);
        damageText.enabled = (damage > 0);
        damageText.text = string.Format("-{0}", damage);

        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        transform.position = mainCamera.WorldToScreenPoint(owner.UITransform.position);

        gameObject.SetActive(true);
        popupAnimation = StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        float animTime = 0;
        float totalAnimTime = 0.3f;
        Vector3 start = transform.position;
        Vector3 end = mainCamera.WorldToScreenPoint(mainCamera.ScreenToWorldPoint(transform.position) + Vector3.up * 0.3f);

        damageText.CrossFadeAlpha(0, 0, false);
        blockText.CrossFadeAlpha(0, 0, false);
        yield return new WaitForEndOfFrame();
        damageText.CrossFadeAlpha(1, 0.2f, false);
        blockText.CrossFadeAlpha(1, 0.2f, false);

        while (animTime < 1)
        {
            transform.position = Vector3.Lerp(start, end, animTime);
            yield return new WaitForEndOfFrame();
            animTime += Time.deltaTime / totalAnimTime;
        }

        yield return new WaitForSeconds(0.3f);

        float fadeOutTime = 1f;
        damageText.CrossFadeAlpha(0, fadeOutTime, false);
        blockText.CrossFadeAlpha(0, fadeOutTime, false);

        yield return new WaitForSeconds(fadeOutTime);
        popupAnimation = null;
        gameObject.SetActive(true);
    }
}
