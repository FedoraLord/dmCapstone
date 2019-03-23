using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CS_Manager : MinigameManager
{
    public Text timerText;
    public List<CS_Card> sequenceCards;

    private PersistentPlayer sequencePlayer;
    private List<PersistentPlayer> viewingPlayers;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
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
        for (int i = 0; i < randomPlayers.Count; i++)
        {
            PersistentPlayer p = randomPlayers[i];
            yield return new WaitUntil(() => p.connectionToClient.isReady);
        }
    }

    protected override void Win()
    {
        winText.text = "Success!";
    }

    protected override void Lose()
    {
        winText.text = "Lose";
    }
}
