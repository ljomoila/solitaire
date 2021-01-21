using System;
using System.Collections.Generic;
using UnityEngine;

public class FoundationSlot : PileSlot
{
	public CardSuit suit;
	
	public SpriteRenderer suitSprite;
	public List<Sprite> suits;

	public GameObject spadeEffect, heartEffect, diamondEffect, clubEffect;
	GameObject suitEffect = null;

	public void Initialize(CardSuit suit, CardPile pile)
	{
		base.Initialize(pile);

		this.suit = suit;
		transform.Translate(0, 0, .05f);

		InitializeSuitSprite();

		NotificationCenter.DefaultCenter.AddObserver(this, GameEvents.FoundationMoveDone);
	}

	void InitializeSuitSprite()
    {
		suitSprite.sprite = suits.Find(item => item.name.ToLower() == suit.ToString().ToLower());

		GameObject go;

		if (suit == CardSuit.Spade)
			go = spadeEffect;
		else if (suit == CardSuit.Heart)
			go = heartEffect;
		else if (suit == CardSuit.Club)
			go = clubEffect;
		else
			go = diamondEffect;

		suitEffect = Instantiate(go, Vector3.zero, Quaternion.identity, transform);
		suitEffect.transform.position = transform.position;
		suitEffect.SetActive(false);
		suitEffect.transform.Translate(0, 0, -.45f);
	}

	void FoundationMoveDone(NotificationCenter.Notification n)
	{
		if ((CardSuit)n.data["suit"] == suit)
			suitEffect.SetActive(true);
	}
}
