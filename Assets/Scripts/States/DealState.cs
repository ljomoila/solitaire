

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealState : StateBase
{
    public CardPile deck;

    public override void OnActivateState()
    {
        this.deck = GameManager.Instance.activeGame.stock;

        StartCoroutine(DoDeal());
    }

    public IEnumerator DoDeal()
    {
        yield return StartCoroutine(GatherDeck());

        yield return StartCoroutine(Shuffle());

        yield return StartCoroutine(Deal());
    }

    public virtual IEnumerator Deal()
    {
        yield return null;
    }

    private IEnumerator GatherDeck()
    {
        List<CardPile> piles = GameManager.Instance.activeGame.AllPiles;

        foreach (CardPile pile in piles)
        {
            if (pile == null)
                continue;

            foreach (Card c in pile.cards)
            {
                if (c != null)
                {
                    c.Turn(true);
                    deck.AddCard(c);
                }
            }
            pile.CardZ = 0;
            pile.NextPos = Vector3.zero;
            pile.Clear();
        }

        deck.AlignCards();
        GameManager.Instance.activeGame.stock = deck;

        yield return null;
    }

    private IEnumerator Shuffle()
    {
        ShuffleCards();
        yield return null;

        float t = .5f;

        DividedInTwo(t);
        yield return new WaitForSeconds(t);

        AnimBend();
        deck.AlignCards(t, 0, 0);

        yield return new WaitForSeconds(1f);
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

        for (int i = 0; i < deck.cards.Count; i++)
        {
            float moveAmount = 1.6f;
            float rotateAmount = -150;

            if (i % 2 == 0)
            {
                moveAmount = -moveAmount;
                rotateAmount = -rotateAmount;
            }

            deck.cards[i].transform.Translate(0, 0, .45f);
            iTween.MoveBy(deck.cards[i].gameObject, iTween.Hash("x", moveAmount, "time", animTime, "isLocal", true));
            iTween.RotateTo(deck.cards[i].sprite.gameObject, iTween.Hash("z", rotateAmount, "time", animTime, "isLocal", true));
        }
    }

    private void ShuffleCards()
    {
        int cardCount = deck.cards.Count;        

        List<Card> newDeck = new List<Card>();

        for (int i = 0; i < cardCount; i++)
        {
            newDeck.Add(null);
        }

        foreach (Card card in deck.cards)
        {
            int nextIndex = Random.Range(0, cardCount);

            while (newDeck[nextIndex] != null)
            {
                nextIndex = Random.Range(0, cardCount);
            }

            card.transform.position = deck.cards[nextIndex].transform.position;
            newDeck.RemoveAt(nextIndex);
            newDeck.Insert(nextIndex, card);
        }
        deck.cards.Clear();

        for (int i = 0; i < cardCount; i++)
        {
            deck.cards.Add(newDeck[i]);
        }

        deck.AlignCards();
    }

    public void DealCard(Card card, float animTime)
    {
        card.Pile.AddCard(card);

        Vector3 nextPos = card.Pile.NextPos;

        iTween.MoveTo(card.gameObject, iTween.Hash("position", nextPos, "time", 0.5f, "delay", animTime, "isLocal", true));

        nextPos.x += card.IsTurned() ? card.Pile.xStepTurned : card.Pile.xStep;
        nextPos.y -= card.IsTurned() ? card.Pile.yStepTurned : card.Pile.yStep;
        nextPos.z -= card.Pile.zStep;
    }

    public IEnumerator TurnLastCards(List<CardPile> piles, float delay = 0)
    {
        foreach (CardPile pileau in piles)
        {
            Card c = pileau.GetLastCard();
            c.Turn(false, .05f);
        }
        yield return new WaitForSeconds(delay);

        AudioController.Play("cardSlide");

        yield return null;
    }
}
