using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

#pragma warning disable CS0618
/// <summary>
/// The script for discovering hosts or broadcasting that you are hosting on a network.
/// This is just to discover or share a host ip address. The NetworkManager Unity component is what lets you CONNECT to each other.
/// Get connected for free free with education connection.
/// </summary>
public class CustomNetworkDiscovery : NetworkDiscovery
{
    private Coroutine cleanup;
    private float timeout = 1;
    private string room;

    private class LANInfo
    {
        public float removeAtTime;
        public GameObject roomButton;
        public string ipAddress;
        public string roomName;
    }

    private List<LANInfo> lanAddresses = new List<LANInfo>();

    /// <summary>
    /// Client starts listening
    /// </summary>
    public void ListenForLANServers()
    {
        Initialize();
        StartAsClient();
        cleanup = StartCoroutine(CleanUpExpiredEntries());
    }

    /// <summary>
    /// Client stops listening
    /// </summary>
    public void StopListening()
    {
        if (isClient)
        {
            StopBroadcast();
        }
        StopCoroutine(cleanup);
    }

    /// <summary>
    /// Host begins broadcasting
    /// </summary>
    public void BroadcastAsServer(string roomName)
    {
        room = roomName;
        BroadcastOpenRoom();
        StopBroadcast();
        Initialize();
        StartAsServer();
    }

    public void BroadcastClosedRoom()
    {
        broadcastData = "[FULL]\0";
        if (isServer)
        {
            StopBroadcast();
            StartCoroutine(Delay(StartAsServer));
        }
    }

    public void BroadcastOpenRoom()
    {
        broadcastData = room + '\0';
        if (isServer)
        {
            StopBroadcast();
            StartCoroutine(Delay(StartAsServer));
        }
    }

    private IEnumerator Delay(Func<bool> callback)
    {
        yield return new WaitForSeconds(0.1f);
        callback();
    }

    /// <summary>
    /// This is just because im bad at code
    /// </summary>
    private void OnApplicationQuit()
    {
        if (running)
            StopBroadcast();
    }

    /// <summary>
    /// Remove available room if it stops broadcasting
    /// </summary>
    /// <returns></returns>
    private IEnumerator CleanUpExpiredEntries()
    {
        while (true)
        {
            for (int i = 0; i < lanAddresses.Count; i++)
            {
                if (lanAddresses[i].removeAtTime <= Time.time)
                {
                    TitleUIManager.HostJoinRoomMenu.RemoveRoom(lanAddresses[i].roomButton);
                    lanAddresses.RemoveAt(i);
                    i--;
                }
            }
            
            yield return new WaitForSeconds(timeout);
        }
    }

    /// <summary>
    /// Parse room name, ip address, and add a button to our room list
    /// </summary>
    /// <param name="rawAddress"></param>
    /// <param name="data"></param>
    public override void OnReceivedBroadcast(string rawAddress, string data)
    {
        base.OnReceivedBroadcast(rawAddress, data);

        //::ffff:192.168.0.4 this is a rawAddress value
        int ipStart = rawAddress.LastIndexOf(':') + 1;
        string cleanAddress = rawAddress.Substring(ipStart);

        for (int i = 0; i < lanAddresses.Count; i++)
        {
            if (lanAddresses[i].ipAddress == cleanAddress)
            {
                if (data.StartsWith("[FULL]"))
                    TitleUIManager.HostJoinRoomMenu.RemoveRoom(lanAddresses[i].roomButton);
                else
                    lanAddresses[i].removeAtTime = Time.time + timeout;
                return;
            }
        }

        if (data.StartsWith("[FULL]"))
            return;

        GameObject button = TitleUIManager.HostJoinRoomMenu.AddRoom(data);
        
        LANInfo info = new LANInfo()
        {
            removeAtTime = Time.time + timeout,
            ipAddress = cleanAddress,
            roomButton = button,
            roomName = data
        };

        lanAddresses.Add(info);
    }
    
    /// <param name="roomName">The name of the room as appears on a button</param>
    /// <returns>The pretty ip address that you connect with i.e. "192.168.0.2"</returns>
    public string GetAddressOfRoom(string roomName)
    {
        for (int i = 0; i < lanAddresses.Count; i++)
        {
            if (lanAddresses[i].roomName == roomName)
            {
                return lanAddresses[i].ipAddress;
            }
        }
        return null;
    }
}
#pragma warning restore CS0618