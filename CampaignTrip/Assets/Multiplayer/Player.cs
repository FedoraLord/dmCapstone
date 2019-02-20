using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    public static Player localAuthority;
    public static List<Player> players = new List<Player>();

    [SyncVar]
    public int playerNum;

    public PlayerPanel lobbyPanel;
	public NetworkIdentity networkIdentity;
	public bool isReady;

    private void Start()
    {
        players.Add(this);
        if (NetworkWrapper.IsHost)
        {
            playerNum = players.Count;
        }
		networkIdentity = GetComponent<NetworkIdentity>();
    }
    
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        localAuthority = this;
    }

    private void OnDestroy()
    {
        if (isLocalPlayer)
        {
            localAuthority = null;
        }
        else
        {
            for (int i = playerNum; i < players.Count; i++)
            {
                players[i].lobbyPanel.SetPlayerName(i);
                if (NetworkWrapper.IsHost)
                {
                    players[i].playerNum = i;
                }
            }
        }

        players.Remove(this);
		if(lobbyPanel && lobbyPanel.gameObject)
			Destroy(lobbyPanel.gameObject);
    }

    [Command]
    public void CmdDisconnect()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Command]
    public void CmdUpdatePanel(int characterIndex, bool isReadyNow)
    {
		isReady = isReadyNow;
		RpcUpdatePanel(characterIndex, isReady);
    }

    [ClientRpc]
    public void RpcUpdatePanel(int characterIndex, bool isReadyNow)
    {
		isReady = isReadyNow;
        lobbyPanel.UpdateUI(characterIndex, isReady);
    }
}
