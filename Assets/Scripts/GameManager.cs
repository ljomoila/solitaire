using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Linq;
using UnityEngine.UI;

public class GameManager : StateBase 
{
	public Camera MainCam;
	
	public SolitaireGame activeGame;
	
	public static GameManager Instance;

	public Text timeTxt;

	public GameState state = GameState.Menu;

	void Awake()
	{
		Instance = this;
	}
	
	public float time = 0;

	// Use this for initialization
	public override void Start ()
	{
		Screen.sleepTimeout = (int) SleepTimeout.NeverSleep;

		//NotificationCenter.DefaultCenter.AddObserver(this, GameEvents.OnStoredStateLoaded);
	}

	//void OnStoredStateLoaded(NotificationCenter.Notification n)
	//{

	//}

	public IEnumerator Initialize(XDocument storedGameState)
    {
		yield return null;
		string viewType = "menu";

		if (storedGameState == null)
		{
			viewType = storedGameState.Root.Element("view").Value;
			time = previousState != null ? float.Parse(storedGameState.Root.Element("time").Value) : 0;

			StartCoroutine(activeGame.RestoreState(storedGameState));

			yield return null;
		}
		else
		{
			yield return StartCoroutine(activeGame.ShuffleAndDeal());
		}

		state = GameState.Running;
	}
	
	// Update is called once per frame
	public override void UpdateState ()
	{
		Time.timeScale = state == GameState.Paused ? 0 : 1;

		if (state == GameState.Running)
		{
			time += Time.deltaTime;
			timeTxt.text = GetTimeText();
		}
	}


	public void NewGame()
	{
		StartCoroutine(StartGame());
	}

	
	public void RestartGame()
	{
		if (state != GameState.Running)
			return;
		
		state = GameState.Restarting;
		
		StartCoroutine(StartGame());
	}
	
	IEnumerator StartGame()
	{
		bool restart = state == GameState.Restarting;
		time = 0;		
		state = GameState.Dealing;

		activeGame.GatherDeck();
		activeGame.stock.rnd = new Random();

		GetComponent<CommandHistory>().Clear();
		
		yield return StartCoroutine(activeGame.ShuffleAndDeal());
		
		state = GameState.Running;
	}
	
	public void Hint()
	{
		activeGame.HintRequest();
	}
	
	void OnApplicationQuit()
	{
		Debug.Log("OnApplicationQuit");
		
		StoreState();
	}
	
	void StoreState()
	{
		string filename = Application.persistentDataPath + "/gameState.xml";
		
		Debug.Log("StoreState to: "+filename);
		
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
			
			
			w.WriteElementString("time", time.ToString());
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
	
	public string GetTimeText()
	{
		System.TimeSpan ts = System.TimeSpan.FromSeconds(time);
		
		string timeStr = System.String.Format("{0:D2}:{1:D2}", ts.Minutes, ts.Seconds);
		
		if (ts.Hours > 0)
		{
			timeStr = System.String.Format("{0:D2}:{1:D2}:{2:D2}", ts.Hours, ts.Minutes, ts.Seconds);
		}
		
		return timeStr;		
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
