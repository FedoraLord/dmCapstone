using System;
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

    public enum Character { Warrior, Rogue, Alchemist, Mage };
    public Character characterType;

    [SyncVar]
    public int playerNum;

	public bool isReady;

    [HideInInspector] public BattlePlayerBase battlePlayer;
    [HideInInspector] public PlayerPanel lobbyPanel;
    [HideInInspector] public CharacterData character;

    [SerializeField] private NetworkIdentity networkIdentity;

    private bool gameplayInitialized;
    private GameObject characterPrefab;

    #region InitAndDestroy

    private void Start()
    {
		DontDestroyOnLoad(gameObject);
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

    private void InitGameplay()
    {
        if (gameplayInitialized)
            return;

        gameplayInitialized = true;
        GameObject obj = Instantiate(NetworkWrapper.Instance.spawnerPrefab);
        NetworkServer.Spawn(obj);
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

        if (NetworkWrapper.currentScene == NetworkWrapper.Scene.MainMenu)
        {
            players.Remove(this);
            Destroy(lobbyPanel.gameObject);
        }
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
		characterPrefab = TitleUIManager.RoomSessionMenu.characters[characterIndex].CharacterPrefab;
		isReady = isReadyNow;
		RpcUpdatePanel(characterIndex, isReadyNow);
		TryStart();
    }

    [ClientRpc]
    public void RpcUpdatePanel(int characterIndex, bool isReadyNow)
    {
        character = TitleUIManager.RoomSessionMenu.characters[characterIndex];
        characterPrefab = character.CharacterPrefab;
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

        InitGameplay();
        NetworkWrapper.manager.ServerChangeScene(NetworkWrapper.manager.sceneAfterLobbyName);
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
        GameObject cp = Instantiate(characterPrefab);
        cp.GetComponent<BattlePlayerBase>().playerNum = playerNum;
        NetworkServer.SpawnWithClientAuthority(cp, gameObject);
        BattleController.Instance.OnPlayerReady();
    }

    #endregion
}
#pragma warning restore CS0618, 0649