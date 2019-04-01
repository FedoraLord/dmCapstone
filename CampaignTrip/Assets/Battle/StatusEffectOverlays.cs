using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BattleActorBase;

#pragma warning disable 0649
public class StatusEffectOverlays : MonoBehaviour
{
    [SerializeField] private ParticleSystem Bleed;
    [SerializeField] private ParticleSystem Blind;
    [SerializeField] private ParticleSystem Burn;
    [SerializeField] private ParticleSystem Focus;
    [SerializeField] private ParticleSystem Poison;
    [SerializeField] private ParticleSystem Stun;
    [SerializeField] private ParticleSystem Weak;

    [SerializeField] private SpriteRenderer Freeze;
    [SerializeField] private SpriteRenderer Protected;
    [SerializeField] private SpriteRenderer Reflect;

    private SpriteRenderer Invisible;
    //Cure;

    private void Start()
    {
        Invisible = transform.parent.GetComponent<SpriteRenderer>();
    }

    public void ToggleOverlay(StatusEffect type, bool active)
    {
        if (type == StatusEffect.Cure)
            return;

        ParticleSystem ps = GetParticleSystem(type);
        if (ps != null)
        {
            if (active)
                ps.Play();
            else
                ps.Stop();
            return;
        }

        SpriteRenderer sr = GetSpriteRenderer(type);
        if (sr != null)
        {
            if (type == StatusEffect.Invisible)
            {
                Color c = sr.color;
                c.a = (active ? 0.5f : 1);
                sr.color = c;
            }
            else
            {
                sr.enabled = active;
            }
            return;
        }

        Debug.LogFormat("Failed to toggle overlay for system {0}", type);
    }

    private ParticleSystem GetParticleSystem(StatusEffect type)
    {
        switch (type)
        {
            case StatusEffect.Bleed:
                return Bleed;
            case StatusEffect.Blind:
                return Blind;
            case StatusEffect.Burn:
                return Burn;
            case StatusEffect.Focus:
                return Focus;
            case StatusEffect.Poison:
                return Poison;
            case StatusEffect.Stun:
                return Stun;
            case StatusEffect.Weak:
                return Weak;
            default:
                return null;
        }
    }

    private SpriteRenderer GetSpriteRenderer(StatusEffect type)
    {
        switch (type)
        {
            case StatusEffect.Freeze:
                return Freeze;
            case StatusEffect.Protected:
                return Protected;
            case StatusEffect.Reflect:
                return Reflect;
            case StatusEffect.Invisible:
                return Invisible;
            default:
                return null;
        }
    }
}
