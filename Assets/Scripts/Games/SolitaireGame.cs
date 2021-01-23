
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class SolitaireGame : MonoBehaviour
{
	public GameType gameType;

	public CardPile stock;
	public CardPile Waste { get; set; } = new CardPile();

	public List<CardPile> Piles { get; set; } = new List<CardPile>();
	public List<CardPile> TableauPiles { get; set; } = new List<CardPile>();

	public DealState DealState { get; set; } = null;

	public int stockDrawAmount = 3;

	public List<Cmd> commands = new List<Cmd>();

	public virtual IEnumerator Initialize()
	{
		yield return null;
	}

	public virtual IEnumerator RestoreState(XDocument xdoc)
	{
		yield return null;
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
