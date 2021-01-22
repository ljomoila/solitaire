using System.Collections.Generic;
using UnityEngine;

public class CardSelectionState : StateBase
{
    public SelectionPile Pile { get; set; }

    private SolitaireGame activeGame;
    private Vector3 startHit = Vector3.zero;
    private float pileDraggingSpeed = 10;

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


    // Update is called once per frame
    public override void UpdateState()
    {
        RaycastHit hit = new RaycastHit();
        Ray ray = GameManager.Instance.MainCam.ScreenPointToRay(Input.mousePosition);

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

                Pile.transform.position = Vector3.Lerp(Pile.transform.position, pos, Time.deltaTime * pileDraggingSpeed);
            }

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << 9))
            {
                Card c = hit.collider.gameObject.GetComponent<Card>();

                if (c != null && c.Pile != null && c.Pile != Pile)
                {
                    CardPile pile = c.Pile;
                    
                    foreach(CardPile p in activeGame.AllowedPiles)
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

                if (c != null && c.Pile != null && c.Pile != Pile)
                    successfullMove = activeGame.TryMove(c.Pile, Pile);
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
        foreach (CardPile pile in activeGame.AllowedPiles)
        {
            pile.Unhighlight();
        }

        Pile.Clear();

        AudioController.Play("cardSlide");
    }

}
