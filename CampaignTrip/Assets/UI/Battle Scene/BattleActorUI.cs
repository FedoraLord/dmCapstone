using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BattleActorUI : MonoBehaviour
{
    public BattleActorBase owner;

    public virtual void Init(BattleActorBase actor)
    {
        owner = actor;
        transform.SetParent(BattleController.Instance.healthAndDamage.transform);
        UpdatePosition();
        (transform as RectTransform).sizeDelta = Vector2.zero;
    }

    public void UpdatePosition()
    {
        Vector3 camPosition = BattleController.Instance.MainCamera.WorldToScreenPoint(owner.UITransform.position);
        transform.position = camPosition;
    }
}
