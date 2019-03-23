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
    public float timer = 30;
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

<<<<<<< HEAD
	protected virtual void Win()
	{
		//do something when you win, or not
	}

    protected virtual void Lose()
    {

    }

    [Command]
	public void CmdWin()
	{
		Win();
		RpcWin();
	}

    [Command]
    public void CmdLose()
    {
        Lose();
        RpcLose();
    }

    [ClientRpc]
	public void RpcWin()
	{
		Win();
	}

    [ClientRpc]
    public void RpcLose()
    {
        Lose();
    }
=======
    protected abstract void Win();
    protected abstract void Lose();
>>>>>>> parent of 0764261... Merge remote-tracking branch 'origin/cardMinigame' into roman
}
