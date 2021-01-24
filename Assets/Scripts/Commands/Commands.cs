using UnityEngine;
using System.Collections.Generic;

public class CmdDrawCards : Cmd
{
    int drawAmount = 0;

    CardPile stock = null;
    CardPile waste = null;

    bool turnStock = false;

    public CmdDrawCards(Game game, int drawAmount, string desc = ""):base(desc)
    {
        this.drawAmount = drawAmount;

        stock = game.stock;
        waste = game.Waste;
        turnStock = CollectCardPickInfo() ? false : true;
    }

    List<PickedCard> cardInfos = new List<PickedCard>();

    private bool CollectCardPickInfo()
    {
        bool cardsOnStock = false;

        cardInfos.Clear();

        if (stock.cards.Count > 0)
        {                
            for (int i = 0; i < drawAmount; i++)
            {
                if (stock.cards.Count - i <= 0)
                {
                    break;
                }
                
                Card pickedCard = stock.cards[stock.cards.Count-1 - i];
                PickedCard pc = new PickedCard() { Card = pickedCard };
                cardInfos.Add(pc);
            }

            cardsOnStock = true;
        }
        else
        {                
            cardsOnStock = false;
        }

        return cardsOnStock;
    }

    public override void Execute(bool immediate = true)
    {
        if (turnStock)
        {
            TurnStock();
        }
        else
        {
            MoveAndTurnCards(immediate);
        }
    }

    public override void Unexecute()
    {
        if (turnStock)
        {
            UnturnStock();
        }
        else
        {
            MoveAndTurnCardsBack();
        }
    }

    private void MoveAndTurnCards(bool immediate)
    {
		waste.AlignCards();
		
		Vector2 nextPos = waste.NextPos = Vector2.zero;
        float t = .25f;
		
		int i = cardInfos.Count;
		
        foreach (PickedCard cardInfo in cardInfos)
        {
            Card card = cardInfo.Card;

            stock.RemoveCard(card);
			
			if (!immediate)
			{
	        	waste.AddCard(card, t, t);
				card.Turn(false, t, i);
			}
			else
			{
				waste.AddCard(card);
				card.transform.localPosition = new Vector3(nextPos.x, nextPos.y, waste.CardZ);
				card.Turn(false);
				
				if (!immediate)
					AudioController.Play("cardTurn");
			}

			nextPos.x += .5f;

            waste.NextPos = nextPos;
			waste.CardZ -= .05f;
			
			i--;
        }
    }

    private void MoveAndTurnCardsBack()
    {
        for (int i = cardInfos.Count-1; i >= 0; i--)
		{
            Card card = cardInfos[i].Card;

            waste.RemoveCard(card);
            stock.AddCard(card);
			
			card.Turn(true);

            stock.AlignCards();
            waste.AlignCards();
        }

        foreach (PickedCard cardInfo in cardInfos)
        {
            cardInfo.Card.Turn(true);
        }
		
		// TODO sound?
    }

    private void TurnStock()
    {
        Debug.Log("TurnStock");

        List<Card> wasteCards = new List<Card>(waste.cards);
        wasteCards.Reverse();

        foreach (Card card in wasteCards)
        {
            waste.RemoveCard(card);
            stock.AddCard(card);

            card.Turn(true);
        }
		
		AudioController.Play("cardSlide");

        stock.AlignCards();
    }

    private void UnturnStock()
    {
        List<Card> stockCards = new List<Card>(stock.cards);
        stockCards.Reverse();

        foreach (Card card in stockCards)
        {
            stock.RemoveCard(card);
            waste.AddCard(card);

            card.Turn(false);
        }

		AudioController.Play("cardSlide");

        waste.AlignCards();
    }

}

class PickedCard
{
    public Card Card = null;
}


public class CmdMoveCards : Cmd
{
	CardPile fromPile;
	CardPile toPile;
	List<Card> cards;
	
	public CmdMoveCards(List<Card> cards, CardPile fromPile, CardPile toPile, string desc = "") :base(desc)
	{
		this.toPile = toPile;
		this.fromPile = fromPile;
		this.cards = cards;

        GameManager.Instance.StoreCommand(this);
    }
	
	public override void Execute (bool immediate = true)
	{
        float t = immediate ? 0 : .5f;

        foreach (Card c in cards)
		{
			toPile.AddCard(c, t, 0);
			fromPile.RemoveCard(c);
			
			if (!immediate)
				AudioController.Play("cardSlide");
		}
		
		AlignPiles();
	}
	
	public override void Unexecute ()
	{
		int i = 0;
		
		foreach(Card c in cards)
		{
			fromPile.AddCard(c);
			toPile.RemoveCard(c);
			
			cards[i] = c;
		}

		AlignPiles ();
	}
	
	void AlignPiles()
	{
		fromPile.AlignCards();
		toPile.AlignCards();
	}
}

public class CmdTurnCard : Cmd
{
    List<Card> cards = null;
    bool directionDown = false;

    public bool Animate = false;
    public double animateStart = 0;        

    public CmdTurnCard(Card card, string desc = "") :base(desc)
    {
        cards = new List<Card>();
        cards.Add(card);
    }

    public CmdTurnCard(List<Card> cards, string desc = "") :base(desc)
    {
        this.cards = new List<Card>(cards);
    }

    public CmdTurnCard(List<Card> movedCards, bool b)
        :this(movedCards)
    {
        directionDown = b;
    }

    public override void Execute(bool immediate = true)
    {
        Set(directionDown, immediate);
    }

    public void Set(bool turnedValue, bool immediate)
    {
		float t = immediate ? 0 : .5f;

        foreach (Card card in cards)
        {
			card.Turn(turnedValue, t);
        }
    }

    public override void Unexecute()
    {
        Set(!directionDown, true);
    }
}

#region Cmd & CommandHistory classes

///	<summary>
/// Base class for command objects.
///	Command is used to perform an operation
///	or series of operations.
/// Command can save it's data for undo.
/// </summary>	
public class Cmd
{
    private string m_desc;
    private object m_tag = null;

    /// <summary>
    /// Gets or sets commands description.
    /// </summary>
    public virtual string Description
    {
        get { return m_desc; }
        set { m_desc = value; }
    }

    /// <summary>
    /// Gets or sets tag to command.
    /// Tags type is object.
    /// </summary>
    public virtual object Tag
    {
        get { return m_tag; }
        set { m_tag = value; }
    }

    /// <summary>
    /// Creates a command.
    /// </summary>
    /// <param name="desc">Commands description.</param>
    protected Cmd(string desc)
    {
        Description = desc;
    }

    /// <summary>
    /// Executes command.
    /// </summary>
    public virtual void Execute(bool immediate = true) { }

    /// <summary>
    /// Unexecutes command.
    /// </summary>
	public virtual void Unexecute() { }

    /// <summary>
    /// Command description.
    /// </summary>
    /// <returns>Command description</returns>
    public override string ToString()
    {
        return Description;
    }

}

//public delegate void CmdCallback(Cmd command);


/// <summary>
/// CommandComposite includes a list of commands.
/// Commands in a list are executed in adding order.
/// On unexecute commands are unexecuted in reverse order.
/// </summary>
/// 
public class CmdComposite : Cmd
{
    #region Fields

    private List<Cmd> m_commands;
    private List<Cmd> m_reversed;

    #endregion  // Fields

    #region Properties

    /// <summary>
    /// Commands.
    /// </summary>
    public List<Cmd> Commands
    {
        get { return m_commands; }
    }

    /// <summary>
    /// Commands in reverse order.
    /// </summary>
    public List<Cmd> Reversed
    {
        get { return m_reversed; }
    }

    #endregion  // Properties

    #region Methods

    /// <summary>
    /// Creates a CompositeCommand.
    /// </summary>
    public CmdComposite(string desc)
        : base(desc)
    {
        m_commands = new List<Cmd>();
    }

    /// <summary>
    /// Creates a CompositeCommand from list of commands.
    /// </summary>
    public CmdComposite(List<Cmd> commands, string desc = "")
        : this(desc)
    {
        foreach (Cmd cmd in commands)
        {
            this.Add(cmd);
        }
    }

    /// <summary>
    /// Adds a command to macro command.
    /// </summary>
    /// <param name="cmd"></param>
    public virtual void Add(Cmd cmd)
    {
        Commands.Add(cmd);
    }

    /// <summary>
    /// Adds commands to CompositeCommand.
    /// </summary>
    /// <param name="lstCmds"></param>
    public virtual void Add(List<Cmd> lstCmds)
    {
        foreach (Cmd cmd in lstCmds)
        {
            Add(cmd);
        }
    }

    /// <summary>
    /// Executes a command.
    /// </summary>
    public override void Execute(bool immediate = true)
    {

        // TODO Siirsin unexecuteen

        //m_reversed = new List<Cmd>();

        //m_reversed.AddRange(Commands);

        //m_reversed.Reverse();

        //            if (BeforeExecute != null)
        //                BeforeExecute(this);

        foreach (Cmd cmd in Commands)
        {
            cmd.Execute(immediate);
        }

        //            if (AfterExecute != null)
        //                AfterExecute(this);
    }

    public override void Unexecute()
    {
        // Reversed
        for (int i = Commands.Count - 1; i >= 0; i--)
        {
            Commands[i].Unexecute();
        }


    }

    /// <summary>
    /// Unexecutes a command.
    /// </summary>
    /*public override void Unexecute()
    {
        m_reversed = new List<Cmd>();
        m_reversed.AddRange(Commands);

        m_reversed.Reverse();

//            if (BeforeUnexecute != null)
//                BeforeUnexecute(this);

        foreach (Cmd cmd in Reversed)
        {
            cmd.Unexecute();
        }

//            if (AfterUnexecute != null)
//                AfterUnexecute(this);
    }*/

    #endregion  // Methods
}

#endregion
