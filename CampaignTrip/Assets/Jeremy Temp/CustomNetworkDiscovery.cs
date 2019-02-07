using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CustomNetworkDiscovery : NetworkDiscovery
{
    public static CustomNetworkDiscovery Instance;

    private Coroutine cleanup;
    private float timeout = 5;

    private class LANInfo
    {
        public GameObject roomButton;
        public float removeAtTime;
        public string data;
        public string ipAddress;

        public LANInfo(float _removeAtTime, string _data, string _ipAddress, GameObject _roomButton)
        {
            removeAtTime = _removeAtTime;
            data = _data;
            ipAddress = _ipAddress;
            roomButton = _roomButton;
        }
    }

    private List<LANInfo> lanAddresses = new List<LANInfo>();

    private void Start()
    {
        if(Instance != null)
        {
            Debug.LogError("Multiple CustomNetworkDiscovery objects exist.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ListenForLANServers()
    {
        Initialize();
        StartAsClient();
        cleanup = StartCoroutine(CleanUpExpiredEntries());
    }

    public void StopListening()
    {
        StopBroadcast();
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

    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        base.OnReceivedBroadcast(fromAddress, data);

        for (int i = 0; i < lanAddresses.Count; i++)
        {
            if (lanAddresses[i].ipAddress == fromAddress)
            {
                lanAddresses[i].removeAtTime = Time.time + timeout;
                return;
            }
        }

        GameObject button = TitleUIManager.Instance.hostJoinRoomMenu.AddRoom(data);
        lanAddresses.Add(new LANInfo(Time.time + timeout, data, fromAddress, button));
    }
}
