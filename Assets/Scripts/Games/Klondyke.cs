using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

public class Klondyke : Game
{
	// TODO position piles according to camera position, no need for these after that
	public Transform wasteHolder, pileauHolder, foundHolder;
	
	public PileSlot pileSlot, foundationSlot;

	private List<CardPile> foundations;

	public float xStep = 3.5f;

    private void Start()
    {
		gameType = GameType.Klondyke;
		stockDrawAmount = 3;

		DealState = gameObject.AddComponent<DealStateKlondyke>();
    }

    public override IEnumerator SetupTable()
	{
		Waste = new GameObject("KlondykeWaste").AddComponent<CardPile>();
		Waste.transform.parent = transform;
		Waste.transform.position = wasteHolder.position;
		Waste.zStep = stock.zStep;
		Waste.Type = PileType.KlondikeWaste;
		Piles.Add(Waste);
		
		PileSlot wasteSlot = Instantiate(pileSlot, Waste.transform.position, Quaternion.identity, Waste.transform);
		wasteSlot.Initialize(Waste);
		
		TableauPiles = new List<CardPile>();
		
		for (int i = 0; i < 7; i++)
        {
			CardPile pile = new GameObject("Tableu "+i).AddComponent<CardPile>();
			pile.gameObject.name = "KlondykeTableu " + (i + 1);
			pile.transform.parent = transform;
			pile.transform.position = new Vector3(pileauHolder.position.x + (xStep * i), pileauHolder.position.y, pileauHolder.position.z);
			pile.Type = PileType.Tableau;
			pile.yStep = .65f;
			pile.yStepTurned = .3f;
			
			TableauPiles.Add(pile);
			
			PileSlot slot = Instantiate(pileSlot, pile.transform.position, Quaternion.identity, pile.transform);
			slot.Initialize(pile);

			TableauPiles[i].slot = slot;
			
			Piles.Add(pile);
		}

		foundations = new List<CardPile>();

		for (int i = 0; i < 4; i++)
		{
			CardPile foundation = new GameObject("KlondykeFoundation " + (Suit)i).AddComponent<CardPile>();
			foundation.Type = PileType.Foundation;
			foundation.transform.parent = transform;
			foundation.transform.position = new Vector3(foundHolder.position.x + (xStep * i), foundHolder.position.y, foundHolder.position.z);
			foundation.Suit = (Suit)i;

			foundations.Add(foundation);

			FoundationSlot slot = Instantiate(foundationSlot, foundation.transform.position, Quaternion.identity, foundation.transform) as FoundationSlot;
			slot.Initialize((Suit)i, foundation);

			foundation.slot = slot;

			Piles.Add(foundation);
		}

		yield return null;
	}

	public override List<Card> Select(Card c)
	{
		CardPile pile = c.pile;
		List<Card> selection = new List<Card>();

        if (pile.Type == PileType.Stock)
        {
			DrawCards();
		} 
        else if (pile.Type == PileType.KlondikeWaste)
		{
            if (!pile.IsEmpty)
				selection = new List<Card>() { pile.GetLastCard() };            
        }		
		else if (pile.Type == PileType.Foundation)
		{
            foreach(CardPile foundation in foundations)
            {
				if (pile != foundation) continue;
				
				int startIndex = pile.cards.IndexOf(c);

				for (int j = startIndex; j < pile.cards.Count; j++)
				{
					Card card = pile.GetCard(j);

					if (card == null) continue;

					if (!card.IsTurned())
						selection.Add(card);					
				}								
            }
		}
		else // tableu piles
		{
            foreach(CardPile tableauPile in TableauPiles)
            {
				if (pile != tableauPile) continue;

				int startIndex = pile.cards.IndexOf(c);

				for (int j = startIndex; j < pile.cards.Count; j++)
				{
					Card card = pile.GetCard(j);

					if (card == null) continue;

					if (!card.IsTurned())
						selection.Add(card);					
				}				
            } 
		}

		return selection;
	}

	public override bool TryMove(CardPile toPile, SelectionPile selectionPile)
	{
		CardPile sourcePile = selectionPile.sourcePile;
		List<Card> cards = selectionPile.cards;
		StateBase activeState = StateManager.Instance.activeState;

		// Try foundation
		if (cards.Count == 1 && toPile == sourcePile)
			toPile = foundations[(int)cards[0].suit];
		
		List<Card> movedCards = new List<Card>();

		if (toPile.Type == PileType.Foundation)
		{
			if (cards.Count > 1)
				return false;

            Card selectedCard = cards[0];
			Card lastCard = toPile.GetLastCard();

			if (lastCard != null && lastCard.suit != selectedCard.suit)
				return false;

			int lastCardNum = lastCard != null ? lastCard.number : 0;

            if (selectedCard.number == lastCardNum + 1)
            {
				movedCards.Add(selectedCard);

				if (!(activeState is HintState))
					NotificationCenter.DefaultCenter.PostNotification(this, GameEvents.FoundationMoveDone, iTween.Hash("suit", selectedCard.suit));
			}
		}		
		else if (toPile.Type == PileType.Tableau)
		{
			foreach(CardPile tableauPile in TableauPiles)			
            {
				if (toPile != tableauPile) continue;

				Card firstCard = cards[0];
				bool move = false;

				if (toPile.cards.Count == 0 && firstCard.number == 13)
				{
					move = true;				
				}
				else
				{
					Card lastPileCard = tableauPile.GetLastCard();	

					if (lastPileCard == null) continue;				

					if (lastPileCard.number - firstCard.number == 1 && Card.IsDifferentSuit(firstCard, lastPileCard))
						move = true;				
				}
				if (move) 
				{
					foreach(Card c in cards)
					{
						movedCards.Add(c);
					}
					break;
				}
            }
		}	
		if (movedCards.Count > 0)
		{
			if (!(activeState is HintState))
            {
				commands.Clear();

				MoveCards(movedCards, sourcePile, toPile);
				UpdateTableuPiles();

				GameManager.Instance.StoreCommand(new CmdComposite(commands));
			}			
		}

		return movedCards.Count > 0;
	}

	void UpdateTableuPiles()
    {
        // Turn last cards after movement
        foreach (CardPile pile in TableauPiles)
        {
            Card lastCard = pile.GetLastCard();

            if (lastCard != null && lastCard.IsTurned())
            {
				TurnCard(lastCard);
            }
        }
    }
	public override IEnumerator RestoreState(XDocument xdoc)
	{
		XElement piles = xdoc.Root.Element("piles");
		
		int fcount = 0;
		int tcount = 0;
		
		foreach (XElement pileNode in piles.Elements("pile"))
		{
			PileType pileType = (PileType) System.Enum.Parse(typeof(PileType), pileNode.Element("type").Value);
			
			CardPile cpile = null;

			if (pileType == PileType.Stock)
			{
				cpile = stock;
			}
			else
			{
				if (pileType == PileType.KlondikeWaste)
				{
					cpile = Waste;
				}
				if (pileType == PileType.Foundation)
				{
					cpile = foundations[fcount];
					fcount++;
				}
				if (pileType == PileType.Tableau)
				{
					cpile = TableauPiles[tcount];
					tcount++;
				}
			}
			
			XElement cards = pileNode.Element("cards");
			
			if (cards != null)
			{
				foreach (XElement cardNode in cards.Elements("card"))
				{
					Vector3 pos = XmlHelpers.ConvertStringToVector3(cardNode.Element("position").Value);
					int number = int.Parse(cardNode.Element("number").Value);
					Suit suit = (Suit) System.Enum.Parse(typeof(Suit), cardNode.Element("suit").Value);   

					Card c = stock.GetCard(number, suit);
					c.pile = cpile;
					c.Turn(System.Convert.ToBoolean(cardNode.Element("turned").Value));
					
					cpile.AddCard(c);
				}
			}
			
			cpile.AlignCards();
		}
		
		yield return null;
	}
}
