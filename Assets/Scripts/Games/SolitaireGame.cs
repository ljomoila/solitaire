
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

    public bool hintMode = false;

    public CardSelectionState cardSelectionState;


    public virtual IEnumerator Initialize(XDocument storedGameState)
	{
		yield return StartCoroutine(SetupTable());

        bool deal = true;

        if (storedGameState != null)
        {
            GameType storedType = (GameType) System.Enum.Parse(typeof(GameType), storedGameState.Root.Element("gameType").Value);

            if (storedType == gameType)
            {
                deal = false;
                yield return StartCoroutine(RestoreState(storedGameState));
            }               
        }
		
        if (deal)
            yield return StartCoroutine(DoDeal());
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
        List<CardPile> piles = GameManager.Instance.activeGame.Piles;

        foreach (CardPile pile in Piles)
        {
            foreach (Card c in pile.cards)
            {
                if (c != null)
                {
                    c.Turn(true);
                    stock.AddCard(c);
                }
            }
            pile.Clear();
        }

        stock.AlignCards();

        yield return null;
    }

    private IEnumerator Shuffle()
    {
        stock.Shuffle();
        yield return null;

        float t = .5f;

        Split(t);
        yield return new WaitForSeconds(t);

        Bend();

        yield return new WaitForSeconds(1f);
        stock.AlignCards(t, 0, 0);

        yield return new WaitForSeconds(.2f);
    }

    private void Bend()
    {
        AudioController.Play("shuffle");
    }

    private void Split(float animTime)
    {
        AudioController.Play("cardSlide");

        for (int i = 0; i < stock.cards.Count; i++)
        {
            float moveAmount = 1.6f;
            float rotateAmount = -150;

            if (i % 2 == 0)
            {
                moveAmount = -moveAmount;
                rotateAmount = -rotateAmount;
            }

            stock.cards[i].transform.Translate(0, 0, .45f);
            iTween.MoveBy(stock.cards[i].gameObject, iTween.Hash("x", moveAmount, "time", animTime, "isLocal", true));
            iTween.RotateTo(stock.cards[i].sprite.gameObject, iTween.Hash("z", rotateAmount, "time", animTime, "isLocal", true));
        }
    }

    public void DealCard(Card card, float animTime)
    {
        card.pile.AddCard(card);

        Vector3 nextPos = card.pile.NextPos;

        iTween.MoveTo(card.gameObject, iTween.Hash("position", nextPos, "time", 0.5f, "delay", animTime, "isLocal", true));

        nextPos.x += card.IsTurned() ? card.pile.xStepTurned : card.pile.xStep;
        nextPos.y -= card.IsTurned() ? card.pile.yStepTurned : card.pile.yStep;
        nextPos.z -= card.pile.zStep;
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

    public virtual void HintRequest()
    {
        hintMode = true;
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
    }

    public virtual void DrawCards()
	{
		CmdDrawCards cmd = new CmdDrawCards(this, stockDrawAmount, "Stock draw");
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
