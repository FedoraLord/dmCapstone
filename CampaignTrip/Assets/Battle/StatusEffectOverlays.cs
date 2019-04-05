using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StatusEffect;

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
	[SerializeField] private ParticleSystem Aggro;

    [SerializeField] private SpriteRenderer Freeze;
    [SerializeField] private SpriteRenderer Protected;
    [SerializeField] private SpriteRenderer Reflect;

    private SpriteRenderer Invisible;
    //Cure;

    private void Start()
    {
        Invisible = transform.parent.GetComponent<SpriteRenderer>();
    }

    public void ToggleOverlay(Stat type, bool active)
    {
        if (type == Stat.Cure)
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
            if (type == Stat.Invisible)
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

    private ParticleSystem GetParticleSystem(Stat type)
    {
        switch (type)
        {
            case Stat.Bleed:
                return Bleed;
            case Stat.Blind:
                return Blind;
            case Stat.Burn:
                return Burn;
            case Stat.Focus:
                return Focus;
            case Stat.Poison:
                return Poison;
            case Stat.Stun:
                return Stun;
            case Stat.Weak:
                return Weak;
            default:
                return null;
        }
    }

    private SpriteRenderer GetSpriteRenderer(Stat type)
    {
        switch (type)
        {
            case Stat.Freeze:
                return Freeze;
            case Stat.Protected:
                return Protected;
            case Stat.Reflect:
                return Reflect;
            case Stat.Invisible:
                return Invisible;
            default:
                return null;
        }
    }

	public void ToggleAggro(bool toggle)
	{
		if (toggle)
			Aggro.Play();
		else
			Aggro.Stop();
	}
}
