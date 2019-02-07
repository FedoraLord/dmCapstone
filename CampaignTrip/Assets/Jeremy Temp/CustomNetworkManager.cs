using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager
{
    public static CustomNetworkManager Instance;

    private void Start()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple CustomNetworkManager objects exist.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}