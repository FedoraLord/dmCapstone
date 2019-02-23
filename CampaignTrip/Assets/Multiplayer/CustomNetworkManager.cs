using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618
public class CustomNetworkManager : NetworkManager
{
	public string sceneAfterLobbyName;

    //public override void OnClientConnect(NetworkConnection conn)
    //{
    //    base.OnClientConnect(conn);
    //    TitleUIManager.Instance.roomSessionMenu.ClientConnected(conn);
    //}

    //public override void OnClientDisconnect(NetworkConnection conn)
    //{
    //    base.OnClientDisconnect(conn);
    //    TitleUIManager.Instance.roomSessionMenu.ClientDisconnected(conn);
    //}
}
#pragma warning restore CS0618