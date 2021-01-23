using System.Collections.Generic;
using UnityEngine;

public class CardSelectionState : StateBase
{
    public SelectionPile Pile { get; set; }

    private SolitaireGame activeGame;
    private Vector3 startHit = Vector3.zero;
    private float pileDraggingFactor = 10;

    private void Awake()
    {
        Pile = gameObject.AddComponent<SelectionPile>();
    }

    public override void OnActivateState()
    {
        this.activeGame = GameManager.Instance.activeGame;
    }

    public override void OnDeactivateState()
    {
        Pile.cards.Clear();
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
				
	            SetSelection(activeGame.Select(c));
			}
		}

        if (Pile == null || Pile.IsEmpty) return;

        if (Input.GetMouseButton(0)) // dragging pile
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Vector3 pos = hit.point - startHit;
                pos.z = -0.5f;

                Pile.transform.position = Vector3.Lerp(Pile.transform.position, pos, Time.deltaTime * pileDraggingFactor);
            }

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 9))
            {
                Card c = hit.collider.gameObject.GetComponent<Card>();

                if (c != null && c.pile != null && c.pile != Pile)
                {
                    CardPile pile = c.pile;
                    
                    foreach(CardPile p in activeGame.Piles)
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

                if (c != null && c.pile != null && c.pile != Pile)
                    successfullMove = activeGame.TryMove(c.pile, Pile);
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

        transform.position = cards[0].transform.position;      
        Pile.sourcePile = cards[0].pile;
        Pile.yStep = cards[0].pile.yStep;

        int i = 0;
        foreach (Card card in cards)
        {
            Pile.AddCard(card);
            card.Pick(i);

            i++;
        }

        Pile.AlignCards();

        startHit.y -= cards[0].transform.position.y;
        startHit.x -= cards[0].transform.position.x;
        startHit.z = 0;

        AudioController.Play("cardSlide");
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

        Reset();
    }

    void Reset() 
    {
        foreach (CardPile pile in activeGame.Piles)
        {
            pile.Unhighlight();
        }

        Pile.Clear();

        AudioController.Play("cardSlide");
    }

}
