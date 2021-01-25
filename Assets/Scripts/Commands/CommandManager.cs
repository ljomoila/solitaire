using UnityEngine;

public class CommandManager : MonoBehaviour
{
    private CommandHistory commandHistory;

	public static CommandManager Instance { get; private set; }

	private void Awake()
    {
		Instance = this;

        commandHistory = gameObject.AddComponent<CommandHistory>();
    }

    public void Clear()
    {
        commandHistory.Clear();
    }

	public void StoreCommand(Cmd cmd)
	{
		commandHistory.StoreCommand(cmd);
	}

	public void Undo()
	{
		if (commandHistory.UndoDescription == "N/A")
			return;

		commandHistory.Undo();		
	}

	public void Redo()
	{
		if (commandHistory.RedoDescription == "N/A")
			return;

		commandHistory.Redo();		
	}
}
