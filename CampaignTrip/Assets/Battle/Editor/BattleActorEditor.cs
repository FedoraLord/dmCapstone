using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BattleActorBase), true)]
public class BattleActorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Set References"))
        {
            BattleActorBase actor = target as BattleActorBase;
            Animator animator = actor.GetComponent<Animator>();
            DamagePopup damagePopup = actor.transform.Find("DamagePopup").GetComponent<DamagePopup>();
            HealthBarUI healthBarUI = actor.transform.Find("HealthBarUI").GetComponent<HealthBarUI>();
            StatusEffectOverlays overlays = actor.transform.Find("StatusEffectOverlays").GetComponent<StatusEffectOverlays>();
            Transform uit = actor.transform.Find("UITransform");
            GameObject tempAbilityTarget = actor.transform.Find("TempAbilityTarget").gameObject;
            actor.SetReferences(animator, damagePopup, healthBarUI, overlays, uit, tempAbilityTarget);
        }
        base.OnInspectorGUI();
    }
}