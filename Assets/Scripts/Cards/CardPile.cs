using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardPile : MonoBehaviour
{
	public CardPile sourcePile;
	
	public List<Card> cards = new List<Card>();
	public CardPileType Type = CardPileType.Piled;
	public PileSlot slot;
	
	public Random rnd;
	
	public int cardCount = 0;

	public float xStep = 0f, yStep = 0f, xStepTurned = 0f, yStepTurned = 0f;
	public float zStep = .05f;

	float cardZ = 0;
	public float CardZ
    {
		get { return cardZ; }
		set
		{
			cardZ = value;
		}
	}

    public CardSuit CardSuit { get; set; }

    Vector2 nextPos;
	public Vector2 NextPos
	{
		get { return nextPos; }
		set
		{
			nextPos = value;
		}
	}


	float animTime = 0;

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public virtual void AddCard(Card card)
	{
		if (!cards.Contains(card))
			cards.Add(card);

		this.animTime = 0;

		card.Pile = this;
		card.transform.parent = transform;
		card.gameObject.layer = 9; // TODO naming

		iTween.RotateTo(card.gameObject, iTween.Hash("rotation", Vector3.zero, "time", 0));
	}

	public void AddCard(List<Card> cards)
	{
		foreach (Card card in cards)
		{
			AddCard(card);
		}
	}

	public virtual void AddCard(Card card, float time, float delay)
	{
		AddCard (card);

		this.animTime = time;
		
		iTween.MoveTo(card.gameObject, iTween.Hash("position", new Vector3(nextPos.x, nextPos.y, cardZ), "time", time, "delay", delay, "isLocal", true));
		
		if (time > 0)
			card.Leave();
	}
	
	public void AddCardDeal(Card card, float time, float delay)
	{
		AddCard (card);
		
		iTween.MoveTo(card.gameObject, iTween.Hash("position", new Vector3(nextPos.x, nextPos.y, cardZ), "time", time, "delay", delay, "isLocal", true));
		
		nextPos.x += card.IsTurned() ? xStepTurned : xStep;
		nextPos.y -= card.IsTurned() ? yStepTurned : yStep;
		
		cardZ -= zStep;
	}
	
	public bool IsEmpty
    {
        get { return cards.Count == 0; }
    }

	internal IEnumerator Shuffle()
	{
		ShuffleCards();
		yield return null;

		yield return StartCoroutine(AnimShuffle());
	}

	private IEnumerator AnimShuffle()
    {
		float t = .5f;

		DividedInTwo(t);
		yield return new WaitForSeconds(t);

		AnimBend();
		AlignCards(t, 0, 0);

		yield return new WaitForSeconds(1.25f);
	}

    private void AnimBend()
    {
		AudioController.Play("shuffle");

		// TODO one single animation for both sides

		//for (int i = 0; i < cards.Count; i++)
		//{
		//	if (i % 2 == 0)
		//	{
		//		cards[i].sprite.GetComponent<Animation>()["cardBend_01"].speed = 1;
		//		cards[i].sprite.GetComponent<Animation>().Play("cardBend_01");
		//	}
		//	else
		//	{
		//		cards[i].sprite.GetComponent<Animation>()["cardBend_02"].speed = 1;
		//		cards[i].sprite.GetComponent<Animation>().Play("cardBend_02");
		//	}
		//}

		//yield return new WaitForSeconds(.1f);

		//for (int i = 0; i < cards.Count; i++)
		//{
		//	if (i % 2 == 0)
		//	{
		//		cards[i].sprite.GetComponent<Animation>()["cardBend_01"].speed = 0;
		//	}
		//	else
		//	{
		//		cards[i].sprite.GetComponent<Animation>()["cardBend_02"].speed = 0;
		//	}
		//}

		//for (int i = 0; i < cards.Count; i++)
		//{
		//	if (i % 2 == 0)
		//	{
		//		cards[i].sprite.GetComponent<Animation>()["cardBend_01"].speed = -1;
		//		//cards[i].sprite.animation.Play("cardBend_01");
		//	}
		//	else
		//	{
		//		yield return new WaitForSeconds(.0001f);
		//		cards[i].sprite.GetComponent<Animation>()["cardBend_02"].speed = -1;
		//		cards[i].sprite.GetComponent<Animation>().Play("cardBend_02");
		//	}

		//	//AudioController.Play("cardTurn");
		//}
	}
    
    private void DividedInTwo(float animTime)
    {
		AudioController.Play("cardSlide");
		
		for (int i = 0; i < cards.Count; i++)
		{
			float moveAmount = 1.6f;
			float rotateAmount = -150;

			if (i % 2 == 0)
			{
				moveAmount = -moveAmount;
				rotateAmount = -rotateAmount;
			}

			cards[i].transform.Translate(0, 0, .45f);
			iTween.MoveBy(cards[i].gameObject, iTween.Hash("x", moveAmount, "time", animTime, "isLocal", true));
			iTween.RotateTo(cards[i].sprite.gameObject, iTween.Hash("z", rotateAmount, "time", animTime, "isLocal", true));
		}
	}

	void ShuffleCards()
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
        AlignCards(0, 0);
    }

    internal void AlignCards(float delay, int startIndex)
    {
        AlignCards(0, delay, startIndex);
    }

    public virtual void AlignCards(float time, float delay, int startIndex)
    {
		cardZ = 0;
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
            iTween.MoveTo(card.gameObject, iTween.Hash("position", new Vector3(cardX, cardY, cardZ), "time", animTime, "isLocal", true));

            cardX += card.IsTurned() ? xStepTurned : xStep;
            cardY -= card.IsTurned() ? yStepTurned : yStep;			
			sizeY += card.IsTurned() ? yStepTurned : sizeY;
			boxY -= card.IsTurned() ? yStepTurned * 1.1f : yStep * 1.2f;
			nextPos = card.IsTurned() ? new Vector2(cardX + xStepTurned, cardY - yStepTurned) : new Vector2(cardX + xStep, cardY - yStep);

			cardZ -= zStep;		
			delay += index > startIndex ? time : 0;
                            
            index++;
        }
	}
	
	public void Clear()
    {
		Unhighlight();

		List<Card> cardsToRemove = new List<Card>(cards);

        foreach (Card card in cardsToRemove)
		{
            RemoveCard(card);
        }
		
		if (Type == CardPileType.SelectionPile)
        {
			Destroy (gameObject);
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

    internal void Unhighlight()
    {
		if (slot == null)
			return;

		slot.Unhighlight();
    }

    internal void Highlight()
    {
		if (slot == null)
			return;

		slot.Highlight();
    }
}

public enum CardPileType
{
    Piled,
    Tableau,
    SelectionPile,
    PyramidPile,
    KlondikeWaste,
	Stock,
	Foundation
}
