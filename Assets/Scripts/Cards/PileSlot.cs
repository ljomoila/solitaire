using System.Collections.Generic;
using UnityEngine;

public class PileSlot : MonoBehaviour
{
	public CardPileType pileType;
	public Card card;
	public CardSuit suit;
	
	public SpriteRenderer suitSprite, border;
	public List<Sprite> suits;

	public GameObject spadeEffect, heartEffect, diamondEffect, clubEffect;
	GameObject suitEffect = null;

	// Use this for initialization
	void Start () 
	{
		suitSprite.gameObject.SetActive(pileType == CardPileType.Foundation);
		
		if (pileType == CardPileType.Foundation)
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
			
			NotificationCenter.DefaultCenter.AddObserver(this, GameEvents.FoundationMoveDone);	
		}
	}

	void FoundationMoveDone(NotificationCenter.Notification n)
	{
		if ((CardSuit)n.data["suit"] == suit)
			suitEffect.SetActive(true);
	}
}
