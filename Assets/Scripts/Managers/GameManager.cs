using UnityEngine;
using System.Collections;
using System.Xml.Linq;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour
{
	public List<Game> games;
	public GameType activeGameType;

	private Game activeGame;
	public Game ActiveGame
	{
		get { return activeGame; }
		set { activeGame = value; }
	}

	public Menu menu;

	public GameTime gameTime;

	public static GameManager Instance { get; private set; }

	void Awake()
	{
		Instance = this;
	}

	void Start ()
	{
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

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
			yield return StartCoroutine(BuildGame(storedGameState));			
    }

    private IEnumerator BuildGame(XDocument storedGameState)
    {
		GameType storedGameType = storedGameState != null ? (GameType)Enum.Parse(typeof(GameType), storedGameState.Root.Element("gameType").Value) : activeGameType;

        SetActiveGameByType();
        yield return null;

        yield return StartCoroutine(activeGame.Initialize());

        if (storedGameState != null && storedGameType == activeGameType)
            yield return StartCoroutine(activeGame.RestoreState(storedGameState));
        else
            yield return StartCoroutine(activeGame.DealNewCards());

		StateManager.Instance.ActivateState(activeGame);
	}

    internal void GameOver()
    {
		Debug.Log("Game over");

		StateManager.Instance.ActivateState(menu);
    }

    internal void Solved()
    {
		Debug.Log("Solved");

		StateManager.Instance.ActivateState(menu);
	}

    private void SetActiveGameByType()
    {
        foreach (Game game in games)
        {
            game.gameObject.SetActive(game.gameType == activeGameType);

            if (game.gameType == activeGameType)
                activeGame = game;
        }
    }

    void StartGame()
	{
		gameTime.Time = 0;

		CommandManager.Instance.Clear();

		StartCoroutine(activeGame.DealNewCards());
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
		ActiveGame.HintRequest();
	}
    #endregion
}

public enum GameState
{
	Solved,
	Playing,
	Paused
}