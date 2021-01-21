using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

public class Klondyke : SolitaireGame
{
	// TODO position piles according to camera position
	public Transform wasteHolder, pileauHolder, foundHolder;

	private CardSelectionState cardSelectionState;	
	private List<CardPile> foundations, pileaus;

	public PileSlot pileSlot, foundationSlot;

	private float xStep = 3.5f;
	private int stockDrawAmount = 3;


	// Use this for initialization
	void Start () 
	{
		gameType = GameType.Klondyke;

		StartCoroutine(Initialize());
	}
	
	public override IEnumerator Initialize()
	{
		stock.rnd = new Random();
		
		waste = new GameObject("KlondykeWaste").AddComponent<CardPile>();
		waste.transform.parent = transform;
		waste.transform.position = wasteHolder.position;
		waste.zStep = stock.zStep;
		waste.Type = CardPileType.KlondikeWaste;
		allPiles.Add(waste);
		AllowedPiles.Add(waste);
		
		PileSlot wasteSlot = Instantiate(pileSlot, waste.transform.position, Quaternion.identity, waste.transform);
		wasteSlot.Initialize(waste);
		
		foundations = new List<CardPile>();		
		int i = 0;
		
        for (i = 0; i < 4; i++)
        {
			CardPile foundation = new GameObject("Foundation "+(CardSuit)i).AddComponent<CardPile>();
			foundation.Type = CardPileType.Foundation;
			foundation.transform.parent = transform;
			foundation.transform.position = new Vector3(foundHolder.position.x + (xStep*i), foundHolder.position.y, foundHolder.position.z);
			foundation.CardSuit = (CardSuit)i;
			
			foundations.Add(foundation);
			
			FoundationSlot slot = Instantiate(foundationSlot, foundation.transform.position, Quaternion.identity, foundation.transform) as FoundationSlot;
			slot.Initialize((CardSuit)i, foundation);			
			
			foundation.slot = slot;
			
			allPiles.Add(foundation);
			AllowedPiles.Add(foundation);
        }
		
		pileaus = new List<CardPile>();
		
		for (i = 0; i < 7; i++)
        {
			CardPile pile = new GameObject("Tableu "+i).AddComponent<CardPile>();
			pile.gameObject.name = "KlondykeTableu " + (i + 1);
			pile.transform.parent = transform;
			pile.transform.position = new Vector3(pileauHolder.position.x + (xStep * i), pileauHolder.position.y, pileauHolder.position.z);
			pile.Type = CardPileType.Tableau;
			pile.yStep = .65f;
			pile.yStepTurned = .3f;			
			pile.cardCount = i+1;
			
			pileaus.Add(pile);
			
			PileSlot slot = Instantiate(pileSlot, pile.transform.position, Quaternion.identity, pile.transform);
			slot.Initialize(pile);

			pileaus[i].slot = slot;
			
			allPiles.Add(pile);
			AllowedPiles.Add(pile);
		}

		yield return null;
	}
	
	public override IEnumerator ShuffleAndDeal()
	{
		yield return StartCoroutine(stock.Shuffle());

		yield return StartCoroutine(Deal());
	}

    private IEnumerator Deal()
    {
		float t = .25f;
		int pileCount = 0;
		int j = 0;

		for (int i = 0; i < 28; i++)
		{
			Card card = stock.GetFirstCard();
			card.Pile = pileaus[pileCount];
			card.transform.parent = card.Pile.transform;

			card.Pile.AddCardDeal(card, t, i * .03f);
			AudioController.Play("cardSlide", 1, i * .03f);

			if (pileCount > 0 && pileCount % 6 == 0)
			{
				j++;
				pileCount = j;

				yield return new WaitForSeconds(t);
			}
			else
			{
				pileCount++;
			}
		}

		yield return new WaitForSeconds(27 * .03f);

		// TODO turn last cards while dealing
		foreach (CardPile pileau in pileaus)
		{
			Card c = pileau.GetLastCard();
			c.Turn(false, .05f);

			AudioController.Play("cardTurn", 1, .025f);

			yield return new WaitForSeconds(.025f);
		}
	}

    Vector3 startHit;
	
	// Update is called once per frame
	void Update () 
	{
		RaycastHit hit = new RaycastHit();
		Ray ray = GameManager.Instance.MainCam.ScreenPointToRay(Input.mousePosition);

		if (Input.GetMouseButtonDown(0))
		{
			if (Physics.Raycast(ray, out hit, Mathf.Infinity))
			{
				Card c = hit.collider.gameObject.GetComponent<Card>();
				iTween[] tweens = hit.collider.gameObject.GetComponents<iTween>();
				
				if (c == null || tweens.Length > 0)
					return;
				
				startHit = hit.point;
				
	            TrySelecting(c);
			}
		}
	}
	
	List<Cmd> commands = new List<Cmd>();
	
	public override bool TryMove(CardPile toPile, List<Card> cards, bool hint = false)
	{
		commands.Clear();

		if (toPile == cardSelectionState.Pile.sourcePile && cards.Count == 1)
		{
			toPile = foundations[(int)cards[0].suit];
		}

		List<Card> movedCards = new List<Card>();

		if (toPile.Type == CardPileType.Foundation)
		{
            Card selectedCard = cards[0];
			Card lastCard = toPile.GetLastCard();
			int lastNum = 0;
            
            if (lastCard != null)
            {
            	lastNum = lastCard.number;
            }
			
            if (selectedCard.Number == lastNum + 1)
            {
				movedCards.Add(cards[0]);

				if (!hint)
					NotificationCenter.DefaultCenter.PostNotification(this, GameEvents.FoundationMoveDone, iTween.Hash("suit", movedCards[0].suit));			
			}
		}
		
		else if (toPile.Type == CardPileType.Tableau)
		{
			for (int i = 0; i < pileaus.Count; i++)
            {
				if (toPile == pileaus[i])
                {
					if (toPile.cards.Count == 0)
					{
	                    Card firstCard = cards[0];
	
	                    if (firstCard.Number == 13)
						{
							foreach(Card c in cards)
							{
								movedCards.Add(c);
							}
	                    }
	                }
					else
					{
						Card topMost = cards[0];
                        Card pileCard = pileaus[i].GetLastCard();

                        if (pileCard.Number - topMost.Number == 1 && Card.IsDifferentSuit(topMost, pileCard))
                        {
							foreach(Card c in cards)
							{
								movedCards.Add(c);
							}
                        }
	                   
					}
				}
            }
		}
			
		
		if (movedCards.Count > 0)
		{
			StoreMoveCommand(movedCards, toPile, hint);
			UpdateLastCards();

			if (hint)
            {
				NotificationCenter.DefaultCenter.PostNotification(this, GameEvents.ShowHint, iTween.Hash("pile", cards[0].Pile));				
			}

			GameManager.Instance.StoreCommand(new CmdComposite(commands));
		}

		return movedCards.Count > 0;
	}

	void StoreMoveCommand(List<Card> movedCards, CardPile targetPile, bool hint = false)
    {
		if (hint) return;

		CmdMoveCards cmdMoveToFoundation = new CmdMoveCards(movedCards, cardSelectionState.Pile.sourcePile, targetPile, "Move cards to: " + targetPile.Type);
		cmdMoveToFoundation.Execute(false);
		commands.Add(cmdMoveToFoundation);
	}
	
	void UpdateLastCards()
    {
        // Turn last cards
        foreach (CardPile pile in pileaus)
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
	
	void TrySelecting(Card c)
	{
		CardPile pile = c.Pile;

		Debug.Log(pile + " " + c);

        if (pile.Type == CardPileType.Stock)
        {
            CmdDrawCards cmd = new CmdDrawCards(this, stockDrawAmount, "Stock draw");
            cmd.Execute(false);

            GameManager.Instance.StoreCommand(cmd);
			AudioController.Play("cardSlide");
		} 
        else if (pile.Type == CardPileType.KlondikeWaste)
		{
            if (pile.IsEmpty == false)
            {
                Card lastWasteCard = pile.GetLastCard();
				
				Select(new List<Card>() { lastWasteCard });
            }
            return;
        }		
		else if (pile.Type == CardPileType.Foundation)
		{
            for (int i = 0; i < foundations.Count; i++)
            {
                if (pile == foundations[i])
				{
                    List<Card> selection = new List<Card>();

                    if (pile.cards.Count > 0)
                    {
                        int startIndex = pile.cards.IndexOf(c);

                        for (int j = startIndex; j < pile.cards.Count; j++)
                        {
                            Card foundationCard = pile.GetCard(j);

                            if (foundationCard != null && foundationCard.IsTurned() == false)
                            {
                                selection.Add(foundationCard);
                            }
                        }
                    }

                    if (selection.Count > 0)
                    {
						Select(selection);
                    }

                    return;
                }
            }
		}
		else // tableu piles
		{
            for (int i = 0; i < 7; i++)
            {
                if (pile == pileaus[i])
				{
                    List<Card> selection = new List<Card>();

                    if (pile.cards.Count > 0)
                    {
                        int startIndex = pile.cards.IndexOf(c);

                        for (int j = startIndex; j < pile.cards.Count; j++)
                        {
                            Card tableauCard = pile.GetCard(j);

                            if (tableauCard != null && tableauCard.IsTurned() == false)
                            {
                                selection.Add(tableauCard);
                            }
                        }
                    }
                    if (selection.Count > 0)
                    {
						Select(selection);
                    }
                    return;
                }
            } 
		}
	}
	
	void Select(List<Card> cards)
	{
		if (cards.Count == 0) return;

		if (cardSelectionState == null)
			cardSelectionState = new GameObject("SelectionPile").AddComponent<CardSelectionState>();

		cardSelectionState.Initialize(cards, startHit);
		StateManager.Instance.ActivateState(cardSelectionState);

		AudioController.Play("cardSlide");

	}
	
	public override void HintRequest()
	{
		List<Card> hintCards = new List<Card>();
		
		for (int i = 0; i < AllowedPiles.Count; i++)
		{
			CardPile pile = AllowedPiles[i];

			hintCards.Clear();
			
			for (int j = 0; j < pile.cards.Count; j++)
			{				
				Card card = pile.cards[j];
				
				if (!card.IsTurned())
				{
					hintCards.Add(card);
					
					if (pile.Type == CardPileType.KlondikeWaste)
					{
						hintCards.Clear();
						hintCards.Add(pile.GetLastCard());
					}
				}
				
				if (hintCards.Count > 0)
				{
					for (int h = 0; h < AllowedPiles.Count; h++)
					{
						CardPile toPile = AllowedPiles[h];
						
						if (TryMove(toPile, hintCards, true))
						{
							return;	
						}
					}
				}
			}
		}
	}
	
	public override void ShowHint(NotificationCenter.Notification n)
	{
		CardPile pile = n.data["pile"] as CardPile;

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
		else
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
					cpile = pileaus[tcount];
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
