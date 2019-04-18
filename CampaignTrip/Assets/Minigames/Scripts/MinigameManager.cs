using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

#pragma warning disable 0618
public abstract class MinigameManager : NetworkBehaviour
{
    public static MinigameManager Instance;

    public float timer = 30;
    public Text timerText;
    public Dialogue dialogue;

    [HideInInspector] public bool isGameOver;

    private Coroutine timerRoutine;

    protected virtual void Start()
    {
        Instance = this;
        PersistentPlayer.localAuthority.CmdReadyForMinigame();

        if (NetworkWrapper.IsHost)
		{
			List<PersistentPlayer> randomPlayers = new List<PersistentPlayer>(PersistentPlayer.players);
			//Fisher-Yates Shuffle
			for (var i = 0; i < randomPlayers.Count - 1; i++)
			{
				//using a range of i to the size avoids bias
				int randomNum = Random.Range(i, randomPlayers.Count);
				//now swap them
				PersistentPlayer tmp = randomPlayers[i];
				randomPlayers[i] = randomPlayers[randomNum];
				randomPlayers[randomNum] = tmp;
			}

			StartCoroutine(HandlePlayers(randomPlayers));
        }

        timerRoutine = StartCoroutine(Timer());
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private IEnumerator Timer()
    {
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            timerText.text = string.Format("Time Remaining: {0}", (int)timer + 1);
            yield return new WaitForEndOfFrame();
        }
        Lose();
    }
    
    [Server]
    protected virtual IEnumerator HandlePlayers(List<PersistentPlayer> randomPlayers)
    {
        yield return new WaitUntil(() => {
            return PersistentPlayer.localAuthority.minigameReady == randomPlayers.Count;
        });
    }
    
    [Command]
	public void CmdWin()
	{
		RpcWin();
	}

    [Command]
    public void CmdLose()
    {
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

    protected virtual void Win()
    {
        EndMinigame(true);
    }

    protected virtual void Lose()
    {
        EndMinigame(false);
    }

    private void EndMinigame(bool won)
    {
        if (timerRoutine != null)
            StopCoroutine(timerRoutine);
        isGameOver = true;

        string message = (won ? "Success" : "Failure");
        dialogue.DisplayMessage(message, 3, () => BattleController.Instance.UnloadMinigame(won));
    }
}
