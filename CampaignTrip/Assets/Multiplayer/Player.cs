using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Player : NetworkBehaviour
{
    public static Player localAuthority;
    public static List<Player> players = new List<Player>();

    [SyncVar]
    public int playerNum;

    public PlayerPanel lobbyPanel;
	public NetworkIdentity networkIdentity;
	public GameObject characterObject;
	protected GameObject characterPrefab;
	public bool isReady;

    private void Start()
    {
		DontDestroyOnLoad(gameObject);
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

	private void OnLevelWasLoaded(int level)
	{
		if (isLocalPlayer && SceneManager.GetActiveScene().name == "mainBattleScene")
			CmdSpawnCharacter();
	}

	[Command]
	private void CmdSpawnCharacter()
	{
		characterObject = Instantiate(characterPrefab);
		NetworkServer.Spawn(characterObject);
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

	#region Lobby

	[Command]
    public void CmdUpdatePanel(int characterIndex, bool isReadyNow)
	{
		characterPrefab = TitleUIManager.RoomSessionMenu.characters[characterIndex].characterPrefab;
		isReady = isReadyNow;
		RpcUpdatePanel(characterIndex, isReadyNow);
		TryStart();
    }

    [ClientRpc]
    public void RpcUpdatePanel(int characterIndex, bool isReadyNow)
    {
		characterPrefab = TitleUIManager.RoomSessionMenu.characters[characterIndex].characterPrefab;
		isReady = isReadyNow;
        lobbyPanel.UpdateUI(characterIndex, isReady);
	}

	private void TryStart()
	{
		foreach (Player p in Player.players)
			if (!p.isReady)
				return; //someones not ready so don't start
		NetworkWrapper.manager.ServerChangeScene(NetworkWrapper.manager.sceneAfterLobbyName);
		RpcRelayStart();
	}

	[ClientRpc]
	private void RpcRelayStart()
	{
		ClientScene.Ready(Player.localAuthority.networkIdentity.connectionToServer);
	}

	#endregion
}
