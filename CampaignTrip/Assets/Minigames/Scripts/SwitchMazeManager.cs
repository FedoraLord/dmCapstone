using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

#pragma warning disable 0618
public class SwitchMazeManager : MinigameManager
{
	public GameObject playerPrefab;
    public Text timerText;
    public Camera cam;

    protected override void Win()
	{
		winText.text = "Success!";
	}

    protected override void Lose()
    {
        winText.text = "Failure";
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        timerText.text = string.Format("Time Remaining: {0}", (int)timer);

        if (timer <= 0.0f)
        {
            Lose();
            BattleController.Instance.UnloadMinigame(false);
        }
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


}
