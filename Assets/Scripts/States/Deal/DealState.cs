
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealState : StateBase
{
    public CardPile deck;    

    public Game activeGame;

    public void Initialize(Game game)
    {
        this.deck = game.stock;
        this.activeGame = game;
    }

    public override void OnActivateState()
    {
        StartCoroutine(DoDeal());
    }

    IEnumerator DoDeal()
    {
        yield return StartCoroutine(GatherDeck());

        yield return StartCoroutine(Shuffle());

        yield return StartCoroutine(Deal());

        StateManager.Instance.ActivateState(activeGame);
    }

    public virtual IEnumerator Deal()
    {
        yield return null;
    }

    private IEnumerator GatherDeck()
    {
        foreach (CardPile pile in activeGame.Piles)
        {
            foreach (Card c in pile.cards)
            {
                if (c != null)
                {
                    c.Turn(true);
                    deck.AddCard(c);
                }
            }
            pile.Clear();
        }

        deck.AlignCards();

        yield return null;
    }

    private IEnumerator Shuffle()
    {
        deck.Shuffle();
        yield return null;

        float t = .5f;

        Split(t);
        yield return new WaitForSeconds(t);

        Bend();

        yield return new WaitForSeconds(1f);
        deck.AlignCards(t, 0, 0);

        yield return new WaitForSeconds(.2f);
    }

    private void Bend()
    {
        AudioController.Play("shuffle");
    }

    private void Split(float animTime)
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
