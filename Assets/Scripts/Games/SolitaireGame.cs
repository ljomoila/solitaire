
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

    public SelectionPile selectionPile;

    private Vector3 startHit = Vector3.zero;

    private float pileDraggingFactor = 10;
    public bool hintMode = false;

    public override void OnActivateState()
    {
        if (selectionPile == null)
		    selectionPile = new GameObject("SelectionPile").AddComponent<SelectionPile>();
    }

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
                    successfullMove = TryMove(c.pile);
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
    }

    public virtual List<Card> Select(Card card)
	{
		return null;
	}

    public bool TryMove(CardPile toPile)
    {
        return TryMove(toPile, selectionPile);
    }

    public virtual bool TryMove(CardPile toPile, SelectionPile sourcePile)
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
		GameManager.Instance.StoreCommand(cmd);
	}

	public virtual void MoveCards(List<Card> movedCards, CardPile sourcePile, CardPile targetPile)
	{
		CmdMoveCards cmd = new CmdMoveCards(movedCards, sourcePile, targetPile, "Move cards to: " + targetPile.Type);
        cmd.Execute(false);
		commands.Add(cmd);
	}

	public virtual void TurnCard(Card card)
	{
		CmdTurnCard cmd = new CmdTurnCard(card, "Turn card") { Animate = true };
        cmd.Execute(false);
		commands.Add(cmd);
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
