using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MinigameManager : NetworkBehaviour
{
    public GameObject playerPrefab;

    public Camera mainCamera;

    // Start is called before the first frame update
    void Start()
    {
        CmdInitialize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnLevelWasLoaded(int level)
    {
        
    }

    [Command]
    private void CmdInitialize()
    {
        GameObject player = Instantiate(playerPrefab);
        NetworkServer.Spawn(player);

        mainCamera.transform.parent = player.transform;
        mainCamera.transform.localPosition = new Vector3(0, 0, -10f);
        mainCamera.transform.LookAt(player.transform);
    }
}
