using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealStateKlondyke : DealState
{
    private static int dealAmount = 28;
    private static float animTime = .0f;
    private static int pileAmount = 6;

    public override IEnumerator Deal()
    {
        List<CardPile> piles = GameManager.Instance.activeGame.tableauPiles;

        float dealTime = 0;
        int pileCount = 0;
        int j = 0;        

        for (int i = 0; i < dealAmount; i++)
        {
            Card card = deck.GetFirstCard();
            card.Pile = piles[pileCount];
            card.transform.parent = card.Pile.transform;

            ThrowCard(card, dealTime);

            yield return new WaitForSeconds(dealTime);

            if (pileCount > 0 && pileCount % pileAmount == 0)
            {
                j++;
                pileCount = j;
            }
            else
            {
                pileCount++;
                AudioController.Play("cardSlide", 1, dealTime);
            }
            dealTime = i * animTime;
        }

        GameManager.Instance.activeGame.stock = deck;

        // TODO Turn while throwing
        yield return StartCoroutine(TurnLastCards(piles, dealTime));
    }
}
