using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

public class Klondyke : SolitaireGame
{
	List<CardPile> foundations, pileaus;
	
	public Transform wasteHolder, pileauHolder, foundHolder;
	
	public float xStep = 1;

	// Use this for initialization
	void Start () 
	{
		gameType = GameType.Klondyke;

		StartCoroutine(Initialize());
	}
	
	public override IEnumerator Initialize()
	{
		CardPile deck = GameManager.Instance.deckMaker.deck;
		PileSlot slot = GameManager.Instance.deckMaker.slotDeco;

		stock = Instantiate(deck, Vector3.zero, Quaternion.identity, transform);
		stock.gameObject.name = "KlondykeStock";
		stock.transform.position = stockHolder.position;
		stock.zStep = 0.015f;
		stock.rnd = new Random();
		
		PileSlot stockSlot = Instantiate(slot, stock.transform.position, Quaternion.identity, stock.transform);
		stockSlot.pileType = CardPileType.Stock;
		stockSlot.card.pile = stock;
		
		waste = new GameObject("KlondykeWaste").AddComponent<CardPile>();
		waste.transform.parent = transform;
		waste.transform.position = wasteHolder.position;
		waste.zStep = 0.015f;
		waste.Type = CardPileType.KlondikeWaste;
		waste.AlignCards();
		allPiles.Add(waste);
		allowedPiles.Add(waste);
		
		PileSlot wasteSlot = Instantiate(slot, waste.transform.position, Quaternion.identity, waste.transform);
		wasteSlot.pileType = CardPileType.KlondikeWaste;
		wasteSlot.card.pile = waste;
		
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
			
			PileSlot s = Instantiate(slot, foundation.transform.position, Quaternion.identity, foundation.transform);
			s.pileType = CardPileType.Foundation;
			s.suit = (CardSuit)i;
			s.card.pile = foundation;
			s.transform.Translate(0, 0, .05f);
			
			foundation.slot = s;
			
			allPiles.Add(foundation);
			allowedPiles.Add(foundation);
        }
		
		pileaus = new List<CardPile>();
		
		for (i = 0; i < 7; i++)
        {
			CardPile pileau = new GameObject("Tableu "+i).AddComponent<CardPile>();
			pileau.transform.parent = transform;
			pileau.gameObject.name = "KlondykeTableu "+(i+1);
			pileau.Type = CardPileType.Tableau;
			pileau.yStep = .65f;
			pileau.yStepTurned = .3f;
			pileau.transform.position = new Vector3(pileauHolder.position.x + (xStep*i), pileauHolder.position.y, pileauHolder.position.z);
			pileau.cardCount = i+1;
			
			pileaus.Add(pileau);
			
			PileSlot s = Instantiate(slot, pileau.transform.position, Quaternion.identity, pileau.transform);
			s.pileType = CardPileType.Tableau;
			s.card.pile = pileau;
			pileaus[i].slot = s;
			
			allPiles.Add(pileau);
			allowedPiles.Add(pileau);
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

		j = 0;

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
		if (/*GameManager.Instance.state != GameState.Running || */GameManager.Instance.UICheck())
		{
			return;
		}
		
		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit hit = new RaycastHit();
			Ray ray = GameManager.Instance.MainCam.ScreenPointToRay(Input.mousePosition);
			
			if (Physics.Raycast(ray, out hit, Mathf.Infinity))
			{
				Card c = hit.collider.gameObject.GetComponent<Card>();
				iTween[] tweens = hit.collider.gameObject.GetComponents<iTween>() as iTween[];
				
				if (c == null || tweens.Length > 0)
					return;
				
				startHit = hit.point;
				
	            TrySelecting(c);
				
				AudioController.Play("cardSlide");
			}
		}
		
		if (cardSelection != null && !cardSelection.IsEmpty)
		{
			if (Input.GetMouseButton(0))
			{
				RaycastHit hit = new RaycastHit();
				Ray ray = GameManager.Instance.MainCam.ScreenPointToRay(Input.mousePosition);
				
				if (Physics.Raycast(ray, out hit, Mathf.Infinity))
				{
					Vector3 pos = hit.point - startHit;
					pos.z = -0.5f;

					//cardSelection.mover.transform.position = Vector3.Lerp(cardSelection.mover.transform.position, pos,Time.deltaTime * 10);
					cardSelection.transform.position = Vector3.Lerp(cardSelection.transform.position, pos, Time.deltaTime * 10);
				}
				
				if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 9))
				{
					Card c = hit.collider.gameObject.GetComponent<Card>();
					
					if (c != null && c.Pile != null && c.Pile != cardSelection)
					{
						CardPile pile = c.Pile;
						
						for (int i = 0; i < allowedPiles.Count; i++)
						{
							if (allowedPiles[i] == pile)
							{
								pile.Highlight();
							}
							else
							{
								allowedPiles[i].Unhighlight();
							}
						}
					}
				}
			}			
			if (Input.GetMouseButtonUp(0))
			{
                AudioController.Play("cardSlide");

                RaycastHit hit = new RaycastHit();
                Ray ray = GameManager.Instance.MainCam.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 9))
                {
                    Card c = hit.collider.gameObject.GetComponent<Card>();

                    if (c != null && c.Pile != null && c.Pile != cardSelection)
                    {
                        CardPile pile = c.Pile;

                        if (c.Pile == cardSelection.sourcePile && cardSelection.cards.Count == 1)
                        {
                            pile.Unhighlight();
                            pile = foundations[(int)cardSelection.cards[0].suit];

                        }

                        pile.Unhighlight();

                        TryMove(pile);
                        return;
                    }
                }

                CancelMove();

                for (int i = 0; i < allowedPiles.Count; i++)
                {
                    allowedPiles[i].Unhighlight();
                }

            }
		}
	}
	
	List<Cmd> commands = new List<Cmd>();
	
	bool TryMove(CardPile toPile, List<Card> cards = null, bool hint = false)
	{
		commands.Clear();
		
		if (cards == null)
			cards = cardSelection.cards;
		
		List<Card> movedCards = new List<Card>();
		
		if (toPile.Type == CardPileType.Foundation)
		{
            Card selectedCard = cards[0];			
            CardPile targetPile = toPile;
			Card lastCard = targetPile.GetLastCard();
			int lastNum = 0;
            
            if (lastCard != null)
            {
            	lastNum = lastCard.number;
            }
			
            if (selectedCard.Number == lastNum + 1)
            {
				movedCards.Add(cards[0]);
				
				if (hint)
				{
					NotificationCenter.DefaultCenter.PostNotification(this, GameEvents.ShowHint, iTween.Hash("pile", cards[0].pile));
					return true;
				}
				
				
                CmdMoveCards cmdMoveToFoundation = new CmdMoveCards(movedCards, cardSelection.sourcePile, targetPile, "Foundation move Klondyke");
                cmdMoveToFoundation.Execute(false);
                commands.Add(cmdMoveToFoundation);

                UpdateLastCards();
				
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
							
							if (hint)
							{
								NotificationCenter.DefaultCenter.PostNotification(this, GameEvents.ShowHint, iTween.Hash("pile", cards[0].pile));
								return true;
							}
							
                            // Move to tableau
                            CmdMoveCards cmdMoveToTableau = new CmdMoveCards(movedCards, cardSelection.sourcePile, toPile, "Tableau move Klondyke (King)");
                            cmdMoveToTableau.Execute(false);
                            commands.Add(cmdMoveToTableau);

                            UpdateLastCards();
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
							
							if (hint)
							{
								NotificationCenter.DefaultCenter.PostNotification(this, GameEvents.ShowHint, iTween.Hash("pile", cards[0].pile));
								return true;
							}
							
                            // Move to tableau
                            CmdMoveCards cmdMoveToTableau = new CmdMoveCards(movedCards, cardSelection.sourcePile, pileaus[i], "Tableau move Klondyke");
                            cmdMoveToTableau.Execute(false);
                            commands.Add(cmdMoveToTableau);

                            UpdateLastCards();
                        }
	                   
					}
				}
            }
		}
		
		if (hint)
			return false;
		
		if (movedCards.Count == 0)
		{
			CancelMove();
		}
		else
		{
			GameManager.Instance.StoreCommand(new CmdComposite(commands));
		}
		
		if (cardSelection != null)
		{
			cardSelection.Clear();			
			cardSelection = null;
		}
		
		return false;
	}
	
	void UpdateLastCards()
    {
        // Turn last cards
        for (int k = 0; k < 7; k++)
        {
            Card lastCard = pileaus[k].GetLastCard();

            if (lastCard != null && lastCard.IsTurned())
            {
                CmdTurnCard cmdTurn = new CmdTurnCard(lastCard, "Update last card move Klondyke") { Animate = true };
                cmdTurn.Execute(false);
                commands.Add(cmdTurn);
            }
        }
    }
	
	void CancelMove()
	{
		if (cardSelection == null)
			return;
		
		CardPile toPile = cardSelection.sourcePile;
		
		cardSelection.cards[0].Leave();
		
		foreach(Card c in cardSelection.cards)
		{
			c.transform.parent = toPile.transform;
			toPile.AddCard(c, .5f, 0);
			//c.Pile = toPile;
		}
		
		toPile.AlignCards();
		
		Destroy (cardSelection.gameObject);
		cardSelection = null;
	}
	
	void TrySelecting(Card c)
	{
		CardPile pile = c.Pile;

        if (pile == stock)
        {
            CmdDrawCards cmd = new CmdDrawCards(this, 3);
            cmd.Execute(false);

            GameManager.Instance.StoreCommand(cmd);
        } 
        else if (pile == waste)
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
	    if (cards.Count > 0)
	    {
			cardSelection = new GameObject("SelectionPile").AddComponent<SelectionPile>();
			cardSelection.transform.position = cards[0].transform.position;
			cardSelection.sourcePile = cards[0].Pile;
			cardSelection.yStep = cards[0].Pile.yStep;	        
			
			int i = 0;
	        foreach (Card card in cards)
	        {
	            cardSelection.AddCard(card);
				card.Pick(i);

				i++;
	        }
			cardSelection.AlignCards();					
			
			startHit.y -= cards[0].transform.position.y;
			startHit.x -= cards[0].transform.position.x;
			startHit.z = 0;

			//iTween.MoveBy(cardSelection.gameObject, iTween.Hash("z", -.45f, "time", .5f, "isLocal", true));
			//cardSelection.MakeDragGroup();
		}
	}
	
	public override void HintRequest()
	{
		List<Card> hintCards = new List<Card>();
		
		for (int i = 0; i < allowedPiles.Count; i++)
		{
			CardPile pile = allowedPiles[i];

			if (pile.Type == CardPileType.Foundation)
				continue;

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
					for (int h = 0; h < allowedPiles.Count; h++)
					{
						CardPile toPile = allowedPiles[h];
						
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
