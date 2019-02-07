using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkDiscovery : NetworkDiscovery
{
    private Coroutine cleanup;
    private float timeout = 5;

    private class LANInfo
    {
        public float removeAtTime;
        public string data;
        public string ipAddress;

        public LANInfo(float _removeAtTime, string _data, string _ipAddress)
        {
            removeAtTime = _removeAtTime;
            data = _data;
            ipAddress = _ipAddress;
        }
    }

    private List<LANInfo> lanAddresses = new List<LANInfo>();

    private void Awake()
    {
        Initialize();
        StartAsClient();
        StartCoroutine(CleanUpExpiredEntries());
    }

    public void BroadcastAsServer()
    {
        StopBroadcast();
        Initialize();
        StartAsServer();
    }

    private void OnApplicationQuit()
    {
        StopBroadcast();
    }

    private IEnumerator CleanUpExpiredEntries()
    {
        while (true)
        {
            bool changed = false;

            for (int i = 0; i < lanAddresses.Count; i++)
            {
                if (lanAddresses[i].removeAtTime <= Time.time)
                {
                    lanAddresses.RemoveAt(i);
                    i--;
                    changed = true;
                }
            }

            if (changed)
            {
                //UpdateMatchInfoUI(); - not implemented
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

        lanAddresses.Add(new LANInfo(Time.time + timeout, data, fromAddress));
        //UpdateMatchInfoUI
    }
}
