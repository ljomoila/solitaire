
using System;
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

	public int stockDrawAmount = 3;

	public List<Cmd> commands = new List<Cmd>();

    public CardSelectionState cardSelectionState;

    public DealState dealState;
    public HintState hintState;

    public virtual IEnumerator Initialize(XDocument storedGameState)
	{
		yield return StartCoroutine(SetupTable());

        bool deal = true;

        if (storedGameState != null)
        {
            GameType storedType = (GameType) Enum.Parse(typeof(GameType), storedGameState.Root.Element("gameType").Value);

            if (storedType == gameType)
            {
                deal = false;
                yield return StartCoroutine(RestoreState(storedGameState));
            }               
        }

        if (deal)
            DealNewCards();
	}

	public virtual IEnumerator SetupTable()
    {
		yield return null;
    }

	public virtual IEnumerator RestoreState(XDocument xdoc)
	{
		yield return null;
	}

    public override void UpdateState()
    {
        TrySelection();
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

                SetSelectionPile(SelectCards(c), hit.point);
            }
        }
    }

    private void SetSelectionPile(List<Card> cards, Vector3 hitPoint)
    {
        if (cards == null || cards.Count == 0)
            return;

        if (cardSelectionState == null)
            cardSelectionState = new GameObject("CardSelectionState").AddComponent<CardSelectionState>();

        cardSelectionState.Initialize(this, cards, hitPoint);

        StateManager.Instance.ActivateState(cardSelectionState);
        
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

    public void DealNewCards()
    {
        dealState.Initialize(this);

        StateManager.Instance.ActivateState(dealState);
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
		CmdMoveCards cmd = new CmdMoveCards(movedCards, sourcePile, targetPile, "Move cards to: " + targetPile.Type);
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
