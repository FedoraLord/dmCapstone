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
	public bool IsDone { get { return numPlayersInWinArea == 4; } }

    private void Start()
    {
        Instance = this;

        numPlayersInWinArea = 0;

        //winText.text = "";

        if (NetworkWrapper.IsHost && playerPrefab != null)
        {
            //SpawnPlayers();
            StartCoroutine(_SpawnPlayers());
        }
    }

    private void LateUpdate()
    {
        if (IsDone)
        {
            winText.text = "Success!";
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private IEnumerator _SpawnPlayers()
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

    public virtual void SpawnPlayers()
    {
        for (int i = 0; i < PersistentPlayer.players.Count; i++)
        {
            GameObject obj = Instantiate(playerPrefab);
            obj.transform.position = spawnPoints[i].position;

            PersistentPlayer p = PersistentPlayer.players[i];

            if (p.connectionToClient.isReady)
            {

            }

            obj.GetComponent<SM_Player>().playernum = p.playerNum;
            NetworkSpawner.Instance.NetworkSpawn(obj, p.gameObject);
        }
    }
}
