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
		StartCoroutine(SpawnWhenCan());
	}

	private IEnumerator SpawnWhenCan()
	{
		yield return new WaitUntil(() => networkIdentity);
		CharacterCreator.instance.CmdSpawnCharacter(networkIdentity.connectionToServer.connectionId);
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
		if (isReadyNow)
		{
			isReady = isReadyNow;
			TryStart(characterIndex);
		}
		RpcUpdatePanel(characterIndex, isReady);
    }

    [ClientRpc]
    public void RpcUpdatePanel(int characterIndex, bool isReadyNow)
    {
		isReady = isReadyNow;
        lobbyPanel.UpdateUI(characterIndex, isReady);
	}

	private void TryStart(int characterIndex)
	{
		foreach (Player p in Player.players)
			if (!p.isReady)
				return; //someones not ready so don't start
		NetworkWrapper.manager.ServerChangeScene(NetworkWrapper.manager.sceneAfterLobbyName);
		RpcRelayStart(characterIndex);
	}

	[ClientRpc]
	private void RpcRelayStart(int characterIndex)
	{
		//give our characterData to the manager
		CharacterCreator.instance.chosenCharacters.Add(Player.localAuthority.networkIdentity.connectionToServer.connectionId, TitleUIManager.RoomSessionMenu.characters[characterIndex]);
		//Let unity know were ready to change scenes
		ClientScene.Ready(Player.localAuthority.networkIdentity.connectionToServer);
	}

	#endregion
}
