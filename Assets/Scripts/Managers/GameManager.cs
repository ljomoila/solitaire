using UnityEngine;
using System.Collections;
using System.Xml.Linq;

public class GameManager : MonoBehaviour 
{
	public Camera MainCam;
	public SolitaireGame activeGame;
	public GameTime gameTime;

	public static GameManager Instance;

	private CardSelectionState cardSelectionState;
	private HintState hintState;

	private CommandHistory commandHistory;

	void Awake()
	{
		Instance = this;
	}

	void Start ()
	{
		commandHistory = GetComponent<CommandHistory>();

		Screen.sleepTimeout = (int) SleepTimeout.NeverSleep;

		StartCoroutine(Initialize(StorageManager.Instance.LoadStoredState()));
	}

	public IEnumerator Initialize(XDocument storedGameState)
    {
		yield return StartCoroutine(activeGame.Initialize());

		if (storedGameState != null)
		{
			yield return StartCoroutine(RestoreGame(storedGameState));
		}
		else
		{
			yield return StartCoroutine(StartGame());
		}
	}

    void Update ()
	{
		//Time.timeScale = state == GameState.Paused ? 0 : 1;
	}

	// TODO move these to menu state?
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
		if (hintState == null)
			hintState = new GameObject("HintState").AddComponent<HintState>();

		StateManager.Instance.ActivateState(hintState);
	}

	IEnumerator StartGame()
	{
		gameTime.Time = 0;

		commandHistory.Clear();

		StateManager.Instance.ActivateState(activeGame.DealState);

		yield return null;

		SetupCardSelectionState();

		yield return null;
	}

	IEnumerator RestoreGame(XDocument storedGameState)
	{
		gameTime.Time = float.Parse(storedGameState.Root.Element("time").Value);

		yield return StartCoroutine(activeGame.RestoreState(storedGameState));

		SetupCardSelectionState();
	}

	private void SetupCardSelectionState()
    {
        if (cardSelectionState == null)
			cardSelectionState = new GameObject("CardSelectionState").AddComponent<CardSelectionState>();

		StateManager.Instance.ActivateState(cardSelectionState);
    } 

	void OnApplicationQuit()
	{
		Debug.Log("OnApplicationQuit");

		StorageManager.Instance.StoreState();
	}

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
