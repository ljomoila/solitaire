using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Linq;

public class GameManager : StateBase 
{
	public Camera MainCam;
	public SolitaireGame activeGame;
	public GameTime gameTime;

	public GameState state = GameState.Menu;

	public static GameManager Instance;

	private CardSelectionState cardSelectionState;	
	
	void Awake()
	{
		Instance = this;
	}

	void Start ()
	{
		Screen.sleepTimeout = (int) SleepTimeout.NeverSleep;
	}

	public IEnumerator Initialize(XDocument storedGameState)
    {
		yield return StartCoroutine(activeGame.Initialize());

		if (storedGameState == null)
		{
			gameTime.Time = float.Parse(storedGameState.Root.Element("time").Value);

			yield return StartCoroutine(activeGame.RestoreState(storedGameState));
		}
		else
		{
			yield return StartCoroutine(StartGame());
		}
	}

	public override void UpdateState ()
	{
		Time.timeScale = state == GameState.Paused ? 0 : 1;
	}

	// TODO move these to menu state?
	public void NewGame()
	{
		StartCoroutine(StartGame());
	}

	public void RestartGame()
	{		
		state = GameState.Restarting;

		// TODO deck seed
		
		StartCoroutine(StartGame());
	}

	public void Hint()
	{
		activeGame.HintRequest();
	}

	IEnumerator StartGame()
	{
		gameTime.Time = 0;

		GetComponent<CommandHistory>().Clear();

		state = GameState.Dealing;

		StateManager.Instance.ActivateState(activeGame.DealState);
		yield return null;

		if (cardSelectionState == null)
			cardSelectionState = new GameObject("CardSelectionState").AddComponent<CardSelectionState>();

		StateManager.Instance.ActivateState(cardSelectionState);

		state = GameState.Running;

		yield return null;
	}

	void StoreState()
	{
		string filename = Application.persistentDataPath + "/gameState.xml";
		
		XmlWriterSettings s = new XmlWriterSettings();
		s.Indent = true;
		s.NewLineOnAttributes = true;
		
		if (!activeGame.allPiles.Contains(activeGame.stock))
			activeGame.allPiles.Add(activeGame.stock);
		
		using (XmlWriter w = XmlWriter.Create(filename, s))
		{
			w.WriteStartDocument();
			w.WriteStartElement("game");
			
			string view = "menu";
			
			if (state == GameState.Running)
				view = "game";
			
			
			w.WriteElementString("time", gameTime.Time.ToString());
			w.WriteElementString("view", view);
			w.WriteElementString("type", activeGame.gameType.ToString());
			
			w.WriteStartElement("piles");
			
			foreach(CardPile p in activeGame.allPiles)
			{
				w.WriteStartElement("pile");
				w.WriteElementString("type", p.Type.ToString());
				
				if (p.cards.Count > 0)
				{
					w.WriteStartElement("cards");
					foreach(Card c in p.cards)
					{
						w.WriteStartElement("card");
						w.WriteElementString("suit", c.suit.ToString());
						w.WriteElementString("number", c.number.ToString());
						w.WriteElementString("turned", c.IsTurned().ToString());
						w.WriteElementString("position", XmlHelpers.ConvertVector3ToString(c.transform.localPosition));
						w.WriteEndElement();
					}
					w.WriteEndElement();
				}

				w.WriteEndElement();
			}

			w.WriteEndElement();
			w.WriteEndElement();
			w.WriteEndDocument();
		}
		
		if (activeGame.allPiles.Contains(activeGame.stock))
			activeGame.allPiles.Remove(activeGame.stock);
	}

	void OnApplicationQuit()
	{
		Debug.Log("OnApplicationQuit");

		StoreState();
	}

	#region CommandHistory
	public void StoreCommand(Cmd cmd)
	{
		GetComponent<CommandHistory>().StoreCommand(cmd);
	}

	public void Undo()
	{
		CommandHistory c = GetComponent<CommandHistory>();

		if (c.UndoDescription != "N/A")
		{
			c.Undo();
		}
	}

	public void Redo()
	{
		CommandHistory c = GetComponent<CommandHistory>();

		if (c.RedoDescription != "N/A")
		{
			c.Redo();
		}
	}
	#endregion

}

public enum GameState
{
	Running,
	Paused,
	GameOver,
	Menu,
	Dealing,
	Restarting	
}
