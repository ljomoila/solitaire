using UnityEngine;
using System.Collections.Generic;
using System;

public class HintState : StateBase
{
    public Game activeGame;

    public override void OnActivateState()
    {
        this.activeGame = GameManager.Instance.activeGame;

        TryShowingHint();

        StateManager.Instance.ActivateState(activeGame);
    }

    public virtual void TryShowingHint()
    {
        throw new NotImplementedException();
    }

    public virtual void ShowHint(Card card)
    {
        ShowHint(new List<Card> { card });
    }

    public virtual void ShowHint(List<Card> cards)
    {
        foreach (Card card in cards)
        {
            // TODO make one single animation
            card.sprite.GetComponent<Animation>().Play("cardHint_01", AnimationPlayMode.Stop);
            card.sprite.GetComponent<Animation>().Play("cardHint_01", AnimationPlayMode.Queue);
        }

        StateManager.Instance.ActivateState(activeGame);
    }
}
