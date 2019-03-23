using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CS_Manager : MinigameManager
{
    public static new CS_Manager Instance;

    public Text timerText;
    public List<CS_Card> sequenceCards;
    public List<CS_Card> selectCards;

    private PersistentPlayer sequencePlayer;
    private List<PersistentPlayer> viewingPlayers;

    private int sequenceIterator;

    // Start is called before the first frame update
    protected override void Start()
    {
        Instance = this;
        base.Start();
        GenerateCardSequence();
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

    private void GenerateCardSequence()
    {
        foreach(var sequenceCard in sequenceCards)
        {
            int index = Random.Range(0, sequenceCards.Count);
            CS_Card selectedCard = selectCards[index];
            sequenceCard.name = selectedCard.name;
            sequenceCard.Sprite = selectedCard.Sprite;
        }
    }

    public void CardSelected(CS_Card card)
    {
        if (card.name.Equals(sequenceCards[sequenceIterator].name))
        {
            sequenceIterator++;
            // TODO make visual showing success

            if (sequenceIterator > 4)
            {
                CmdWin();
            }
        }
        else
        {
            sequenceIterator = 0;
            // TODO make visual showing failed sequence
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
