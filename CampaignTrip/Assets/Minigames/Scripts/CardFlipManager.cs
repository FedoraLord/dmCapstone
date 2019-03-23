using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardFlipManager : MinigameManager
{
	public bool CanFlip { get { return PersistentPlayer.localAuthority == playerWhoPicksTheCard; } }

	public Sprite winningSprite;
	public Sprite failSprite;
	public List<FlippableCard> cards;

	private PersistentPlayer playerWhoPicksTheCard;

	protected override IEnumerator HandlePlayers(List<PersistentPlayer> randomPlayers)
	{
		List<FlippableCard> randomCards = new List<FlippableCard>(cards);
		//Fisher-Yates Shuffle
		for (var i = 0; i < randomCards.Count - 1; i++)
		{
			int randomNum = Random.Range(i, randomCards.Count);
			//now swap them
			FlippableCard tmp = randomCards[i];
			randomCards[i] = randomCards[randomNum];
			randomCards[randomNum] = tmp;
		}

		List<PersistentPlayer> randomPlayersCopy = new List<PersistentPlayer>(randomPlayers);
		playerWhoPicksTheCard = randomPlayersCopy[0];
		randomPlayersCopy.Remove(playerWhoPicksTheCard);
		FlippableCard winningCard = randomCards[0];
		randomCards.Remove(winningCard);

		for (int playerNum = 0; playerNum < randomPlayersCopy.Count; playerNum++)
		{
			PersistentPlayer p = randomPlayersCopy[0];
			yield return new WaitUntil(() => p.connectionToClient.isReady);
			
			//select a number of cards based on how many players have yet to get their cards
			for(int cardsToReveal = randomCards.Count / randomPlayersCopy.Count; cardsToReveal > 0; cardsToReveal--)
			{
				Debug.Log(p);
				Debug.Log(randomCards[0]);
				Debug.Log(p.connectionToClient);
				randomCards[0].TargetFlip(p.connectionToClient);
				randomCards.RemoveAt(0);
			}

			randomPlayers.Remove(p);
		}
	}

    protected override void Win()
    {
        throw new System.NotImplementedException();
    }

    protected override void Lose()
    {
        throw new System.NotImplementedException();
    }
}
