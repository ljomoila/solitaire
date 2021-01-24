
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

	public DealState DealState { get; set; } = null;

	public int stockDrawAmount = 3;

	public List<Cmd> commands = new List<Cmd>();

    public SelectionPile selectionPile;

    private Vector3 startHit = Vector3.zero;
    private float pileDraggingFactor = 10;

    public override void OnActivateState()
    {
        if (selectionPile == null)
		    selectionPile = new GameObject("SelectionPile").AddComponent<SelectionPile>();
    }

    public virtual IEnumerator Initialize(XDocument storedGameState)
	{
		yield return StartCoroutine(SetupTable());

		if (storedGameState != null)
			yield return StartCoroutine(RestoreState(storedGameState));
		else
			yield return StartCoroutine(DealState.DoDeal());

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
        RaycastHit hit = new RaycastHit();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(0)) // try selection
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Card c = hit.collider.gameObject.GetComponent<Card>();
                iTween[] tweens = hit.collider.gameObject.GetComponents<iTween>();

                if (c == null || tweens.Length > 0)
                    return;

                startHit = hit.point;

                SetSelection(Select(c));
            }
        }

        if (selectionPile == null || selectionPile.IsEmpty) return;

        if (Input.GetMouseButton(0)) // dragging pile
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Vector3 pos = hit.point - startHit;
                pos.z = -0.5f;

                selectionPile.transform.position = Vector3.Lerp(selectionPile.transform.position, pos, Time.deltaTime * pileDraggingFactor);
            }

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 9))
            {
                Card c = hit.collider.gameObject.GetComponent<Card>();

                if (c != null && c.pile != null && c.pile != selectionPile)
                {
                    CardPile pile = c.pile;

                    foreach (CardPile p in Piles)
                    {
                        if (p == pile)
                            p.Highlight();
                        else
                            p.Unhighlight();
                    }
                }
            }
        }
        if (Input.GetMouseButtonUp(0)) // try move to target pile
        {
            bool successfullMove = false;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 9))
            {
                Card c = hit.collider.gameObject.GetComponent<Card>();

                if (c != null && c.pile != null && c.pile != selectionPile)
                    successfullMove = TryMove(c.pile, selectionPile);
            }

            if (!successfullMove)
                CancelMove();
            else
                Reset();
        }
    }

    void SetSelection(List<Card> cards)
    {
        if (cards == null || cards.Count == 0)
            return;

        selectionPile.transform.position = cards[0].transform.position;
        selectionPile.sourcePile = cards[0].pile;
        selectionPile.yStep = cards[0].pile.yStep;

        int i = 0;
        foreach (Card card in cards)
        {
            selectionPile.AddCard(card);
            card.Pick(i);

            i++;
        }

        selectionPile.AlignCards();

        startHit.y -= cards[0].transform.position.y;
        startHit.x -= cards[0].transform.position.x;
        startHit.z = 0;

        AudioController.Play("cardSlide");
    }

    void CancelMove()
    {
        CardPile toPile = selectionPile.sourcePile;

        selectionPile.cards[0].Leave();

        foreach (Card c in selectionPile.cards)
        {
            toPile.AddCard(c, .5f, 0);
        }

        toPile.AlignCards();

        Reset();
    }

    void Reset()
    {
        foreach (CardPile pile in Piles)
        {
            pile.Unhighlight();
        }

        selectionPile.Clear();

        AudioController.Play("cardSlide");

        //StateManager.Instance.ActivateState(game);
    }

    public virtual List<Card> Select(Card card)
	{
		return null;
	}

	public virtual bool TryMove(CardPile toPile, SelectionPile sourcePile)
	{
		return false;
	}

	public virtual void DrawCards()
	{
		CmdDrawCards cmd = new CmdDrawCards(this, stockDrawAmount, "Stock draw");
		cmd.Execute(false);
		GameManager.Instance.StoreCommand(cmd);
	}

	public virtual void MoveCards(List<Card> movedCards, CardPile sourcePile, CardPile targetPile)
	{
		CmdMoveCards cmdMoveCards = new CmdMoveCards(movedCards, sourcePile, targetPile, "Move cards to: " + targetPile.Type);
		cmdMoveCards.Execute(false);
		commands.Add(cmdMoveCards);
	}

	public virtual void TurnCard(Card card)
	{
		CmdTurnCard cmdTurn = new CmdTurnCard(card, "Turn Klondyke card") { Animate = true };
		cmdTurn.Execute(false);
		commands.Add(cmdTurn);
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
