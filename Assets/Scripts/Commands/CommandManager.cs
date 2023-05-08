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
        if (commandHistory.UndoDescription == CommandHistory.NoCommandsStr)
            return;

        commandHistory.Undo();
    }

    public void Redo()
    {
        if (commandHistory.RedoDescription == CommandHistory.NoCommandsStr)
            return;

        commandHistory.Redo();
    }
}
