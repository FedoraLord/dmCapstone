using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable 0618
/// <summary>
/// This just holds static references to other Network scripts
/// </summary>
public class NetworkWrapper : MonoBehaviour
{
    public static CustomNetworkDiscovery discovery;
    public static CustomNetworkManager manager;
    public static NetworkWrapper Instance;

    public static bool IsHost { get; private set; }
    public static bool IsClient { get { return !IsHost; } }

    public static Scene currentScene;
    public enum Scene
    {
        MainMenu,
        Battle
    }

    private void Start()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple NetworkWrapper objects exist.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        discovery = GetComponent<CustomNetworkDiscovery>();
        manager = GetComponent<CustomNetworkManager>();
    }

    public static void ConnectToServer(string ipAddress)
    {
        discovery.StopListening();
        manager.networkAddress = ipAddress;
        manager.StartClient();
        IsHost = false;
    }

    public static void OnEnterScene(Scene scene)
    {
        currentScene = scene;
        switch (scene)
        {
            case Scene.MainMenu:
                break;
            case Scene.Battle:
                PersistentPlayer.OnEnterBattleScene();
                break;
            default:
                break;
        }
    }

    public static void StartServer(string roomName)
    {
        discovery.broadcastData = roomName;
        discovery.BroadcastAsServer();
        manager.StartHost();
        IsHost = true;
    }
}