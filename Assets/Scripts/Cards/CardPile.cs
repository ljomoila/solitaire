using UnityEngine;
using System.Collections.Generic;

public class CardPile : MonoBehaviour
{
	public List<Card> cards = new List<Card>();
	public PileType Type = PileType.Piled;
	public PileSlot slot;

	public float xStep = 0f, yStep = 0f, xStepTurned = 0f, yStepTurned = 0f;
	public float zStep = .05f;

	public float CardZ { get; set; }
    public Suit Suit { get; set; }

	private Vector3 nextPos;
	public Vector3 NextPos
	{
		get { return nextPos; }
		set
		{
			nextPos = value;
		}
	}

	float animTime = 0;

	public virtual void AddCard(Card card)
	{
		if (!cards.Contains(card))
			cards.Add(card);

		this.animTime = 0;

		card.pile = this;
		card.transform.parent = transform;
		card.gameObject.layer = 9; // TODO naming

		nextPos.x += card.IsTurned() ? xStepTurned : xStep;
		nextPos.y -= card.IsTurned() ? yStepTurned : yStep;
		nextPos.z -= zStep;

		iTween.RotateTo(card.gameObject, iTween.Hash("rotation", Vector3.zero, "time", 0));
	}

	public virtual void AddCard(Card card, float time, float delay = 0)
	{
		AddCard (card);

		this.animTime = time;
		
		iTween.MoveTo(card.gameObject, iTween.Hash("position", new Vector3(NextPos.x, NextPos.y, CardZ), "time", time, "delay", delay, "isLocal", true));
		
		if (time > 0)
			card.Leave();
	}
	
	public bool IsEmpty
    {
        get { return cards.Count == 0; }
    }

	public void Shuffle()
    {
		int cardCount = cards.Count;

		List<Card> deck = new List<Card>();

		for (int i = 0; i < cardCount; i++)
		{
			deck.Add(null);
		}

		foreach (Card card in cards)
		{
			int nextIndex = Random.Range(0, cardCount);

			while (deck[nextIndex] != null)
			{
				nextIndex = Random.Range(0, cardCount);
			}

			card.transform.position = cards[nextIndex].transform.position;
			deck.RemoveAt(nextIndex);
			deck.Insert(nextIndex, card);
		}

		cards.Clear();

		for (int i = 0; i < cardCount; i++)
		{
			cards.Add(deck[i]);
		}

		AlignCards();
	}


    public virtual void AlignCards()
    {
        AlignCards(0, 0, 0);
    }

    internal void AlignCards(float delay, int startIndex)
    {
        AlignCards(0, delay, startIndex);
    }

    public virtual void AlignCards(float time, float delay, int startIndex)
    {
		CardZ = 0;
		float cardX = 0;
		float cardY = 0;
		float sizeY = 3.5f;
		float boxY = 0f;		
		
		int index = 0;
		
		foreach (Card card in cards)
        {
            if (card == null)
                continue;		
			
			iTween.RotateTo(card.sprite.gameObject, iTween.Hash("rotation", new Vector3(0, card.IsTurned() ? 180 : 0, 0), "time", animTime));
			iTween.MoveTo(card.sprite.gameObject, iTween.Hash("position", Vector3.zero, "time", animTime, "isLocal", true));
            iTween.MoveTo(card.gameObject, iTween.Hash("position", new Vector3(cardX, cardY, CardZ), "time", animTime, "isLocal", true));

            cardX += card.IsTurned() ? xStepTurned : xStep;
            cardY -= card.IsTurned() ? yStepTurned : yStep;			
			sizeY += card.IsTurned() ? yStepTurned : sizeY;
			boxY -= card.IsTurned() ? yStepTurned * 1.1f : yStep * 1.2f;
			NextPos = card.IsTurned() ? new Vector2(cardX + xStepTurned, cardY - yStepTurned) : new Vector2(cardX + xStep, cardY - yStep);

			CardZ -= zStep;		
			delay += index > startIndex ? time : 0;
                            
            index++;
        }
	}
	
	public void Clear()
    {
		CardZ = 0;
		NextPos = Vector3.zero;

		Unhighlight();

		List<Card> cardsToRemove = new List<Card>(cards);

        foreach (Card card in cardsToRemove)
		{
            RemoveCard(card);
        }
    }

    public virtual void RemoveCard(Card card)
    {
		cards.Remove(card);
		
    }
	
	public Card GetFirstCard()
    {
        Card card = null;

        if (cards.Count > 0)
        {
            card = cards[cards.Count - 1];

            cards.Remove(card);
        }

        return card;
    }

    public Card GetLastCard()
    {
        return GetCard(cards.Count - 1);
    }

    public Card GetCard(int index)
    {
        if (index >= 0 && cards.Count > index)
            return cards[index];

        return null;
    }

	public Card GetCard(int number, Suit suit)
	{
		Card card = null;

		foreach (Card c in cards)
		{
			card = c;

			if (c.number == number && c.suit == suit)
			{
				RemoveCard(c);
				break;
			}
		}
		return card;
	}

	public void Unhighlight()
    {
		if (slot == null)
			return;

		slot.Unhighlight();
    }

    public void Highlight()
    {
		if (slot == null)
			return;

		slot.Highlight();
    }
}

public enum PileType
{
    Piled,
    Tableau,
    SelectionPile,
    PyramidPile,
    Waste,
	Stock,
	Foundation
}
