using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MinigameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public List<Transform> spawnPoints;

    public Camera mainCamera;
    public static MinigameManager Instance;

    public int numPlayersInWinArea;
    public Text winText;

    private void Start()
    {
        Instance = this;
        numPlayersInWinArea = 0;

        if (NetworkWrapper.IsHost && playerPrefab != null)
        {
            StartCoroutine(SpawnPlayers());
        }
    }

    private void LateUpdate()
    {
        if (numPlayersInWinArea == 4)
        {
            winText.text = "Success!";
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private IEnumerator SpawnPlayers()
    {
        for (int i = 0; i < PersistentPlayer.players.Count; i++)
        {
            PersistentPlayer p = PersistentPlayer.players[i];
            yield return new WaitUntil(() => p.connectionToClient.isReady);

            GameObject obj = Instantiate(playerPrefab);
            obj.transform.position = spawnPoints[i].position;

            obj.GetComponent<SM_Player>().playernum = p.playerNum;
            NetworkSpawner.Instance.NetworkSpawn(obj, p.gameObject);
        }
    }
}
