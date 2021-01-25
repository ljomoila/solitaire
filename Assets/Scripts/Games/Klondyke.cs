using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

public class Klondyke : Game
{
	// TODO position piles according to camera position, no need for these after that
	public Transform wasteHolder, pileauHolder, foundHolder;
	
	public PileSlot pileSlot, foundationSlot;

	private List<CardPile> foundations = new List<CardPile>();

	public float xStep = 3.5f;

    private void Awake()
    {
		dealState = gameObject.AddComponent<DealStateKlondyke>();
		hintState = gameObject.AddComponent<HintKlondyke>();
    }

    private void Start()
    {
		gameType = GameType.Klondyke;
		stockDrawAmount = 3;
    }

    public override IEnumerator Initialize()
	{ 		Waste = new GameObject("KlondykeWaste").AddComponent<CardPile>();
		Waste.transform.parent = transform;
		Waste.transform.position = wasteHolder.position;
		Waste.zStep = stock.zStep;
		Waste.Type = PileType.Waste;
		Piles.Add(Waste);
		
		PileSlot wasteSlot = Instantiate(pileSlot, Waste.transform.position, Quaternion.identity, Waste.transform);
		wasteSlot.Initialize(Waste);

		for (int i = 0; i < 7; i++)
        {
			CardPile pile = new GameObject("KlondykeTableu " + i).AddComponent<CardPile>();
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

	public override List<Card> SelectCards(Card c)
	{
		CardPile pile = c.pile;
		List<Card> selection = new List<Card>();

        if (pile.Type == PileType.Stock)
        {
			DrawCards(3);
		} 
        else if (pile.Type == PileType.Waste)
		{
            if (!pile.IsEmpty)
				selection = new List<Card>() { pile.GetLastCard() };            
        }		
		else if (pile.Type == PileType.Foundation)
        {
            selection = SelectFromPiles(c, foundations);
        }
        else // tableu piles
		{
			selection = SelectFromPiles(c, TableauPiles);
			
		}

		return selection;
	}

    private List<Card> SelectFromPiles(Card card, List<CardPile> piles)
    {
		CardPile pile = card.pile;
		List<Card> selection = new List<Card>();

		foreach(CardPile p in piles)
        {
            if (pile != p) continue;

            int startIndex = pile.cards.IndexOf(card);

            for (int i = startIndex; i < pile.cards.Count; i++)
            {
                Card c = pile.GetCard(i);

                if (c == null) continue;

                if (!c.IsTurned())
                    selection.Add(c);
            }
        }

		return selection;
    }


    public override bool TryMoveToPile(CardPile toPile, SelectionPile selectionPile = null)
	{
		CardPile sourcePile = selectionPile.sourcePile;
		List<Card> cards = selectionPile.cards;

		// Try foundation
		if (cards.Count == 1 && toPile == sourcePile)
			toPile = foundations[(int)cards[0].suit];

		List<Card> movedCards = new List<Card>();

		if (toPile.Type == PileType.Foundation)
		{
			TryFoundationMove(toPile, cards, movedCards);
		}
		else if (toPile.Type == PileType.Tableau)
        {
            TryTableuMove(toPile, cards, movedCards);
        }

        if (movedCards.Count > 0 && !IsHintActive())
		{
			commands.Clear();

			MoveCards(movedCards, sourcePile, toPile);

			if (sourcePile.Type == PileType.Tableau)
				TurnCard(sourcePile.GetLastCard());

			//TurnLastTableauCards();
		}

		return movedCards.Count > 0;
	}

	private void TryFoundationMove(CardPile toPile, List<Card> cards, List<Card> movedCards)
	{
		if (cards.Count > 1)
			return;

		Card selectedCard = cards[0];
		Card lastCard = toPile.GetLastCard();

		if (lastCard != null && lastCard.suit != selectedCard.suit)
			return;

		int lastCardNum = lastCard != null ? lastCard.number : 0;

		if (selectedCard.number == lastCardNum + 1)
		{
			movedCards.Add(selectedCard);

			if (!IsHintActive())
				NotificationCenter.DefaultCenter.PostNotification(this, GameEvents.FoundationMoveDone, iTween.Hash("suit", selectedCard.suit));
		}
	}

	private void TryTableuMove(CardPile toPile, List<Card> cards, List<Card> movedCards)
    {
        foreach (CardPile tableauPile in TableauPiles)
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
                foreach (Card c in cards)
                {
                    movedCards.Add(c);
                }
                break;
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
				if (pileType == PileType.Waste)
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
