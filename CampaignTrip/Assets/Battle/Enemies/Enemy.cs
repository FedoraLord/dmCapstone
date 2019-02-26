using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Enemy : NetworkBehaviour
{
    public int maxHealth = 100;

    [SyncVar]
	protected int health;

	public bool isAlive { get { return health > 0; } }

    private void Start()
    {
        if (NetworkWrapper.IsHost)
        {
            health = maxHealth;
        }
    }

    private void OnMouseUpAsButton()
	{
		AdjustHealth(-20);
	}

	public void AdjustHealth(int adjustment)
	{
		health += adjustment;
        if (!isAlive)
            Destroy(gameObject);//CmdDie();
	}

	[Command]
	protected void CmdDie()
	{
		BattleController.Instance.CmdTryEndWave();
		gameObject.SetActive(false);
		RpcDie();
	}

	[ClientRpc]
	protected void RpcDie()
	{
		gameObject.SetActive(false);
	}
}
