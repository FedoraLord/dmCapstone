using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CS_Manager : MinigameManager
{
    public static new CS_Manager Instance;

    public Text timerText;
    public List<CS_Card> sequenceCards;
    public List<CS_Card> selectCards;
    public List<Sprite> cardSprites;
    public GameObject selectCardObject;
    public GameObject sequenceCardObject;

    private List<CS_Card> unassignedCards;

    private int sequenceIterator;

    // Start is called before the first frame update
    protected override void Start()
    {
        Instance = this;
        if (NetworkWrapper.IsHost)
            GenerateCardSequence();

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

    [Server]
    private void GenerateCardSequence()
    {
        foreach (var sequenceCard in sequenceCards)
        {
            int index = Random.Range(0, sequenceCards.Count);
            CS_Card selectedCard = selectCards[index];
            sequenceCard.Sprite = selectedCard.Sprite;
            sequenceCard.index = selectedCard.index;
        }
        unassignedCards = sequenceCards;
    }

    public void CardSelected(CS_Card card)
    {
        if (card.index == sequenceCards[sequenceIterator].index)
        {
            sequenceIterator++;
            // TODO make visual showing success

            if (sequenceIterator > sequenceCards.Count - 1)
            {
                CmdWin();
            }
        }
        else
        {
            CmdLose();
            // TODO make visual showing failed sequence
        }
    }

    protected override IEnumerator HandlePlayers(List<PersistentPlayer> randomPlayers)
    {
        yield return base.HandlePlayers(randomPlayers);
        randomPlayers = PersistentPlayer.players;

        PersistentPlayer p = randomPlayers[0];
        TargetShowSelectCards(p.connectionToClient);

        for (int i = 1; i < randomPlayers.Count; i++)
        {
            p = randomPlayers[i];
            int[] cardsToShow = GetCardsToShow();
            if (p.isServer)
            {
                TargetShowSequenceCards(p.connectionToClient, cardsToShow);
            }
            else
            {
                TargetShowSequenceCards(p.connectionToClient, cardsToShow);
            }
        }
    }

    private int[] GetCardsToShow()
    {
        int[] cards = { 0, 0, 0, 0, 0 };
        int randomCard = Random.Range(0, unassignedCards.Count);
        cards[randomCard] = unassignedCards[randomCard].index;
        unassignedCards.RemoveAt(randomCard);
        if (unassignedCards.Count > 1)
        {
            randomCard = Random.Range(0, unassignedCards.Count);
            cards[randomCard] = unassignedCards[randomCard].index;
            unassignedCards.RemoveAt(randomCard);
        }

        return cards;
    }

    private void ServerShowSequenceCards(int[] cardsToShow) 
    {
        for (int i = 0; i < cardsToShow.Length; i++)
        {
            sequenceCards[i].Sprite = cardSprites[cardsToShow[i]];
        }
    }

    [TargetRpc]
    private void TargetShowSequenceCards(NetworkConnection connection, int[] cardInfo)
    {
        for(int i = 0; i < sequenceCards.Count; i++)
        {

            sequenceCards[i].index = cardInfo[i];
            sequenceCards[i].Sprite = cardSprites[cardInfo[i]];
        }

        selectCardObject.SetActive(false);
        sequenceCardObject.SetActive(true);
    }

    [TargetRpc]
    private void TargetShowSelectCards(NetworkConnection connection)
    {
        selectCardObject.SetActive(true);
        sequenceCardObject.SetActive(false);
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
