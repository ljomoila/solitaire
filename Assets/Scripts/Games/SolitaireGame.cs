
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

public class SolitaireGame : MonoBehaviour
{
	public GameType gameType;

	public CardPile stock, waste;

	public List<CardPile> AllPiles { get; set; } = new List<CardPile>();
	public List<CardPile> TableauPiles { get; set; } = new List<CardPile>();
	public List<CardPile> AllowedPiles { get; set; } = new List<CardPile>();

	public DealState DealState { get; set; } = null;

	public virtual IEnumerator Initialize()
	{
		yield return null;
	}
	
	public virtual bool TryMove(CardPile toPile, SelectionPile sourcePile)
	{
		return false;
	}


	public Card GetCard(int number, CardSuit suit)
	{
		Card c = null;
		
		for(int i = 0; i < stock.cards.Count; i++)
		{
			c = stock.cards[i];
			
			if (c.number == number && c.suit == suit)
			{
				stock.RemoveCard(c);
				break;
			}
		}
		
		return c;
	}
	
	public virtual IEnumerator RestoreState(XDocument xdoc)
	{
		yield return null;	
	}

	public void MenuState()
    {
		foreach (CardPile pile in AllPiles)
		{
			foreach (Card card in pile.cards)
			{
				if (!card.IsTurned())
				{
					card.sprite.transform.localEulerAngles = new Vector3(0, 180, 0);
				}
			}
		}
	}

	public virtual List<Card> Select(Card card) 
	{
		return null;
	}
}

public enum GameType
{
	Klondyke,
	Spider,
	Freecell,
	Golf,
	Pyramid,
	EightOff,
	Clock
}
