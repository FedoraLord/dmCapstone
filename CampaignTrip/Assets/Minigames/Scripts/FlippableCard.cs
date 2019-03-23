using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class FlippableCard : NetworkBehaviour
{
	public bool isWinner;

	protected void OnMouseUpAsButton()
	{
		if (((CardFlipManager)MinigameManager.Instance).CanFlip)
			CmdFlip();
	}

	public bool Flip()
	{
		if (isWinner)
			GetComponent<Image>().sprite = ((CardFlipManager)MinigameManager.Instance).winningSprite;
		else
			GetComponent<Image>().sprite = ((CardFlipManager)MinigameManager.Instance).failSprite;

		return isWinner;
	}

	[Command]
	public void CmdFlip()
	{
		RpcFlip();
		if (Flip())
			MinigameManager.Instance.CmdWin();
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
