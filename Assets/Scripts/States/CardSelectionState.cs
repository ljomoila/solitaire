using System.Collections.Generic;
using UnityEngine;

public class CardSelectionState : StateBase
{
    public CardPile Pile { get; set; }

    SolitaireGame activeGame;

    Vector3 startHit = Vector3.zero;

    private void Awake()
    {
        Pile = gameObject.AddComponent<CardPile>();
    }

    public void Initialize(List<Card> cards, Vector3 _startHit)
    {        
        transform.position = cards[0].transform.position;        
        Pile.sourcePile = cards[0].Pile;
        Pile.yStep = cards[0].Pile.yStep;

        int i = 0;
        foreach (Card card in cards)
        {
            Pile.AddCard(card);
            card.Pick(i);

            i++;
        }
        Pile.AlignCards();

        _startHit.y -= cards[0].transform.position.y;
        _startHit.x -= cards[0].transform.position.x;
        _startHit.z = 0;

        startHit = _startHit;
    }

    public override void OnActivateState()
    {
        this.activeGame = GameManager.Instance.activeGame;
    }

    public override void OnDeactivateState()
    {
        Pile.cards.Clear();
    }


    // Update is called once per frame
    public override void UpdateState()
    {
        if (Pile == null || Pile.IsEmpty) return;
        
        RaycastHit hit = new RaycastHit();
        Ray ray = GameManager.Instance.MainCam.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButton(0))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Vector3 pos = hit.point - startHit;
                pos.z = -0.5f;

                Pile.transform.position = Vector3.Lerp(Pile.transform.position, pos, Time.deltaTime * 10);
            }

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 9))
            {
                Card c = hit.collider.gameObject.GetComponent<Card>();

                if (c != null && c.Pile != null && c.Pile != Pile)
                {
                    CardPile pile = c.Pile;

                    for (int i = 0; i < activeGame.AllowedPiles.Count; i++)
                    {
                        if (activeGame.AllowedPiles[i] == pile)
                        {
                            pile.Highlight();
                        }
                        else
                        {
                            activeGame.AllowedPiles[i].Unhighlight();
                        }
                    }
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            AudioController.Play("cardSlide");
            bool successfullMove = false;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 9))
            {
                Card c = hit.collider.gameObject.GetComponent<Card>();

                if (c != null && c.Pile != null && c.Pile != Pile)
                {
                    CardPile toPile = c.Pile;

                    successfullMove = activeGame.TryMove(toPile, Pile.cards);
                }
            }
            if (!successfullMove)
                CancelMove();
            else
            {
                Pile.cards.Clear();
            }

            StateManager.Instance.ActivateState(previousState);
        }
        
    }

    void CancelMove()
    {
        CardPile toPile = Pile.sourcePile;

        Pile.cards[0].Leave();

        foreach (Card c in Pile.cards)
        {
            toPile.AddCard(c, .5f, 0);
        }

        toPile.AlignCards();

        foreach (CardPile pile in activeGame.AllowedPiles)
        {
            pile.Unhighlight();
        }

        Pile.Clear();
    }

}
