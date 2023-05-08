using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DealStateGolf : DealState
{
    public override IEnumerator Deal()
    {
        List<CardPile> piles = activeGame.TableauPiles;

        float animTime = .5f;
        float dealTime = 0;
        int pileIndex = 0;

        for (int i = 0; i < 35; i++)
        {
            Card card = deck.GetFirstCard();
            card.pile = piles[pileIndex];

            DealCard(card, animTime, dealTime);
            card.Turn(false, animTime);

            yield return new WaitForSeconds(.1f);

            pileIndex++;
            AudioController.Play("cardSlide", 1, dealTime);

            if (pileIndex % 7 == 0)
                pileIndex = 0;

            dealTime = i * 0.0001f;
        }

        // Draw first card to waste
        activeGame.DrawCards(1);
    }
}
