using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

#pragma warning disable 0618
public class CustomNetworkManager : NetworkManager
{
	public string sceneAfterLobbyName;
    
    public override void OnStopClient()
    {
        base.OnStopClient();
        if (NetworkWrapper.currentScene == NetworkWrapper.Scene.Battle)
        {
            BattleController.Instance.ReturnToTitle();
            networkSceneName = string.Empty;
        }
    }

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