using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

#pragma warning disable 0618
public class CustomNetworkManager : NetworkManager
{
	public List<string> battleScenes;
    
    public override void OnStopClient()
    {
        base.OnStopClient();
        if (NetworkWrapper.currentScene == NetworkWrapper.Scene.Battle)
        {
            BattleController.Instance.ReturnToTitle();
            networkSceneName = string.Empty;
        }
    }
}