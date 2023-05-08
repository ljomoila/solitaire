using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DealStateKlondyke : DealState
{
    public override IEnumerator Deal()
    {
        List<CardPile> piles = activeGame.TableauPiles;

        float dealTime = 0;
        int pileIndex = 0;
        int j = 0;

        for (int i = 0; i < 28; i++)
        {
            Card card = deck.GetFirstCard();
            card.pile = piles[pileIndex];

            //if (turn)
            //	card.Turn(false, .5f);

            DealCard(card, .5f, dealTime);

            yield return new WaitForSeconds(dealTime);

            if (pileIndex > 0 && pileIndex % 6 == 0)
            {
                j++;
                pileIndex = j;
            }
            else
            {
                pileIndex++;
                AudioController.Play("cardSlide", 1, dealTime);
            }

            dealTime = i * 0.001f;
        }

        yield return new WaitForSeconds(dealTime);

        activeGame.TurnLastTableauCards();

        AudioController.Play("cardSlide");
    }
}
