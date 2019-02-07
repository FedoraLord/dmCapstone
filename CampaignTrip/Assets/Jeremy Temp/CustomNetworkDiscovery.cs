using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CustomNetworkDiscovery : NetworkDiscovery
{
    private Coroutine cleanup;
    private float timeout = 5;

    private class LANInfo
    {
        public float removeAtTime;
        public GameObject roomButton;
        public string data;
        public string ipAddress;
        public string roomName;
    }

    private List<LANInfo> lanAddresses = new List<LANInfo>();

    public void ListenForLANServers()
    {
        Initialize();
        StartAsClient();
        cleanup = StartCoroutine(CleanUpExpiredEntries());
    }

    public void StopListening()
    {
        if (isClient)
        {
            StopBroadcast();
        }
        StopCoroutine(cleanup);
    }

    public void BroadcastAsServer()
    {
        StopBroadcast();
        Initialize();
        StartAsServer();
    }

    private void OnApplicationQuit()
    {
        if (running)
            StopBroadcast();
    }

    private IEnumerator CleanUpExpiredEntries()
    {
        while (true)
        {
            for (int i = 0; i < lanAddresses.Count; i++)
            {
                if (lanAddresses[i].removeAtTime <= Time.time)
                {
                    TitleUIManager.Instance.hostJoinRoomMenu.RemoveRoom(lanAddresses[i].roomButton);
                    lanAddresses.RemoveAt(i);
                    i--;
                }
            }
            
            yield return new WaitForSeconds(timeout);
        }
    }

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
                lanAddresses[i].removeAtTime = Time.time + timeout;
                return;
            }
        }

        GameObject button = TitleUIManager.Instance.hostJoinRoomMenu.AddRoom(data);
        string roomName = button.GetComponentInChildren<Text>().text;
        
        LANInfo info = new LANInfo()
        {
            removeAtTime = Time.time + timeout,
            data = data,
            ipAddress = cleanAddress,
            roomButton = button,
            roomName = roomName
        };

        lanAddresses.Add(info);
    }

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
