using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public abstract class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance;

    public List<Transform> spawnPoints;

	public int numPlayersWon = 0;
    public int timer = 30;
	public Text winText;

    protected virtual void Start()
    {
        Instance = this;

		numPlayersWon = 0;

        if (NetworkWrapper.IsHost)
		{
			List<PersistentPlayer> randomPlayers = new List<PersistentPlayer>(PersistentPlayer.players);
			//Fisher-Yates Shuffle
			for (var i = 0; i < randomPlayers.Count - 1; i++)
			{
				//using a range of i to the size avoids bias
				int randomNum = Random.Range(i, randomPlayers.Count);
				//now swap them
				PersistentPlayer tmp = PersistentPlayer.players[i];
				PersistentPlayer.players[i] = PersistentPlayer.players[randomNum];
				PersistentPlayer.players[randomNum] = tmp;
			}

			StartCoroutine(HandlePlayers(randomPlayers));
        }
    }

    private IEnumerator DecrementTimer()
    {
        
    }

    private void LateUpdate()
    {
		if (numPlayersWon != 0 && numPlayersWon == PersistentPlayer.players.Count)
		{
            Win();
            BattleController.Instance.UnloadMinigame(true);
		}
        // TODO Add timer for failure
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    protected virtual IEnumerator HandlePlayers(List<PersistentPlayer> randomPlayers)
    {
		yield return 0; //please override this, also we have to return something here
	}

    protected abstract void Win();
}
