using UnityEngine;
using System.Collections;
using System.Xml.Linq;

public class GameManager : MonoBehaviour
{
	public Game activeGame;
	public Menu menu;
	public GameTime gameTime;

	private CommandHistory commandHistory;

	public static GameManager Instance { get; private set; }

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
		XDocument storedGameState = StorageManager.LoadStoredState();
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

	void StartGame()
	{
		gameTime.Time = 0;

		CommandManager.Instance.Clear();

		activeGame.DealNewCards();
	}

	void OnApplicationQuit()
	{
		Debug.Log("OnApplicationQuit");

		StorageManager.StoreState();
	}

	#region GameMenu

	public void NewGame()
	{
		StartGame();
	}

	public void RestartGame()
	{		
		// TODO deck seed
		
		StartGame();
	}

	public void Hint()
	{
		activeGame.HintRequest();
	}
    #endregion
}