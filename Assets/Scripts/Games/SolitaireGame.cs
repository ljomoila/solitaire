
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

public class SolitaireGame : MonoBehaviour
{
	public GameType gameType;

	public Transform stockHolder;	

	public CardPile stock, waste;	
	
	public int seed = 123456;

	public List<CardPile> allPiles = new List<CardPile>();

    public List<CardPile> AllowedPiles { get; set; } = new List<CardPile>();

    // Use this for initialization
    void Start ()
	{
		
	}

	void Update()
	{

	}

	public virtual IEnumerator Initialize()
	{
		yield return null;
	}
	
	public virtual IEnumerator ShuffleAndDeal()
	{
		yield return null;
	}

	public virtual bool CheckCard(Card card)
	{
		return false;
	}

	public virtual bool TryMove(CardPile toPile, List<Card> cards, bool hint = false)
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
	
	public bool GetCardHit(ref RaycastHit returned)
	{
		RaycastHit hit = new RaycastHit();
		Ray ray = GameManager.Instance.MainCam.ScreenPointToRay(Input.mousePosition);
		
		int layerMask = 1 << 9;
		bool hitTarget = false;
		
		if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
		{	
			returned = hit;
			hitTarget = true;
		}
	
		return hitTarget;
		
	}
	
	public virtual IEnumerator RestoreState(XDocument xdoc)
	{
		yield return null;	
	}
		
	public virtual void HintRequest()
	{
		
	}
	
	public virtual void ShowHint(NotificationCenter.Notification n)
	{
		
	}

	public void MenuState()
    {
		foreach (CardPile pile in allPiles)
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

	public void GatherDeck()
    {
		CardPile deck = stock;

		foreach (CardPile pile in allPiles)
		{
			if (pile == null)
				continue;

			foreach (Card c in pile.cards)
			{
				if (c != null)
				{
					c.Turn(true);
					deck.AddCard(c);
				}
			}
			pile.CardZ = 0;
			pile.NextPos = Vector3.zero;
			pile.Clear();			
		}


		deck.AlignCards();
		stock = deck;
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
