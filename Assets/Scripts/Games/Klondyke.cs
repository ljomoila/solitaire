using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

public class Klondyke : SolitaireGame
{
	// TODO position piles according to camera position, no need for these after that
	public Transform wasteHolder, pileauHolder, foundHolder;
	
	public PileSlot pileSlot, foundationSlot;

	private List<CardPile> foundations;

	private float xStep = 3.5f;
	private int stockDrawAmount = 3;

	private List<Cmd> commands = new List<Cmd>();

    private void Awake()
    {
		gameType = GameType.Klondyke;

		DealState = gameObject.AddComponent<DealStateKlondyke>();
    }
	
	public override IEnumerator Initialize()
	{
		waste = new GameObject("KlondykeWaste").AddComponent<CardPile>();
		waste.transform.parent = transform;
		waste.transform.position = wasteHolder.position;
		waste.zStep = stock.zStep;
		waste.Type = CardPileType.KlondikeWaste;
		allPiles.Add(waste);
		AllowedPiles.Add(waste);
		
		PileSlot wasteSlot = Instantiate(pileSlot, waste.transform.position, Quaternion.identity, waste.transform);
		wasteSlot.Initialize(waste);
		
		tableauPiles = new List<CardPile>();
		
		for (int i = 0; i < 7; i++)
        {
			CardPile pile = new GameObject("Tableu "+i).AddComponent<CardPile>();
			pile.gameObject.name = "KlondykeTableu " + (i + 1);
			pile.transform.parent = transform;
			pile.transform.position = new Vector3(pileauHolder.position.x + (xStep * i), pileauHolder.position.y, pileauHolder.position.z);
			pile.Type = CardPileType.Tableau;
			pile.yStep = .65f;
			pile.yStepTurned = .3f;			
			pile.cardCount = i+1;
			
			tableauPiles.Add(pile);
			
			PileSlot slot = Instantiate(pileSlot, pile.transform.position, Quaternion.identity, pile.transform);
			slot.Initialize(pile);

			tableauPiles[i].slot = slot;
			
			allPiles.Add(pile);
			AllowedPiles.Add(pile);
		}

		foundations = new List<CardPile>();

		for (int i = 0; i < 4; i++)
		{
			CardPile foundation = new GameObject("Foundation " + (CardSuit)i).AddComponent<CardPile>();
			foundation.Type = CardPileType.Foundation;
			foundation.transform.parent = transform;
			foundation.transform.position = new Vector3(foundHolder.position.x + (xStep * i), foundHolder.position.y, foundHolder.position.z);
			foundation.CardSuit = (CardSuit)i;

			foundations.Add(foundation);

			FoundationSlot slot = Instantiate(foundationSlot, foundation.transform.position, Quaternion.identity, foundation.transform) as FoundationSlot;
			slot.Initialize((CardSuit)i, foundation);

			foundation.slot = slot;

			allPiles.Add(foundation);
			AllowedPiles.Add(foundation);
		}

		yield return null;
	}

	public override List<Card> Select(Card c)
	{
		CardPile pile = c.Pile;
		List<Card> selection = new List<Card>();

        if (pile.Type == CardPileType.Stock)
        {
            CmdDrawCards cmd = new CmdDrawCards(this, stockDrawAmount, "Stock draw");
            cmd.Execute(false);

            GameManager.Instance.StoreCommand(cmd);
			AudioController.Play("cardSlide");
		} 
        else if (pile.Type == CardPileType.KlondikeWaste)
		{
            if (!pile.IsEmpty)
				selection = new List<Card>() { pile.GetLastCard() };            
        }		
		else if (pile.Type == CardPileType.Foundation)
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
            foreach(CardPile tableauPile in tableauPiles)
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

	public override bool TryMove(CardPile toPile, SelectionPile selectionPile, bool hint = false)
	{
		commands.Clear();

		List<Card> cards = selectionPile.cards;
		CardPile sourcePile = selectionPile.sourcePile;

		// single click -> try foundation move
		if (cards.Count == 1 && toPile == sourcePile)
			toPile = foundations[(int)cards[0].suit];

		List<Card> movedCards = new List<Card>();

		if (toPile.Type == CardPileType.Foundation)
		{
			if (cards.Count > 1)
				return false;

            Card selectedCard = cards[0];
			Card lastCard = toPile.GetLastCard();

			if (lastCard != null && lastCard.CardSuit != selectedCard.CardSuit)
				return false;

			int lastCardNum = lastCard != null ? lastCard.Number : 0;

            if (selectedCard.Number == lastCardNum + 1)
            {
				movedCards.Add(selectedCard);

				if (!hint)
					NotificationCenter.DefaultCenter.PostNotification(this, GameEvents.FoundationMoveDone, iTween.Hash("suit", selectedCard.suit));
			}
		}		
		else if (toPile.Type == CardPileType.Tableau)
		{
			foreach(CardPile tableauPile in tableauPiles)			
            {
				if (toPile != tableauPile) continue;

				Card firstCard = cards[0];
				bool move = false;

				if (toPile.cards.Count == 0 && firstCard.Number == 13)
				{
					move = true;				
				}
				else
				{
					Card lastPileCard = tableauPile.GetLastCard();	

					if (lastPileCard == null) continue;				

					if (lastPileCard.Number - firstCard.Number == 1 && Card.IsDifferentSuit(firstCard, lastPileCard))
						move = true;				
				}
				if (move) 
				{
					foreach(Card c in cards)
					{
						movedCards.Add(c);
					}	
				}
            }
		}	
		if (movedCards.Count > 0)
		{
			if (hint)
            {
				ShowHint(cards[0].Pile);
			}
			else
            {
				StoreMoveCommand(movedCards, sourcePile, toPile);
				UpdateLastCards();

				GameManager.Instance.StoreCommand(new CmdComposite(commands));
			}			
		}

		return movedCards.Count > 0;
	}

	void StoreMoveCommand(List<Card> movedCards, CardPile sourcePile, CardPile targetPile)
    {
		CmdMoveCards cmdMoveCards = new CmdMoveCards(movedCards, sourcePile, targetPile, "Move cards to: " + targetPile.Type);
		cmdMoveCards.Execute(false);
		commands.Add(cmdMoveCards);
	}
	
	void UpdateLastCards()
    {
        // Turn last cards after movement
        foreach (CardPile pile in tableauPiles)
        {
            Card lastCard = pile.GetLastCard();

            if (lastCard != null && lastCard.IsTurned())
            {
                CmdTurnCard cmdTurn = new CmdTurnCard(lastCard, "Update last card move Klondyke") { Animate = true };
                cmdTurn.Execute(false);
                commands.Add(cmdTurn);
            }
        }
    }
	
	public override void HintRequest()
	{
		List<Card> hintCards = new List<Card>();		

		foreach(CardPile pile in AllowedPiles) 
		{
			hintCards.Clear();

			foreach(Card card in pile.cards)
			{				
				if (!card.IsTurned())
				{
					hintCards.Add(card);
					
					if (pile.Type == CardPileType.KlondikeWaste)
					{
						hintCards.Clear();
						hintCards.Add(pile.GetLastCard());
					}
				}
			}
			if (hintCards.Count > 0)
			{
				SelectionPile hintPile = new SelectionPile();
				hintPile.cards = hintCards;

				foreach(CardPile toPile in AllowedPiles)
				{
					if (TryMove(toPile, hintPile, true))
					{
						ShowHint(hintCards[0].Pile);
						return;
					}
				}
			}
		}
	}
	
	void ShowHint(CardPile pile)
	{
		// no hint when theres only one card in foundation
		if (pile.Type == CardPileType.Foundation && pile.cards.Count == 1)
			return;        
		
		List<Card> hintCards = new List<Card>();
		
		if (pile.Type == CardPileType.Tableau)
		{
			for (int i = 0; i < pile.cards.Count; i++)
			{
				Card c = pile.cards[i];
				
				if (!c.IsTurned())
				{
					hintCards.Add(c);
				}
			}
		}
		else // waste or foundation
		{			
			hintCards.Add(pile.GetLastCard());
		}
		
		for (int j = 0; j < hintCards.Count; j++)
		{
			Card c = hintCards[j];

			// TODO make one single animation
			c.sprite.GetComponent<Animation>().Play("cardHint_01", AnimationPlayMode.Stop);
			c.sprite.GetComponent<Animation>().Play("cardHint_01", AnimationPlayMode.Queue);
		}			
	}
	
	public override IEnumerator RestoreState(XDocument xdoc)
	{
		XElement piles = xdoc.Root.Element("piles");
		
		int fcount = 0;
		int tcount = 0;
		
		foreach (XElement pileNode in piles.Elements("pile"))
		{
			CardPileType pileType = (CardPileType) System.Enum.Parse(typeof(CardPileType), pileNode.Element("type").Value);
			
			CardPile cpile = null;

			if (pileType == CardPileType.Stock)
			{
				cpile = stock;
			}
			else
			{
				if (pileType == CardPileType.KlondikeWaste)
				{
					cpile = waste;
				}
				if (pileType == CardPileType.Foundation)
				{
					cpile = foundations[fcount];
					fcount++;
				}
				if (pileType == CardPileType.Tableau)
				{
					cpile = tableauPiles[tcount];
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
					CardSuit suit = (CardSuit) System.Enum.Parse(typeof(CardSuit), cardNode.Element("suit").Value);   

					Card c = GetCard(number, suit);
					c.Pile = cpile;
					c.Turn(System.Convert.ToBoolean(cardNode.Element("turned").Value));
					
					cpile.AddCard(c);
				}
			}
			
			cpile.AlignCards();
		}
		
		yield return null;
	}
}
