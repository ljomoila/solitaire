using UnityEngine;
using System.Collections;
using System.Xml.Linq;

public class GameManager : MonoBehaviour
{
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
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		commandHistory = GetComponent<CommandHistory>();

		StartCoroutine(Initialize());	
	}

	public IEnumerator Initialize()
    {
		XDocument storedGameState = StorageManager.Instance.LoadStoredState();

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

	IEnumerator StartGame()
	{
		gameTime.Time = 0;

		commandHistory.Clear();

		StateManager.Instance.ActivateState(activeGame.DealState);

		yield return null;

		ActivateCardSelectionState();

		yield return null;
	}

	IEnumerator RestoreGame(XDocument storedGameState)
	{
		gameTime.Time = float.Parse(storedGameState.Root.Element("time").Value);

		yield return StartCoroutine(activeGame.RestoreState(storedGameState));

		ActivateCardSelectionState();
	}

	private void ActivateCardSelectionState()
	{
		if (cardSelectionState == null)
			cardSelectionState = new GameObject("CardSelectionState").AddComponent<CardSelectionState>();

		StateManager.Instance.ActivateState(cardSelectionState);
	}

	void Update()
	{
		//Time.timeScale = state == GameState.Paused ? 0 : 1;
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
		if (hintState == null)
			hintState = new GameObject("HintState").AddComponent<HintState>();

		StateManager.Instance.ActivateState(hintState);
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
