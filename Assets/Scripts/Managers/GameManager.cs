using UnityEngine;
using System.Collections;
using System.Xml.Linq;

public class GameManager : MonoBehaviour
{
	public Game activeGame;
	public Menu menu;
	public GameTime gameTime;

	public static GameManager Instance;

	private CommandHistory commandHistory;

	void Awake()
	{
		Instance = this;
	}

	void Start ()
	{
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		commandHistory = GetComponent<CommandHistory>();

		StartCoroutine(Initialize());	
	}

	public IEnumerator Initialize()
    {
		XDocument storedGameState = StorageManager.Instance.LoadStoredState();
		yield return null;

		string viewType = "game";// storedGameState != null ? storedGameState.Root.Element("view").Value : "game";

		if (viewType.Equals("menu"))
			StateManager.Instance.ActivateState(menu);
		else if (viewType.Equals("game"))
        {
			yield return StartCoroutine(activeGame.Initialize(storedGameState));

			StateManager.Instance.ActivateState(activeGame);			
		}
    }

	IEnumerator StartGame()
	{
		gameTime.Time = 0;

		commandHistory.Clear();

		yield return StartCoroutine(activeGame.DoDeal());

		StateManager.Instance.ActivateState(activeGame);
	}

	void OnApplicationQuit()
	{
		Debug.Log("OnApplicationQuit");

		StorageManager.Instance.StoreState();
	}

	#region GameMenu

	public void NewGame()
	{
		StartCoroutine(StartGame());
	}

	public void RestartGame()
	{		
		// TODO deck seed
		
		StartCoroutine(StartGame());
	}

	public void Hint()
	{
		activeGame.HintRequest();
	}

    #endregion

	#region CommandHistory
	public void StoreCommand(Cmd cmd)
	{
		commandHistory.StoreCommand(cmd);
	}

	public void Undo()
	{
		if (commandHistory.UndoDescription != "N/A")
		{
			commandHistory.Undo();
		}
	}

	public void Redo()
	{
		if (commandHistory.RedoDescription != "N/A")
		{
			commandHistory.Redo();
		}
	}
	#endregion
}