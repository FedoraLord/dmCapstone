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
    public static NetworkManager manager;
    public static NetworkWrapper Instance;

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
        manager = GetComponent<NetworkManager>();
    }
}
#pragma warning restore CS0618