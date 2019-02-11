using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    public static Player localAuthorityPlayer;
    public static List<Player> players = new List<Player>();

    [SyncVar]
    public int playerNum;

    public GameObject lobbyPanelPrefab;

    private GameObject lobbyPanelInstance;

    private void Start()
    {
        if (!isLocalPlayer)
        {
            players.Add(this);
            name += " " + playerNum;
        }
    }

    /// <summary>
    /// Called before Start()
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        localAuthorityPlayer = this;
        players.Add(this);
        playerNum = players.Count;
        name += " " + playerNum;
        CmdSpawnPanel();
    }

    public override void OnNetworkDestroy()
    {
        players.Remove(this);
        Destroy(lobbyPanelInstance);
        
        if (isLocalPlayer)
        {
            localAuthorityPlayer = null;
        }

        base.OnNetworkDestroy();
    }

    [Command]
    public void CmdSpawnPanel()
    {
        lobbyPanelInstance = Instantiate(lobbyPanelPrefab);
        if (connectionToServer == null)
        {
            if (connectionToClient == null)
                Debug.LogError("they both null");
            else
                Debug.Log("use connectionToClient");
        }
        else
        {
            Debug.Log("you good");
        }
        NetworkServer.Spawn(lobbyPanelInstance);
    }
    
    //lobbyPanelInstance.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);

    [Command]
    public void CmdDisconnect()
    {
        NetworkServer.Destroy(gameObject);
        //PlayerPanel pp = lobbyPanelInstance.GetComponent<PlayerPanel>();
        //TitleUIManager.Instance.roomSessionMenu.RemovePlayerPanel(pp);
        //Destroy(lobbyPanelInstance);
        //RpcRemovePanel();
    }

    [ClientRpc]
    private void RpcRemovePanel()
    {
        PlayerPanel pp = lobbyPanelInstance.GetComponent<PlayerPanel>();
        TitleUIManager.Instance.roomSessionMenu.RemovePlayerPanel(pp);

        if (lobbyPanelInstance != null)
        {
            Destroy(lobbyPanelInstance);
        }
    }
}
