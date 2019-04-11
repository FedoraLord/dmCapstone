using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

#pragma warning disable 0618
public class FlippableCard : NetworkBehaviour
{
	[SyncVar]
	public bool isWinner;

	protected void OnMouseUpAsButton()
	{
		//the player who can flip them is the only one with authority to do so
		if(hasAuthority)
			CmdFlip();
	}

	public bool Flip()
	{
		if (isWinner)
			GetComponent<SpriteRenderer>().sprite = ((CardFlipManager)MinigameManager.Instance).winningSprite;
		else
			GetComponent<SpriteRenderer>().sprite = ((CardFlipManager)MinigameManager.Instance).failSprite;

		return isWinner;
	}

	[Command]
	public void CmdFlip()
	{
		RpcFlip();
        if (Flip())
            MinigameManager.Instance.CmdWin();
        else
            MinigameManager.Instance.CmdLose();
	}

	[ClientRpc]
	public void RpcFlip()
	{
		Flip();
	}

	[TargetRpc]
	public void TargetFlip(NetworkConnection conn)
	{
		Flip();
	}
}
