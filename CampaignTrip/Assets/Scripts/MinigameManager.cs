using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MinigameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public List<Transform> spawnPoints;

    public Camera mainCamera;
    public static MinigameManager Instance;

    private void Start()
    {
        Instance = this;

        if (NetworkWrapper.IsHost && playerPrefab != null)
        {
            SpawnPlayers();
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public virtual void SpawnPlayers()
    {
        //var i = 0;
        //foreach(PersistentPlayer player in PersistentPlayer.players)
        //{
        //    GameObject obj = Instantiate(playerPrefab);
        //    obj.transform.position = spawnPoints[i].position;

        //    obj.GetComponent<SM_Player>().playernum = player.playerNum;
        //    NetworkSpawner.Instance.NetworkSpawn(obj);
        //    i++;
        //}

        var player = PersistentPlayer.players[0];
        GameObject obj = Instantiate(playerPrefab);
        obj.transform.position = spawnPoints[0].position;

        obj.GetComponent<SM_Player>().playernum = player.playerNum;
        NetworkSpawner.Instance.NetworkSpawn(obj);

        player = PersistentPlayer.players[1];
        obj = Instantiate(playerPrefab);
        obj.transform.position = spawnPoints[1].position;

        obj.GetComponent<SM_Player>().playernum = player.playerNum;
        NetworkSpawner.Instance.NetworkSpawn(obj);
    }
}
