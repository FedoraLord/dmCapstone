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

    private void Start()
    {
        players.Add(this);
        if (NetworkWrapper.IsHost)
        {
            playerNum = players.Count;
        }
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
        Destroy(lobbyPanel.gameObject);
    }

    [Command]
    public void CmdDisconnect()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Command]
    public void CmdUpdatePanel(int characterIndex, bool isReady)
    {
        RpcUpdatePanel(characterIndex, isReady);
    }

    [ClientRpc]
    public void RpcUpdatePanel(int characterIndex, bool isReady)
    {
        lobbyPanel.UpdateUI(characterIndex, isReady);
    }
}
