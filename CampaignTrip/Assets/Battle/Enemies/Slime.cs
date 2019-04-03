using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Slime : EnemyBase
{

	public List<GameObject> smallSlimes;

    public override void OnMinigameFailed()
    {
		RpcSplit();
    }

	[ClientRpc]
	private void RpcSplit()
	{
		attacksPerTurn = 4;
		GetComponent<SpriteRenderer>().enabled = false;
		foreach (GameObject smallSlime in smallSlimes)
			smallSlime.GetComponent<SpriteRenderer>().enabled = true;
	}

    public override void PlayAnimation(BattleAnimation type)
    {
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
				//need the real art before we can implement this
				break;
		}
		foreach (GameObject smallSlime in smallSlimes)
			smallSlime.GetComponent<Animator>().SetTrigger(trigger);
	}
}
