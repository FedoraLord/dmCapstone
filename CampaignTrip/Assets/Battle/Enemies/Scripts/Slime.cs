using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable 0618
public class Slime : EnemyBase
{
	public List<Animator> smallSlimes;

    private bool isSplit;

    protected override void Initialize()
    {
        base.Initialize();
        if (battleStats.AttacksPerTurn > 1)
        {
            isSplit = true;
            GetComponent<SpriteRenderer>().enabled = false;
            for (int i = 0; i < smallSlimes.Count && i < battleStats.AttacksPerTurn; i++)
            {
                smallSlimes[i].gameObject.SetActive(true);
            }
        }
    }
    
    public override void PlayAnimation(BattleAnimation type)
    {
        if (!isSplit)
        {
            base.PlayAnimation(type);
            return;
        }

		string trigger = "";
		switch (type)
		{
			case BattleAnimation.Attack:
				trigger = "Attack";
				break;
			case BattleAnimation.Hurt:
				trigger = "Hurt";
				break;
			case BattleAnimation.Die:
				trigger = "Die";
				break;
		}

        StartCoroutine(DelayAnimationTrigger(trigger));
	}

    private IEnumerator DelayAnimationTrigger(string trigger)
    {
        List<Animator> visible = smallSlimes.Where(x => x.isActiveAndEnabled).ToList();
        while (visible.Count > 0 && IsAlive)
        {
            Animator rand = visible.Random();
            rand.SetTrigger(trigger);
            visible.Remove(rand);
            yield return new WaitForSeconds(0.1f);
        }
    }
}
