using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BattleActorUI : MonoBehaviour
{
    public BattleActorBase owner;

    public virtual void Init(BattleActorBase actor)
    {
        owner = actor;
        transform.SetParent(BattleController.Instance.transform);
        Vector3 camPosition = BattleController.Instance.MainCamera.WorldToScreenPoint(actor.UITransform.position);
        transform.position = camPosition;
        (transform as RectTransform).sizeDelta = Vector2.zero;
    }
}
