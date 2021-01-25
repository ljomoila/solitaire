using System.Collections.Generic;

public class HintKlondyke : HintState
{

    public override void TryShowingHint()
    {
		List<CardPile> piles = activeGame.Piles;

		List<Card> hintCards = new List<Card>();
		bool hintShown = false;

		foreach (CardPile pile in piles)
		{
			if (hintShown) break;

			hintCards.Clear();

			foreach (Card card in pile.cards)
			{
				if (card.IsTurned()) continue;

				hintCards.Add(card);

				if (pile.Type == PileType.Waste)
				{
					hintCards.Clear();
					hintCards.Add(pile.GetLastCard());
				}
			}

			if (hintCards.Count == 0) continue;

			SelectionPile hintPile = hintCards[0].pile.gameObject.AddComponent<SelectionPile>();
			hintPile.cards = hintCards;
			hintPile.sourcePile = pile;

			foreach (CardPile toPile in piles)
			{
				if (activeGame.TryMoveToPile(toPile, hintPile))
				{
					ShowHint(GetHintCards(hintCards[0].pile));
					hintShown = true;
					break;
				}
			}
		}
	}

    private List<Card> GetHintCards(CardPile pile)
	{
		// no hint when theres only one card in foundation
		if (pile.Type == PileType.Foundation && pile.cards.Count == 1)
			return new List<Card>();

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

		return hintCards;
	}
}
