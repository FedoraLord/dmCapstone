using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MinigameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public List<Transform> spawnPoints;

    private void Start()
    {
        if (NetworkWrapper.IsHost && playerPrefab != null)
        {
            SpawnPlayers();
        }
    }

    public virtual void SpawnPlayers()
    {
        for (int i = 0; i < PersistentPlayer.players.Count; i++)
        {
            GameObject obj = Instantiate(playerPrefab);
            obj.transform.position = spawnPoints[i].position;
            NetworkSpawner.Instance.NetworkSpawn(obj);
        }
    }
}
