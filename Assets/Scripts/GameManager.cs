using UnityEngine;
using System.Collections;
using System.Xml;
using System.Xml.Linq;
using UnityEngine.UI;

public class GameManager : MonoBehaviour 
{
	public Camera MainCam;//, MenuCam, UICamera, AnimCamera;
	
	public DeckMaker deckMaker;
	
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
	void Start ()
	{
		Screen.sleepTimeout = (int) SleepTimeout.NeverSleep;

		StartCoroutine(StartRoutine());
	}

	IEnumerator StartRoutine()
	{
		yield return null;
		string viewType = "menu";
		XDocument previousState = LoadPreviousState();		

		if (previousState != null)
        {
			StartCoroutine(activeGame.RestoreState(previousState));

			viewType = previousState.Root.Element("view").Value;
			time = previousState != null ? float.Parse(previousState.Root.Element("time").Value) : 0;
			yield return null;
		}
		else if (viewType == "game")
        {
			state = GameState.Dealing;
			yield return StartCoroutine(activeGame.ShuffleAndDeal());
			state = GameState.Running;

		}
        else // menu
        {
            activeGame.MenuState();
        }

        // TODO sound settings from xml
        //AudioController.Play("gameMusic", MainCam.transform);

        yield return null;
    }

    XDocument LoadPreviousState()
    {
		string filename = Application.persistentDataPath + "/gameState.xml";

		if (System.IO.File.Exists(filename))
		{
			return XDocument.Load(filename);
		}

		return null;
	}
	
	bool backButtonPressed = false;
	
	// Update is called once per frame
	void Update ()
	{
		if (state == GameState.Running)
		{
			time += Time.deltaTime;
			timeTxt.text = GetTimeText();
		}
		
		if (state == GameState.Paused)
		{
			if (Time.timeScale == 1)
				Time.timeScale = 0;
		}
		else
		{
			if (Time.timeScale == 0)
				Time.timeScale = 1;
		}
		
		if ((Application.platform == RuntimePlatform.Android) || Application.platform == RuntimePlatform.OSXEditor)
		{
			if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.Escape))
			{
				backButtonPressed = true;
			}
		}
		
		if (backButtonPressed)
		{
			if (state == GameState.Menu)
				Application.Quit();
			else if (state == GameState.Running)
			{
				//StartCoroutine(ui.MenuRoutine(false));
			}
			
			backButtonPressed = false;
		}
	}

	//public SolitaireUI ui;

    Vector3 playCamPos, playCamRot;

	public void ShowMenu(GameObject go)
	{
		//if (ui.switchingView)
		//	return;

		//StartCoroutine(ui.MenuRoutine(go == null));
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
	
	public bool UICheck()
	{
		//Ray ray = UICamera.ScreenPointToRay (Input.mousePosition);
		//RaycastHit hit;		
		//if (Physics.Raycast (ray, out hit, Mathf.Infinity, 1 << 8)) 
		//{		
		//	return true;
		//} 
			
		
		return false;
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
