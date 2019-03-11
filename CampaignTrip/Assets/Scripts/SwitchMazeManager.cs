using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchMazeManager : MinigameManager
{
	public GameObject playerPrefab;
	
	protected override void Win()
	{
		winText.text = "Success!";
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
			NetworkSpawner.Instance.NetworkSpawn(obj, p.gameObject);
		}
	}
}
