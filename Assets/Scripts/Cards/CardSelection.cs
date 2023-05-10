using System.Collections.Generic;
using UnityEngine;

public class CardSelection : MonoBehaviour
{
    private List<Card> cards;

    private SelectionPile pile;

    public void Initialize(List<Card> cards)
    {
        this.cards = cards;

        transform.position = cards[0].transform.position;

        SetupPile();
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
        AudioController.Play("cardSlide");
    }

    RaycastHit hit;
    Ray ray;

    void Update()
    {
        hit = new RaycastHit();
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButton(0))
        {
            Drag();
            HighlightPiles();
        }
        if (Input.GetMouseButtonUp(0))
        {
            TryMove();
        }
    }

    private void TryMove()
    {
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 9))
        {
            Card c = hit.collider.gameObject.GetComponent<Card>();

            if (c != null && c.pile != null && c.pile != pile)
            {
                if (!GameManager.Instance.ActiveGame.TryMoveToPile(c.pile, pile))
                    CancelMove();
            }
        }

        Reset();
    }

    private void HighlightPiles()
    {
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 9))
        {
            Card c = hit.collider.gameObject.GetComponent<Card>();

            if (c != null && c.pile != null && c.pile != pile)
            {
                CardPile pile = c.pile;

                foreach (CardPile p in GameManager.Instance.ActiveGame.Piles)
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
            Vector3 pos = hit.point;
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
            pile.sourcePile.AddCard(c, .5f);
        }

        pile.sourcePile.AlignCards();
    }

    void Reset()
    {
        foreach (CardPile pile in GameManager.Instance.ActiveGame.Piles)
        {
            pile.Unhighlight();
        }

        pile.Clear();
    }
}
