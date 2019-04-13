using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

#pragma warning disable 0618
public class CS_Manager : MinigameManager
{
    public bool CanSelect { get { return rndSeqIndecies.Length > userSequence.Count; } }
    
    public List<CS_Card> selectableCards;
    public List<CS_Card> sequenceCards;
    public GameObject selectCardObject;
    public GameObject sequenceCardObject;

    //contains the indecies in selectCards
    private List<CS_Card> randomSequenceCards;
    private List<CS_Card> unassignedCards;
    private List<int> userSequence = new List<int>();
    private int[] rndSeqIndecies;

    public static CS_Manager GetInstance()
    {
        return Instance as CS_Manager;
    }

    protected override void Start()
    {
        base.Start();
        
        for (int i = 0; i < selectableCards.Count; i++)
        {
            selectableCards[i].InitializeSelectable(i);
        }
    }

    [Server]
    protected override IEnumerator HandlePlayers(List<PersistentPlayer> randomPlayers)
    {
        yield return base.HandlePlayers(randomPlayers);
        
        TargetShowSelectCards(randomPlayers[0].connectionToClient);

		GenerateCardSequence();
        for (int i = 1; i < randomPlayers.Count; i++)
        {
            //will look something like this { -1, 3, -1, -1, 8, -1 } where non-negatives are their shown cards
            int[] indecies = new int[randomSequenceCards.Count];
            indecies.Initialize(() => -1);
            
            //chooses 6 / 3 = 2 cards per player, did it this way so you dont have to use 4 players to test the minigame
            for (int j = 0; j < indecies.Length / (randomPlayers.Count - 1); j++)
            {
                int k = Random.Range(0, unassignedCards.Count);
                CS_Card assignment = unassignedCards[k];
                int sequenceIndex = randomSequenceCards.IndexOf(assignment);
                indecies[sequenceIndex] = assignment.index;
                unassignedCards.RemoveAt(k);
            }
            
            TargetShowSequenceCards(randomPlayers[i].connectionToClient, indecies);
        }

		GetComponent<NetworkIdentity>().AssignClientAuthority(randomPlayers[0].connectionToClient); //give the person who selects the card the ability to end the game
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
        randomSequenceCards = new List<CS_Card>();
        unassignedCards = new List<CS_Card>();
        for (int i = 0; i < sequenceCards.Count; i++)
        {
            int tempPoolIndex = Random.Range(0, tempPool.Count);
            CS_Card randomCard = tempPool[tempPoolIndex];
            tempPool.RemoveAt(tempPoolIndex);

            randomSequenceCards.Add(randomCard);
            unassignedCards.Add(randomCard);
        }

        int[] rnd = randomSequenceCards.Select(x => x.index).ToArray();
        RpcSendRandomSequence(rnd);
    }
    
    [ClientRpc]
    private void RpcSendRandomSequence(int[] seq)
    {
        rndSeqIndecies = seq;
    }

    public void CardSelected(CS_Card card)
    {
        userSequence.Add(card.index);
        if (userSequence.Count == rndSeqIndecies.Length)
        {
            for (int i = 0; i < rndSeqIndecies.Length; i++)
            {
                if (userSequence[i] != rndSeqIndecies[i])
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
}
