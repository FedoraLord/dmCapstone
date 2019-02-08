using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkWrapper : MonoBehaviour
{
    public NetworkDiscovery discovery;
    public NetworkManager manager;
    public GameObject flagme;

    private void Start()
    {
        StartCoroutine(CheckBroadcasts());
    }

    private IEnumerator CheckBroadcasts()
    {
        yield return new WaitUntil(() => discovery.broadcastsReceived != null);

        while (true)
        {
            yield return new WaitForSeconds(3);

            int count = discovery.broadcastsReceived.Count;
            foreach (string key in discovery.broadcastsReceived.Keys)
            {
                Debug.LogFormat("{0} broadcasts received - last one was {1}", count, key);
                flagme.SetActive(true);
                break;
            }
        }
    }
}
