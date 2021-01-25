using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System;

public class Golf : Game
{
	// TODO position piles according to camera position, no need for these after that
	public Transform wasteHolder, pileauHolder;
	
	public PileSlot pileSlot;

	public float xStep = 3.5f;

	private void Awake()
	{
		dealState = gameObject.AddComponent<DealStateGolf>();
		hintState = gameObject.AddComponent<HintGolf>();
	}

    public override IEnumerator Initialize()
	{
		Waste = new GameObject("GolfWaste").AddComponent<CardPile>();
		Waste.transform.parent = transform;
		Waste.transform.position = wasteHolder.position;
		Waste.zStep = stock.zStep;
		Waste.Type = PileType.Waste;
		Piles.Add(Waste);
		
		PileSlot wasteSlot = Instantiate(pileSlot, Waste.transform.position, Quaternion.identity, Waste.transform);
		wasteSlot.Initialize(Waste);
		
		for (int i = 0; i < 7; i++)
        {
			CardPile pile = new GameObject("GolfTableu "+ i).AddComponent<CardPile>();
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

		yield return null;
	}

	public override List<Card> SelectCards(Card card)
	{
		CardPile pile = card.pile;

        if (pile.Type == PileType.Stock)
        {
			if (stock.cards.Count == 0)
				return null;            

			DrawCards(1);
		} 
        else if (pile.Type == PileType.Tableau)
		{
			TryMoveToPile(card.pile);
		}

		return null;
	}

	public override bool TryMoveToPile(CardPile pile, SelectionPile selectionPile = null)
	{
		Card card = pile.GetLastCard();
		Card lastWasteCard = Waste.GetLastCard();		

		if (card == null || lastWasteCard == null) return false;

		if (Math.Abs(lastWasteCard.number - card.number) == 1)
		{
			if (IsHintActive()) return true;

			MoveCards(new List<Card> { card }, card.pile, Waste);

			// TODO effect
			//NotificationCenter.DefaultCenter.PostNotification(this, GameEvents.FoundationMoveDone, iTween.Hash("suit", selectedCard.suit));

			if (Waste.cards.Count == 52)
				State = GameState.Solved;

			return true;
		}

		return false;
	}

	public override IEnumerator RestoreState(XDocument xdoc)
	{
		XElement piles = xdoc.Root.Element("piles");
		
		int tcount = 0;
		
		foreach (XElement pileNode in piles.Elements("pile"))
		{
			PileType pileType = (PileType) Enum.Parse(typeof(PileType), pileNode.Element("type").Value);
			
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
					Suit suit = (Suit) Enum.Parse(typeof(Suit), cardNode.Element("suit").Value);   

					Card c = stock.GetCard(number, suit);
					c.pile = cpile;
					c.Turn(Convert.ToBoolean(cardNode.Element("turned").Value));
					
					cpile.AddCard(c);
				}
			}
			
			cpile.AlignCards();
		}
		
		yield return null;
	}
}
