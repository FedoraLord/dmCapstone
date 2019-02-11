using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    public static Player localAuthorityPlayer;

    public GameObject lobbyPanelPrefab;

    private GameObject lobbyPanelInstance;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        localAuthorityPlayer = this;
        CmdSpawnPanel();
    }

    public override void OnNetworkDestroy()
    {
        base.OnNetworkDestroy();

        if (isLocalPlayer)
        {
            //idk if this is necessary
            localAuthorityPlayer = null;
        }
    }

    //this doesnt update fast enough because it isnt called for a few seconds when the connection times out. implement the two methods below
    //public override void OnNetworkDestroy()
    //{
    //    base.OnNetworkDestroy();
    //    PlayerPanel pp = lobbyPanelInstance.GetComponent<PlayerPanel>();
    //    TitleUIManager.Instance.roomSessionMenu.RemovePlayerPanel(pp);
    //    Destroy(lobbyPanelInstance);
    //}

    [Command]
    public void CmdSpawnPanel()
    {
        lobbyPanelInstance = Instantiate(lobbyPanelPrefab);
        NetworkServer.Spawn(lobbyPanelInstance);
    }

    //call this method on a client when the client leaves the lobby and it will run on the server
    [Command]
    public void CmdRemovePanel()
    {
        PlayerPanel pp = lobbyPanelInstance.GetComponent<PlayerPanel>();
        TitleUIManager.Instance.roomSessionMenu.RemovePlayerPanel(pp);
        Destroy(lobbyPanelInstance);
        RpcRemovePanel();
    }

    //call this method on the server and it will run on all the clients
    [ClientRpc]
    public void RpcRemovePanel()
    {
        PlayerPanel pp = lobbyPanelInstance.GetComponent<PlayerPanel>();
        TitleUIManager.Instance.roomSessionMenu.RemovePlayerPanel(pp);
        Destroy(lobbyPanelInstance);
    }
}
