using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class Game : StateBase
{
    public GameType gameType;

    public CardPile stock;
    public CardPile Waste { get; set; } = new CardPile();
    public List<CardPile> Piles { get; set; } = new List<CardPile>();
    public List<CardPile> TableauPiles { get; set; } = new List<CardPile>();

    public GameState State { get; set; } = GameState.Playing;

    public List<Cmd> commands = new List<Cmd>();

    public CardSelection cardSelection;

    public DealState dealState;
    public HintState hintState;

    public override void OnActivateState()
    {
        stock.gameObject.SetActive(true);
    }

    public virtual IEnumerator Initialize()
    {
        yield return null;
    }

    public virtual IEnumerator RestoreState(XDocument xdoc)
    {
        GameManager.Instance.gameTime.Time = (float)xdoc.Root.Element("gameTime");

        yield return null;
    }

    public override void UpdateState()
    {
        if (State == GameState.Playing)
        {
            TrySelection();
        }
        else if (State == GameState.Solved)
        {
            GameManager.Instance.Solved();
        }
    }

    void TrySelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit = new RaycastHit();
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Card c = hit.collider.gameObject.GetComponent<Card>();
                iTween[] tweens = hit.collider.gameObject.GetComponents<iTween>();

                if (c == null || tweens.Length > 0)
                    return;

                SetSelectionPile(SelectCards(c));
            }
        }
    }

    private void SetSelectionPile(List<Card> cards)
    {
        if (cards == null || cards.Count == 0)
            return;

        if (cardSelection == null)
        {
            cardSelection = new GameObject("CardSelectionState").AddComponent<CardSelection>();
        }

        cardSelection.Initialize(cards);
    }

    public virtual List<Card> SelectCards(Card card)
    {
        return null;
    }

    public virtual bool TryMoveToPile(CardPile toPile)
    {
        return TryMoveToPile(toPile, null);
    }

    public virtual bool TryMoveToPile(CardPile toPile, SelectionPile selectionPile)
    {
        return false;
    }

    public void TurnLastTableauCards()
    {
        foreach (CardPile pile in TableauPiles)
        {
            Card lastCard = pile.GetLastCard();

            if (lastCard != null && lastCard.IsTurned())
            {
                TurnCard(lastCard);
            }
        }
    }

    public IEnumerator DealNewCards()
    {
        dealState.Initialize(this);

        StateManager.Instance.ActivateState(dealState);

        yield return null;
    }

    public virtual void HintRequest()
    {
        StateManager.Instance.ActivateState(hintState);
    }

    public bool IsHintActive()
    {
        return StateManager.Instance.activeState == hintState;
    }

    public virtual void DrawCards(int amount)
    {
        CmdDrawCards cmd = new CmdDrawCards(this, amount, "Stock draw");
        cmd.Execute(false);

        CommandManager.Instance.StoreCommand(cmd);
    }

    public virtual void MoveCards(List<Card> movedCards, CardPile sourcePile, CardPile targetPile)
    {
        CmdMoveCards cmd = new CmdMoveCards(
            movedCards,
            sourcePile,
            targetPile,
            "Move cards to: " + targetPile.Type
        );
        cmd.Execute(false);
        commands.Add(cmd);

        CommandManager.Instance.StoreCommand(new CmdComposite(commands));
    }

    public virtual void TurnCard(Card card)
    {
        CmdTurnCard cmd = new CmdTurnCard(card, "Turn card") { Animate = true };
        cmd.Execute(false);
        commands.Add(cmd);

        CommandManager.Instance.StoreCommand(new CmdComposite(commands));
    }

    public void Solved()
    {
        Debug.Log("Game solved");
        // TODO
    }
}

public enum GameType
{
    Klondyke,
    Spider,
    Freecell,
    Golf,
    Pyramid,
    EightOff,
    Clock
}
