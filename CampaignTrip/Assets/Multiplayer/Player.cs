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
        name += GetComponent<NetworkIdentity>().netId;
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

    [Command]
    public void CmdSpawnPanel()
    {
        lobbyPanelInstance = Instantiate(lobbyPanelPrefab);
        NetworkServer.Spawn(lobbyPanelInstance);
    }

    [Command]
    public void CmdRemovePanel()
    {
        PlayerPanel pp = lobbyPanelInstance.GetComponent<PlayerPanel>();
        TitleUIManager.Instance.roomSessionMenu.RemovePlayerPanel(pp);

        //Destroying an object with a NetworkIdentity component on the server also destroys it on the clients
        Destroy(lobbyPanelInstance);
    }

    [ClientRpc]
    private void RpcRemovePanel()
    {

    }
}
