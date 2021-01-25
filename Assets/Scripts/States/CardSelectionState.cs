using System.Collections.Generic;
using UnityEngine;

public class CardSelectionState : StateBase
{
    private Game activeGame;
    private Vector3 startHit;
    private List<Card> cards;

    private SelectionPile pile;

    public void Initialize(Game activeGame, List<Card> cards, Vector3 startHit)
    {
        this.activeGame = activeGame;
        this.cards = cards;
        this.startHit = startHit;
    }

    public override void OnActivateState()
    { 
        transform.position = cards[0].transform.position;

        startHit.y -= cards[0].transform.position.y;
        startHit.x -= cards[0].transform.position.x;
        startHit.z = 0;

        SetupPile();
        
        AudioController.Play("cardSlide");
    }

    private void SetupPile()
    {
        if (pile == null)
            pile = gameObject.AddComponent<SelectionPile>();

        pile.sourcePile = cards[0].pile;
        pile.yStep = cards[0].pile.yStep;

        int i = 0;
        foreach (Card card in cards)
        {
            pile.AddCard(card);
            card.Pick(i);

            i++;
        }

        pile.AlignCards();
    }

    RaycastHit hit;
    Ray ray;
    public override void UpdateState()
    {
        hit = new RaycastHit();
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButton(0))
        {
            Drag();
            HighlightPiles();
        }        

        TryMove();
    }

    private void TryMove()
    {
        if (!Input.GetMouseButtonUp(0))
            return;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 9))
        {
            Card c = hit.collider.gameObject.GetComponent<Card>();

            if (c != null && c.pile != null && c.pile != pile)
            {
                if (!activeGame.TryMoveToPile(c.pile, pile))
                    CancelMove();
            }
        }

        Reset();

        StateManager.Instance.ActivateState(activeGame);
    }

    private void HighlightPiles()
    {
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 9))
        {
            Card c = hit.collider.gameObject.GetComponent<Card>();

            if (c != null && c.pile != null && c.pile != pile)
            {
                CardPile pile = c.pile;

                foreach (CardPile p in activeGame.Piles)
                {
                    if (p == pile)
                        p.Highlight();
                    else
                        p.Unhighlight();
                }
            }
        }
    }

    private void Drag()
    {
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Vector3 pos = hit.point - startHit;
            pos.z = -0.5f;

            transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * 10);
        }
    }

    void CancelMove()
    {
        if (pile.cards.Count == 0)
            return;

        pile.cards[0].Leave();

        foreach (Card c in pile.cards)
        {
            pile.AddCard(c, .5f, 0);
        }

        pile.AlignCards();
    }

    void Reset()
    {
        foreach (CardPile pile in activeGame.Piles)
        {
            pile.Unhighlight();
        }

        pile.Clear();

        AudioController.Play("cardSlide");
    }
}
