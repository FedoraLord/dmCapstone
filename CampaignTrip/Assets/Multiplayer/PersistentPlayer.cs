﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

#pragma warning disable CS0618, 0649
public class PersistentPlayer : NetworkBehaviour
{
    public static PersistentPlayer localAuthority;
    public static List<PersistentPlayer> players = new List<PersistentPlayer>();

    [SyncVar]
    public int playerNum;

	public bool isReady;
    public int minigameReady;

    [HideInInspector] public BattlePlayerBase battlePlayer;
    [HideInInspector] public PlayerPanel lobbyPanel;
    [HideInInspector] public CharacterData character;

    [SerializeField] private NetworkIdentity networkIdentity;

    private bool gameplayInitialized;

    #region InitAndDestroy

    private void Start()
    {
		DontDestroyOnLoad(gameObject);
        players.Add(this);
        if (NetworkWrapper.IsHost)
        {
            playerNum = players.Count;
            if (playerNum == 4)
            {
                NetworkWrapper.discovery.BroadcastClosedRoom();
            }
        }
        else if (players.Count > 4)
        {
            NetworkWrapper.manager.StopClient();
            SceneManager.LoadScene("Title");
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
                if (NetworkWrapper.currentScene == NetworkWrapper.Scene.MainMenu)
                    players[i].lobbyPanel.SetPlayerName(i);
                if (NetworkWrapper.IsHost)
                    players[i].playerNum = i;
            }
        }

        if (NetworkWrapper.currentScene == NetworkWrapper.Scene.MainMenu && players.Count == 4)
            NetworkWrapper.discovery.BroadcastOpenRoom();

        players.Remove(this);
        if (lobbyPanel != null)
            Destroy(lobbyPanel.gameObject);
    }

    [Command]
    public void CmdDisconnect()
    {
        NetworkServer.Destroy(gameObject);
    }

    #endregion

    #region Lobby

    [Command]
    public void CmdUpdatePanel(int characterIndex, bool isReadyNow)
	{
		isReady = isReadyNow;
		RpcUpdatePanel(characterIndex, isReadyNow);
		TryStart();
    }

    [ClientRpc]
    public void RpcUpdatePanel(int characterIndex, bool isReadyNow)
    {
        character = TitleUIManager.RoomSessionMenu.characters[characterIndex];
		isReady = isReadyNow;
        lobbyPanel.UpdateUI(characterIndex, isReady);
	}

	private void TryStart()
	{
		foreach (PersistentPlayer p in players)
        {
			if (!p.isReady)
				return; //someones not ready so don't start
        }
        
        NetworkWrapper.manager.ServerChangeScene(NetworkWrapper.manager.battleScenes.Random()); 
        NetworkWrapper.discovery.StopBroadcast();
        RpcRelayStart();
	}

	[ClientRpc]
	private void RpcRelayStart()
	{
		ClientScene.Ready(localAuthority.networkIdentity.connectionToServer);
	}

    #endregion

    #region Battle

    public static void OnEnterBattleScene()
    {
        
    }

    [Command]
    public void CmdSpawnBattlePlayer()
    {
        BattleController.Instance.SpawnPlayer(this);
    }

    #endregion



    [Command]
    public void CmdReadyForMinigame()
    {
        localAuthority.minigameReady++;
    }
}