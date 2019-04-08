using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

#pragma warning disable 0618
public class CS_Manager : MinigameManager
{
    public Text timerText;
    public List<CS_Card> selectableCards;
    public List<CS_Card> sequenceCards;
    public GameObject selectCardObject;
    public GameObject sequenceCardObject;

    //contains the indecies in selectCards
    private List<CS_Card> randomSequence;
    private List<CS_Card> unassignedCards;
    private List<int> userSequence = new List<int>();

    protected override void Start()
    {
        base.Start();
        for (int i = 0; i < selectableCards.Count; i++)
        {
            selectableCards[i].InitializeSelectable(i);
        }
    }

    protected override IEnumerator HandlePlayers(List<PersistentPlayer> randomPlayers)
    {
        yield return base.HandlePlayers(randomPlayers);
        
        StartCoroutine(Timer());

        TargetShowSelectCards(randomPlayers[0].connectionToClient);

        GenerateCardSequence();
        for (int i = 1; i < randomPlayers.Count; i++)
        {
            //will look something like this { -1, 3, -1, -1, 8, -1 } where non-negatives are their shown cards
            int[] indecies = new int[randomSequence.Count];
            for (int j = 0; j < indecies.Length; j++)
            {
                indecies[j] = -1;
            }
            
            //chooses 6 / 3 = 2 cards per player, did it this way so you dont have to use 4 players to test the minigame
            for (int j = 0; j < indecies.Length / (randomPlayers.Count - 1); j++)
            {
                int k = Random.Range(0, unassignedCards.Count);
                CS_Card assignment = unassignedCards[k];
                int sequenceIndex = randomSequence.IndexOf(assignment);
                indecies[sequenceIndex] = assignment.index;
                unassignedCards.RemoveAt(k);
            }
            
            TargetShowSequenceCards(randomPlayers[i].connectionToClient, indecies);
        }
    }

    private IEnumerator Timer()
    {
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            timerText.text = string.Format("Time Remaining: {0}", (int)timer);
            yield return new WaitForEndOfFrame();
        }

        Lose();
        BattleController.Instance.UnloadMinigame(false);
    }

    [TargetRpc]
    private void TargetShowSelectCards(NetworkConnection connection)
    {
        selectCardObject.SetActive(true);
        sequenceCardObject.SetActive(false);
    }
    
    [Server]
    private void GenerateCardSequence()
    {
        List<CS_Card> tempPool = new List<CS_Card>(selectableCards);
        randomSequence = new List<CS_Card>();
        unassignedCards = new List<CS_Card>();
        for (int i = 0; i < sequenceCards.Count; i++)
        {
            int tempPoolIndex = Random.Range(0, tempPool.Count);
            CS_Card randomCard = tempPool[tempPoolIndex];
            tempPool.RemoveAt(tempPoolIndex);

            randomSequence.Add(randomCard);
            unassignedCards.Add(randomCard);
        }
    }

    public void CardSelected(CS_Card card)
    {
        userSequence.Add(card.index);
        if (userSequence.Count == randomSequence.Count)
        {
            for (int i = 0; i < randomSequence.Count; i++)
            {
                if (userSequence[i] != randomSequence[i].index)
                {
                    CmdLose();
                    return;
                }
            }
            CmdWin();
        }
    }

    [TargetRpc]
    private void TargetShowSequenceCards(NetworkConnection connection, int[] cardIndecies)
    {
        for(int i = 0; i < cardIndecies.Length; i++)
        {
            if (cardIndecies[i] >= 0)
            {
                sequenceCards[i].spriteRenderer.sprite = selectableCards[cardIndecies[i]].spriteRenderer.sprite;
            }
        }

        selectCardObject.SetActive(false);
        sequenceCardObject.SetActive(true);
    }
    
    protected override void Win()
    {
        winText.text = "Success!";
        StartCoroutine(EndGame(true));
    }

    protected override void Lose()
    {
        winText.text = "Lose";
        StartCoroutine(EndGame(false));
    }

    private IEnumerator EndGame(bool won)
    {
        if (isServer)
        {
            yield return new WaitForSeconds(1);
            BattleController.Instance.UnloadMinigame(won);
        }
    }
}
