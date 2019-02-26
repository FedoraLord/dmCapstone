using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

#pragma warning disable CS0618
/// <summary>
/// This just holds static references to other Network scripts
/// </summary>
public class NetworkWrapper : MonoBehaviour
{
    public static CustomNetworkDiscovery discovery;
    public static CustomNetworkManager manager;
    public static NetworkWrapper Instance;
    public GameObject SpawnerPrefab;

    public static bool IsHost { get { return hosting; } }
    public static bool IsClient { get { return !hosting; } }

    private static bool hosting;

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
        hosting = false;
    }

    public static void StartServer(string roomName)
    {
        discovery.broadcastData = roomName;
        discovery.BroadcastAsServer();
        manager.StartHost();
        hosting = true;
    }
}
#pragma warning restore CS0618