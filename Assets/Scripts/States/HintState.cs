using System.Collections.Generic;
using UnityEngine;

public class HintState : StateBase
{
    public override void OnActivateState()
    {
		Game activeGame = GameManager.Instance.activeGame;
		List<Card> hintCards = new List<Card>();
		bool hintShown = false;

		foreach (CardPile pile in activeGame.Piles)
		{
			if (hintShown) break;

			hintCards.Clear();

			foreach (Card card in pile.cards)
			{
				if (card.IsTurned()) continue;

				hintCards.Add(card);

				if (pile.Type == PileType.KlondikeWaste)
				{
					hintCards.Clear();
					hintCards.Add(pile.GetLastCard());
				}
			}

			if (hintCards.Count == 0) continue;

			SelectionPile hintPile = hintCards[0].pile.gameObject.AddComponent<SelectionPile>();
			hintPile.cards = hintCards;
			hintPile.sourcePile = pile;

			foreach (CardPile toPile in activeGame.Piles)
			{
				if (activeGame.TryMove(toPile, hintPile))
				{
					ShowHint(hintCards[0].pile);
					hintShown = true;
					break;
				}
			}
		}

		StateManager.Instance.ActivateState(previousState);
	}

	public void ShowHint(CardPile pile)
	{
		// no hint when theres only one card in foundation
		if (pile.Type == PileType.Foundation && pile.cards.Count == 1)
			return;

		List<Card> hintCards = new List<Card>();

		if (pile.Type == PileType.Tableau)
		{
			foreach (Card card in pile.cards)
			{
				if (card.IsTurned()) continue;

				hintCards.Add(card);			
			}
		}
		else // waste or foundation
		{
			hintCards.Add(pile.GetLastCard());
		}

		foreach (Card card in hintCards)
		{
			// TODO make one single animation
			card.sprite.GetComponent<Animation>().Play("cardHint_01", AnimationPlayMode.Stop);
			card.sprite.GetComponent<Animation>().Play("cardHint_01", AnimationPlayMode.Queue);
		}
	}
}
