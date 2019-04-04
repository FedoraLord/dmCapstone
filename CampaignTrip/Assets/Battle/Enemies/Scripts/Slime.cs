using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Slime : EnemyBase
{
	public List<Animator> smallSlimes;

    private bool isSplit;

    public override void OnMinigameFailed()
    {
		RpcSplit();
    }

	[ClientRpc]
	private void RpcSplit()
	{
		attacksPerTurn = 4;
        isSplit = true;
		GetComponent<SpriteRenderer>().enabled = false;

        foreach (Animator smallSlime in smallSlimes)
        {
            smallSlime.gameObject.SetActive(true);
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

        foreach (Animator smallSlime in smallSlimes)
        {
            smallSlime.SetTrigger(trigger);
        }
	}
}
