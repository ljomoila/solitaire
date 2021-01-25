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

    private void Start()
    {
		gameType = GameType.Golf;
		stockDrawAmount = 1;
    }

    public override IEnumerator SetupTable()
	{
		Waste = new GameObject("GolfWaste").AddComponent<CardPile>();
		Waste.transform.parent = transform;
		Waste.transform.position = wasteHolder.position;
		Waste.zStep = stock.zStep;
		Waste.Type = PileType.Waste;
		Piles.Add(Waste);
		
		PileSlot wasteSlot = Instantiate(pileSlot, Waste.transform.position, Quaternion.identity, Waste.transform);
		wasteSlot.Initialize(Waste);
		
		TableauPiles = new List<CardPile>();
		
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
			DrawCards();
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
			if (hintMode) return true;

			MoveCards(new List<Card> { card }, card.pile, Waste);

			if (Waste.cards.Count == 52)
			{
				Debug.Log("Golf solved");
				// TODO
			}

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

	public override IEnumerator Deal()
	{
		List<CardPile> piles = TableauPiles;

		float dealTime = 0;

		foreach (CardPile pile in piles)
		{
			for (int i = 0; i < 5; i++)
			{
				Card card = stock.GetFirstCard();
				card.pile = pile;
				card.transform.parent = pile.transform;

				DealCard(card, dealTime);

				yield return new WaitForSeconds(.1f);

				card.Turn(false);

				AudioController.Play("cardSlide", 1, dealTime);

				yield return new WaitForSeconds(dealTime);

				pile.AlignCards();

				dealTime = i * 0.01f;
			}
		}

		// Turn first card to waste
		DrawCards();
	}

	public override void HintRequest()
	{
		base.HintRequest();

		foreach (CardPile pile in TableauPiles)
		{
			if (TryMoveToPile(pile))
			{
				ShowHint(pile.GetLastCard());
				break;
			}
		}

		hintMode = false;
	}
}
