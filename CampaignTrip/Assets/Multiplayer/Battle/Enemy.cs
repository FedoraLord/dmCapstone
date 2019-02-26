using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Enemy : NetworkBehaviour
{
	[SerializeField][SyncVar]
	protected int health = 100;

	public bool isAlive { get { return health > 0; } }

	private void OnMouseUpAsButton()
	{
		AdjustHealth(-20);
	}

	public void AdjustHealth(int adjustment)
	{
		health += adjustment;
		if (!isAlive)
			CmdDie();
	}

	[Command]
	protected void CmdDie()
	{
		BattleController.instance.CmdTryEndWave();
		gameObject.SetActive(false);
		RpcDie();
	}

	[ClientRpc]
	protected void RpcDie()
	{
		gameObject.SetActive(false);
	}
}
