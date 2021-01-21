using UnityEngine;

public class PileSlot : MonoBehaviour
{
	public CardPileType pileType;
	public Card card;

	public SpriteRenderer border;

	void Awake()
    {
		Unhighlight();
    }

	public void Initialize(CardPile pile)
	{		
		gameObject.SetActive(true);

		pileType = pile.Type;
		card.Pile = pile;
	}

	internal void Unhighlight()
	{
		Color c = border.color;
		c.a = .3f;
		border.color = c;
	}

	internal void Highlight()
	{
		Color c = border.color;
		c.a = 1f;
		border.color = c;
	}
}
