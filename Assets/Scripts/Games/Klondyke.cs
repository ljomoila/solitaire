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
		AllPiles.Add(waste);
		AllowedPiles.Add(waste);
		
		PileSlot wasteSlot = Instantiate(pileSlot, waste.transform.position, Quaternion.identity, waste.transform);
		wasteSlot.Initialize(waste);
		
		TableauPiles = new List<CardPile>();
		
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
			
			TableauPiles.Add(pile);
			
			PileSlot slot = Instantiate(pileSlot, pile.transform.position, Quaternion.identity, pile.transform);
			slot.Initialize(pile);

			TableauPiles[i].slot = slot;
			
			AllPiles.Add(pile);
			AllowedPiles.Add(pile);
		}

		foundations = new List<CardPile>();

		for (int i = 0; i < 4; i++)
		{
			CardPile foundation = new GameObject("KlondykeFoundation " + (CardSuit)i).AddComponent<CardPile>();
			foundation.Type = CardPileType.Foundation;
			foundation.transform.parent = transform;
			foundation.transform.position = new Vector3(foundHolder.position.x + (xStep * i), foundHolder.position.y, foundHolder.position.z);
			foundation.CardSuit = (CardSuit)i;

			foundations.Add(foundation);

			FoundationSlot slot = Instantiate(foundationSlot, foundation.transform.position, Quaternion.identity, foundation.transform) as FoundationSlot;
			slot.Initialize((CardSuit)i, foundation);

			foundation.slot = slot;

			AllPiles.Add(foundation);
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

				if (!(activeState is HintState))
					NotificationCenter.DefaultCenter.PostNotification(this, GameEvents.FoundationMoveDone, iTween.Hash("suit", selectedCard.suit));
			}
		}		
		else if (toPile.Type == CardPileType.Tableau)
		{
			foreach(CardPile tableauPile in TableauPiles)			
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
					break;
				}
            }
		}	
		if (movedCards.Count > 0)
		{
			if (!(activeState is HintState))
            {
				commands.Clear();
				StoreMoveCommand(movedCards, sourcePile, toPile);
				UpdateTableuPiles();

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
	
	void UpdateTableuPiles()
    {
        // Turn last cards after movement
        foreach (CardPile pile in TableauPiles)
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
