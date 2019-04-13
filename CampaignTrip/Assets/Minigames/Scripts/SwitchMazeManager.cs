using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

#pragma warning disable 0618
public class SwitchMazeManager : MinigameManager
{
    public List<Transform> spawnPoints;
    public GameObject playerPrefab;
    public Camera cam;

    private int numPlayersWon;

    public static SwitchMazeManager GetInstance()
    {
        return Instance as SwitchMazeManager;
    }
    
    protected override IEnumerator HandlePlayers(List<PersistentPlayer> randomPlayers)
	{
		if (!playerPrefab)
			throw new System.Exception("Please assign a prefab to the Player Prefab value in the editor");

		for (int i = 0; i < randomPlayers.Count; i++)
		{
			PersistentPlayer p = randomPlayers[i];
			yield return new WaitUntil(() => p.connectionToClient.isReady);

			GameObject obj = Instantiate(playerPrefab);
			obj.transform.position = spawnPoints[i].position;
			obj.GetComponent<SM_Player>().playernum = p.playerNum;
            NetworkServer.SpawnWithClientAuthority(obj, p.gameObject);
		}
	}

    public void PlayerEnteredWinArea()
    {
        numPlayersWon++;
        if (isServer && numPlayersWon == PersistentPlayer.players.Count)
        {
            if (!isGameOver)
            {
                Instance.CmdWin();
            }
        }
    }
    
    public void PlayerLeftWinArea()
    {
        numPlayersWon--;
    }
}
